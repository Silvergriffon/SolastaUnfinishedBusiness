﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static ActionDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Models.SpellsContext;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class CircleOfTheWildfire : AbstractSubclass
{
    private const string Name = "CircleOfTheWildfire";
    private const string SpiritName = "WildfireSpirit";
    private const string ConditionCommandSpirit = $"Condition{Name}Command";

    private static readonly EffectProxyDefinition EffectProxyCauterizingFlames = EffectProxyDefinitionBuilder
        .Create(EffectProxyDefinitions.ProxyDancingLights, $"Proxy{Name}CauterizingFlames")
        .SetOrUpdateGuiPresentation($"Power{Name}SummonCauterizingFlames", Category.Feature)
        .SetCanMove(false, false)
        .AddToDB();

    private static readonly FeatureDefinitionPower PowerCauterizingFlames =
        FeatureDefinitionPowerBuilder
            .Create($"Power{Name}CauterizingFlames")
            .SetGuiPresentationNoContent(true)
            .SetUsesProficiencyBonus(ActivationTime.NoCost)
            .AddToDB();

    public CircleOfTheWildfire()
    {
        //
        // LEVEL 03
        //

        var autoPreparedSpellsWildfire = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation("ExpandedSpells", Category.Feature)
            .SetAutoTag("Circle")
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, BurningHands, CureWounds),
                BuildSpellGroup(3, FlamingSphere, ScorchingRay),
                BuildSpellGroup(5, AshardalonStride, Revivify),
                BuildSpellGroup(7, AuraOfLife, FireShield),
                BuildSpellGroup(9, FlameStrike, MassCureWounds))
            .SetSpellcastingClass(CharacterClassDefinitions.Druid)
            .AddToDB();

        //
        // Summon Spirit
        //

        var powerSpiritTeleport = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}SpiritTeleport")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.All, RangeType.Distance, 3, TargetType.Position)
                    .InviteOptionalAlly()
                    .SetSavingThrowData(true, AttributeDefinitions.Wisdom, false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .SetMotionForm(MotionForm.MotionType.TeleportToDestination, 1)
                            .Build())
                    .SetParticleEffectParameters(DimensionDoor)
                    .Build())
            .AddCustomSubFeatures(new ModifyTeleportEffectBehaviorSpiritTeleport())
            .AddToDB();

        powerSpiritTeleport.EffectDescription.EffectParticleParameters.targetParticleReference = new AssetReference();

        var actionAffinityEldritchCannon =
            FeatureDefinitionActionAffinityBuilder
                .Create($"ActionAffinity{Name}Spirit")
                .SetGuiPresentationNoContent(true)
                .SetForbiddenActions(
                    Id.AttackMain, Id.AttackOff, Id.AttackFree, Id.AttackReadied, Id.AttackOpportunity, Id.Ready,
                    Id.PowerMain, Id.PowerBonus, Id.PowerReaction, Id.SpendPower, Id.Shove, Id.ShoveBonus, Id.ShoveFree)
                .AddCustomSubFeatures(new SummonerHasConditionOrKOd(), ForceInitiativeToSummoner.Mark)
                .AddToDB();

        var acBonus = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}ArmorClass")
            .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
            .SetAddConditionAmount(AttributeDefinitions.ArmorClass)
            .AddToDB();

        var toHit = FeatureDefinitionAttackModifierBuilder
            .Create($"AttackModifier{Name}AttackRoll")
            .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
            .SetAttackRollModifier(1, AttackModifierMethod.SourceConditionAmount)
            .AddToDB();

        var toDamage = FeatureDefinitionAttackModifierBuilder
            .Create($"AttackModifier{Name}DamageRoll")
            .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
            .SetDamageRollModifier(1, AttackModifierMethod.SourceConditionAmount)
            .AddToDB();

        var hpBonus = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}HitPoints")
            .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
            .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.AddConditionAmount,
                AttributeDefinitions.HitPoints)
            .AddToDB();

        var summoningAffinitySpirit = FeatureDefinitionSummoningAffinityBuilder
            .Create($"SummoningAffinity{Name}Spirit")
            .SetGuiPresentationNoContent(true)
            .SetRequiredMonsterTag(SpiritName)
            .SetAddedConditions(
                ConditionDefinitionBuilder
                    .Create($"Condition{Name}SpiritArmorClass")
                    .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
                    .SetPossessive()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyAndAbilityBonus, AttributeDefinitions.Wisdom)
                    .SetFeatures(acBonus)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create($"Condition{Name}SpiritAttackRoll")
                    .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
                    .SetPossessive()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ConditionDefinition.OriginOfAmount.SourceSpellAttack)
                    .SetFeatures(toHit)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create($"Condition{Name}SpiritDamageRoll")
                    .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
                    .SetPossessive()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceProficiencyBonus)
                    .SetFeatures(toDamage)
                    .AddToDB(),
                ConditionDefinitionBuilder
                    .Create($"Condition{Name}SpiritHitPoints")
                    .SetGuiPresentation("Feedback/&SpiritBonusTitle", Gui.NoLocalization)
                    .SetPossessive()
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetAmountOrigin(ExtraOriginOfAmount.SourceClassLevel, DruidClass)
                    .SetFeatures(hpBonus, hpBonus, hpBonus, hpBonus, hpBonus)
                    .AddToDB())
            .AddToDB();

        var monsterDefinitionSpirit = MonsterDefinitionBuilder
            .Create(MonsterDefinitions.Fire_Elemental, "WildfireSpirit")
            .SetOrUpdateGuiPresentation(Category.Monster)
            .SetSizeDefinition(CharacterSizeDefinitions.Small)
            .SetMonsterPresentation(
                MonsterPresentationBuilder
                    .Create()
                    .SetAllPrefab(MonsterDefinitions.Fire_Elemental.MonsterPresentation)
                    .SetPhantom()
                    .SetModelScale(0.3f)
                    .SetHasMonsterPortraitBackground(true)
                    .SetCanGeneratePortrait(true)
                    .Build())
            .SetCreatureTags(SpiritName)
            .SetStandardHitPoints(1)
            .SetHeight(2)
            .NoExperienceGain()
            .SetArmorClass(13)
            .SetChallengeRating(0)
            .SetHitDice(DieType.D8, 1)
            .SetAbilityScores(10, 14, 14, 13, 15, 11)
            .SetDefaultFaction(FactionDefinitions.Party)
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .SetFullyControlledWhenAllied(true)
            .SetDungeonMakerPresence(MonsterDefinition.DungeonMaker.None)
            .ClearAttackIterations()
            .SetFeatures(
                actionAffinityEldritchCannon,
                powerSpiritTeleport,
                FeatureDefinitionMoveModes.MoveModeMove6,
                FeatureDefinitionMoveModes.MoveModeFly6,
                FeatureDefinitionDamageAffinitys.DamageAffinityFireImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityCharmImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityFrightenedImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityProneImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityRestrainedmmunity,
                FeatureDefinitionSenses.SenseDarkvision)
            .AddToDB();

        // Command Spirit

        var conditionCommandSpirit = ConditionDefinitionBuilder
            .Create(ConditionCommandSpirit)
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddToDB();

        var powerCommandSpirit = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}CommandSpirit")
            .SetGuiPresentation(Category.Feature, Command)
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionCommandSpirit, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(ValidatorsValidatePowerUse.InCombat,
                new ValidatorsValidatePowerUse(x => HasSpirit(x.Guid)))
            .AddToDB();

        powerCommandSpirit.AddCustomSubFeatures(
            new CharacterBeforeTurnEndListenerCommandSpirit(
                conditionCommandSpirit,
                powerCommandSpirit));

        // Summon Spirit

        var powerSummonSpirit = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"PowerSharedPool{Name}SummonSpirit")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.Action, PowerDruidWildShape)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.HalfClassLevelHours)
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.Position)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetSummonCreatureForm(1, monsterDefinitionSpirit.Name)
                            .Build())
                    .SetParticleEffectParameters(PowerDruidWildShape)
                    .Build())
            .AddToDB();

        var featureSetSummonSpirit = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}SummonSpirit")
            .SetGuiPresentation($"PowerSharedPool{Name}SummonSpirit", Category.Feature)
            .SetFeatureSet(
                summoningAffinitySpirit,
                powerCommandSpirit,
                powerSummonSpirit)
            .AddToDB();

        //
        // LEVEL 06 - Enhanced Bond
        //

        var featureEnhancedBond = FeatureDefinitionBuilder
            .Create($"Feature{Name}EnhancedBond")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureEnhancedBond.AddCustomSubFeatures(
            new MagicEffectBeforeHitConfirmedOnEnemyEnhancedBond(featureEnhancedBond));

        //
        // LEVEL 10 - Cauterizing Flames
        //

        EffectProxyCauterizingFlames.actionId = Id.NoAction;
        EffectProxyCauterizingFlames.addLightSource = false;

        var powerSummonCauterizingFlames = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}SummonCauterizingFlames")
            .SetGuiPresentation(Category.Feature, hidden: true)
            .SetUsesFixed(ActivationTime.NoCost)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.All, RangeType.Distance, 6, TargetType.Position)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetSummonEffectProxyForm(EffectProxyCauterizingFlames)
                            .Build())
                    .Build())
            .AddToDB();

        powerSummonCauterizingFlames.AddCustomSubFeatures(
            new OnReducedToZeroHpByMeOrAllySummonCauterizingFlames(powerSummonCauterizingFlames));

        var featureSetCauterizingFlames = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}CauterizingFlames")
            .SetGuiPresentation($"Power{Name}SummonCauterizingFlames", Category.Feature)
            .SetFeatureSet(powerSummonCauterizingFlames, PowerCauterizingFlames)
            .AddToDB();

        //
        // LEVEL 14 - Blazing Revival
        //

        var featureBlazingRevival = FeatureDefinitionBuilder
            .Create($"Feature{Name}BlazingRevival")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureBlazingRevival.AddCustomSubFeatures(
            new OnReducedToZeroHpByEnemyBlazingRevival());

        //
        // MAIN
        //

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.PatronElementalist, 256))
            .AddFeaturesAtLevel(2, autoPreparedSpellsWildfire, featureSetSummonSpirit)
            .AddFeaturesAtLevel(6, featureEnhancedBond)
            .AddFeaturesAtLevel(10, featureSetCauterizingFlames)
            .AddFeaturesAtLevel(14, featureBlazingRevival)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Druid;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceDruidCircle;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private static GameLocationCharacter GetMySpirit(ulong guid)
    {
        var locationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();
        var mySpirit = locationCharacterService.GuestCharacters
            .FirstOrDefault(x =>
                x.RulesetCharacter is RulesetCharacterMonster rulesetCharacterMonster &&
                rulesetCharacterMonster.MonsterDefinition.CreatureTags.Contains(SpiritName) &&
                rulesetCharacterMonster.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagConjure, ConditionConjuredCreature, out var conjured) &&
                conjured.SourceGuid == guid);

        return mySpirit;
    }

    private static bool HasSpirit(ulong guid)
    {
        return GetMySpirit(guid) != null;
    }

    //
    // called from GLBM when a character's move ends. handles spirit flames behavior
    //

    internal static IEnumerator ProcessOnCharacterMoveEnd(
        GameLocationBattleManager battleManager,
        GameLocationCharacter mover)
    {
        var power = PowerCauterizingFlames;
        var locationCharacterService = ServiceRepository.GetService<IGameLocationCharacterService>();
        var cauterizingFlamesProxies = locationCharacterService.AllProxyCharacters
            .Where(u =>
                mover.IsWithinRange(u, 1) &&
                u.RulesetActor is RulesetCharacterEffectProxy rulesetCharacterEffectProxy &&
                rulesetCharacterEffectProxy.EffectProxyDefinition == EffectProxyCauterizingFlames)
            .ToList(); // avoid changing enumerator

        foreach (var cauterizingFlamesProxy in cauterizingFlamesProxies)
        {
            var rulesetProxy = cauterizingFlamesProxy.RulesetActor as RulesetCharacterEffectProxy;
            var rulesetSource = EffectHelpers.GetCharacterByGuid(rulesetProxy!.ControllerGuid);
            var usablePower = PowerProvider.Get(power, rulesetSource);
            var source = GameLocationCharacter.GetFromActor(rulesetSource);

            if (source == null ||
                !source.CanReact() ||
                rulesetSource.GetRemainingUsesOfPower(usablePower) == 0)
            {
                continue;
            }

            var actionService = ServiceRepository.GetService<IGameLocationActionService>();
            var implementationManager =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            var actionParams = new CharacterActionParams(source, Id.PowerReaction)
            {
                StringParameter = mover.Side == Side.Enemy ? "CauterizingFlamesDamage" : "CauterizingFlamesHeal",
                ActionModifiers = { new ActionModifier() },
                RulesetEffect = implementationManager
                    .MyInstantiateEffectPower(rulesetSource, usablePower, false),
                UsablePower = usablePower,
                TargetCharacters = { mover }
            };

            var count = actionService.PendingReactionRequestGroups.Count;

            actionService.ReactToUsePower(actionParams, "UsePower", source);

            yield return battleManager.WaitForReactions(mover, actionService, count);

            if (!actionParams.ReactionValidated)
            {
                yield break;
            }

            var powerToTerminate = rulesetSource.PowersUsedByMe.FirstOrDefault(x =>
                x.Guid == rulesetProxy.EffectGuid);

            if (powerToTerminate != null)
            {
                rulesetSource.TerminatePower(powerToTerminate);
            }

            var wisdom = rulesetSource.TryGetAttributeValue(AttributeDefinitions.Wisdom);
            var wisMod = AttributeDefinitions.ComputeAbilityScoreModifier(wisdom);
            var rulesetMover = mover.RulesetCharacter;

            if (mover.Side == Side.Enemy)
            {
                var rolls = new List<int>();
                var damageForm = new DamageForm
                {
                    DamageType = DamageTypeFire, DieType = DieType.D10, DiceNumber = 2, BonusDamage = wisMod
                };
                var damageRoll = rulesetSource.RollDamage(
                    damageForm, 0, false, 0, 0, 1, false, false, false, rolls);

                var applyFormsParams = new RulesetImplementationDefinitions.ApplyFormsParams
                {
                    sourceCharacter = rulesetSource,
                    targetCharacter = rulesetMover,
                    position = mover.LocationPosition
                };

                RulesetActor.InflictDamage(
                    damageRoll,
                    damageForm,
                    damageForm.DamageType,
                    applyFormsParams,
                    rulesetMover,
                    false,
                    rulesetSource.Guid,
                    false,
                    [],
                    new RollInfo(damageForm.DieType, [], damageForm.BonusDamage),
                    true,
                    out _);
            }
            else
            {
                var dieRoll =
                    rulesetSource.RollDie(DieType.D10, RollContext.None, false, AdvantageType.None, out _, out _);

                rulesetMover.ReceiveHealing(dieRoll + wisMod, true, rulesetSource.Guid);
            }
        }
    }

    private sealed class SummonerHasConditionOrKOd : IValidateDefinitionApplication, ICharacterTurnStartListener
    {
        public void OnCharacterTurnStarted(GameLocationCharacter locationCharacter)
        {
            // if commanded allow anything
            if (IsCommanded(locationCharacter.RulesetCharacter))
            {
                return;
            }

            // if not commanded it cannot move
            locationCharacter.usedTacticalMoves = locationCharacter.MaxTacticalMoves;

            // or use powers so force the dodge action
            ServiceRepository.GetService<ICommandService>()?
                .ExecuteAction(new CharacterActionParams(locationCharacter, Id.Dodge), null, false);
        }

        public bool IsValid(BaseDefinition definition, RulesetCharacter character)
        {
            //Apply limits if not commanded
            return !IsCommanded(character);
        }

        private static bool IsCommanded(RulesetCharacter character)
        {
            //can act freely outside of battle
            if (Gui.Battle == null)
            {
                return true;
            }

            var summoner = character.GetMySummoner()?.RulesetCharacter;

            //shouldn't happen, but consider being commanded in this case
            if (summoner == null)
            {
                return true;
            }

            //can act if summoner is KO
            return summoner.IsUnconscious ||
                   //can act if summoner commanded
                   summoner.HasConditionOfType(ConditionCommandSpirit);
        }
    }

    // Command Spirit

    private sealed class CharacterBeforeTurnEndListenerCommandSpirit(
        ConditionDefinition conditionEldritchCannonCommand,
        FeatureDefinitionPower power) : ICharacterBeforeTurnEndListener
    {
        public void OnCharacterBeforeTurnEnded(GameLocationCharacter locationCharacter)
        {
            var status = locationCharacter.GetActionStatus(Id.PowerBonus, ActionScope.Battle);

            if (status != ActionStatus.Available ||
                !HasSpirit(locationCharacter.Guid))
            {
                return;
            }

            var rulesetCharacter = locationCharacter.RulesetCharacter;

            rulesetCharacter.LogCharacterUsedPower(power);
            rulesetCharacter.InflictCondition(
                conditionEldritchCannonCommand.Name,
                DurationType.Round,
                1,
                TurnOccurenceType.StartOfTurn,
                AttributeDefinitions.TagEffect,
                rulesetCharacter.guid,
                rulesetCharacter.CurrentFaction.Name,
                1,
                conditionEldritchCannonCommand.Name,
                0,
                0,
                0);
        }
    }

    // Spirit Teleport

    private sealed class ModifyTeleportEffectBehaviorSpiritTeleport : IModifyTeleportEffectBehavior
    {
        public bool AllyOnly => true;

        public bool TeleportSelf => true;

        public int MaxTargets => 8;
    }

    // Enhanced Bond

    private sealed class MagicEffectBeforeHitConfirmedOnEnemyEnhancedBond(FeatureDefinition featureEnhancedBond)
        : IMagicEffectBeforeHitConfirmedOnEnemy
    {
        private static readonly EffectForm HealingEffectForm = EffectFormBuilder
            .Create()
            .SetHealingForm(HealingComputation.Dice, 0, DieType.D8, 1, false, HealingCap.MaximumHitPoints)
            .Build();

        public IEnumerator OnMagicEffectBeforeHitConfirmedOnEnemy(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier actionModifier,
            RulesetEffect rulesetEffect,
            List<EffectForm> actualEffectForms,
            bool firstTarget,
            bool criticalHit)
        {
            if (!firstTarget ||
                !HasSpirit(attacker.Guid))
            {
                yield break;
            }

            var firstDamageForm = actualEffectForms.FirstOrDefault(x => x.FormType == EffectForm.EffectFormType.Damage);

            if (firstDamageForm != null)
            {
                attacker.RulesetCharacter.LogCharacterUsedFeature(featureEnhancedBond);
                actualEffectForms.Add(
                    EffectFormBuilder.DamageForm(firstDamageForm.DamageForm.DamageType, 1, DieType.D8));

                yield break;
            }

            var firstHealingForm = actualEffectForms.FirstOrDefault(x =>
                x.FormType == EffectForm.EffectFormType.Healing &&
                x.HealingForm.HealingComputation == HealingComputation.Dice);

            if (firstHealingForm == null)
            {
                yield break;
            }

            attacker.RulesetCharacter.LogCharacterUsedFeature(featureEnhancedBond);
            actualEffectForms.Add(HealingEffectForm);
        }
    }

    // Summon Cauterizing Flames

    private sealed class OnReducedToZeroHpByMeOrAllySummonCauterizingFlames(
        FeatureDefinitionPower powerSummonCauterizingFlames) : IOnReducedToZeroHpByMeOrAlly
    {
        public IEnumerator HandleReducedToZeroHpByMeOrAlly(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            GameLocationCharacter ally,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            var implementationManager =
                ServiceRepository.GetService<IRulesetImplementationService>() as RulesetImplementationManager;

            if (downedCreature.RulesetCharacter is not RulesetCharacterMonster rulesetCharacterMonster ||
                (rulesetCharacterMonster.MonsterDefinition.SizeDefinition != CharacterSizeDefinitions.Small &&
                 rulesetCharacterMonster.MonsterDefinition.SizeDefinition != CharacterSizeDefinitions.Medium))
            {
                yield break;
            }

            var spirit = GetMySpirit(attacker.Guid);

            if (!ally.IsWithinRange(downedCreature, 6) &&
                (spirit == null || !spirit.IsWithinRange(downedCreature, 6)))
            {
                yield break;
            }

            var rulesetAlly = ally.RulesetCharacter;
            var usablePower = PowerProvider.Get(powerSummonCauterizingFlames, rulesetAlly);
            var actionParams = new CharacterActionParams(ally, Id.PowerNoCost)
            {
                RulesetEffect = implementationManager
                    .MyInstantiateEffectPower(rulesetAlly, usablePower, false),
                UsablePower = usablePower,
                Positions = { downedCreature.LocationPosition }
            };

            ServiceRepository.GetService<IGameLocationActionService>()?
                .ExecuteAction(actionParams, null, true);
        }
    }

    // Blazing Revival

    private sealed class OnReducedToZeroHpByEnemyBlazingRevival : IOnReducedToZeroHpByEnemy
    {
        public IEnumerator HandleReducedToZeroHpByEnemy(
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            var spirit = GetMySpirit(attacker.Guid);

            if (spirit == null ||
                !defender.IsWithinRange(spirit, 12))
            {
                yield break;
            }
        }
    }
}
