﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static FeatureDefinitionAttributeModifier;
using static ActionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Builders.Features.AutoPreparedSpellsGroupBuilder;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class RangerLightBearer : AbstractSubclass
{
    internal const string Name = "RangerLightBearer";

    public RangerLightBearer()
    {
        // LEVEL 03

        // Lightbearer Magic

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetAutoTag("Ranger")
            .SetSpellcastingClass(CharacterClassDefinitions.Ranger)
            .SetPreparedSpellGroups(
                BuildSpellGroup(2, Bless),
                BuildSpellGroup(5, BrandingSmite),
                BuildSpellGroup(9, SpellsContext.BlindingSmite),
                BuildSpellGroup(13, GuardianOfFaith),
                BuildSpellGroup(17, SpellsContext.BanishingSmite))
            .AddToDB();

        var powerLight = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}Light")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerLight", Resources.PowerLight, 256, 128))
            .SetUsesFixed(ActivationTime.Action)
            .SetEffectDescription(Light.EffectDescription)
            .AddToDB();

        var featureSetLight = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}Light")
            .SetGuiPresentationNoContent(true)
            .AddFeatureSet(powerLight)
            .AddToDB();

        // Blessed Warrior

        var conditionBlessedWarrior = ConditionDefinitionBuilder
            .Create($"Condition{Name}BlessedWarrior")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionMarkedByBrandingSmite)
            .SetConditionType(ConditionType.Detrimental)
            .CopyParticleReferences(ConditionDefinitions.ConditionMarkedByHunter)
            .AddToDB();

        var powerBlessedWarrior = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}BlessedWarrior")
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerBlessedWarrior", Resources.PowerBlessedWarrior, 256, 128))
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Round, 1, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Enemy, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionBlessedWarrior, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(new PhysicalAttackInitiatedByMeBlessedWarrior(conditionBlessedWarrior))
            .AddToDB();

        // Lifebringer

        var attributeModifierLifeBringerBase = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}LifeBringerBase")
            .SetGuiPresentationNoContent(true)
            .SetModifier(
                AttributeModifierOperation.Set,
                AttributeDefinitions.HealingPool)
            .AddToDB();

        var attributeModifierLifeBringerAdditive = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}LifeBringerAdditive")
            .SetGuiPresentationNoContent(true)
            .SetModifier(
                AttributeModifierOperation.Additive,
                AttributeDefinitions.HealingPool, 1)
            .AddToDB();

        var powerLifeBringer = FeatureDefinitionPowerBuilder
            .Create(FeatureDefinitionPowers.PowerPaladinLayOnHands, $"Power{Name}LifeBringer")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerLifeBringer", Resources.PowerLifeBringer, 256, 128))
            .SetUsesFixed(ActivationTime.Action, RechargeRate.HealingPool, 0)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(FeatureDefinitionPowers.PowerPaladinLayOnHands.EffectDescription)
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.Individuals)
                    .Build())
            .AddToDB();

        // LEVEL 07

        // Blessed Glow

        var powerBlessedGlow = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}BlessedGlow")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Reaction, RechargeRate.ShortRest)
            .SetReactionContext(ExtraReactionContext.Custom)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Enemy, RangeType.Self, 0, TargetType.Sphere, 4)
                    .SetSavingThrowData(
                        false,
                        AttributeDefinitions.Constitution,
                        false,
                        EffectDifficultyClassComputation.SpellCastingFeature)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionBlinded,
                                ConditionForm.ConditionOperation.Add)
                            .HasSavingThrow(EffectSavingThrowType.Negates, TurnOccurenceType.EndOfTurn, true)
                            .Build())
                    .Build())
            .AddToDB();

        powerBlessedGlow.EffectDescription.savingThrowAffinitiesByFamily = new List<SaveAffinityByFamilyDescription>
        {
            new() { advantageType = AdvantageType.Disadvantage, family = "Fiend" },
            new() { advantageType = AdvantageType.Disadvantage, family = "Undead" }
        };

        var powerLightEnhanced = FeatureDefinitionPowerBuilder
            .Create(powerLight, $"Power{Name}LightEnhanced")
            .SetOverriddenPower(powerLight)
            .AddCustomSubFeatures(new MagicEffectFinishedByMeBlessedGlow(powerBlessedGlow))
            .AddToDB();

        var featureSetBlessedGlow = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}BlessedGlow")
            .SetGuiPresentation($"Power{Name}BlessedGlow", Category.Feature)
            .AddFeatureSet(powerLightEnhanced, powerBlessedGlow)
            .AddToDB();

        // LEVEL 11

        // Angelic Form

        var conditionAngelicForm = ConditionDefinitionBuilder
            .Create($"Condition{Name}AngelicForm")
            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionShine)
            .AddFeatures(
                FeatureDefinitionAttackModifierBuilder
                    .Create($"AttackModifier{Name}AngelicForm")
                    .SetGuiPresentation($"Condition{Name}AngelicForm", Category.Condition, Gui.NoLocalization)
                    .SetMagicalWeapon()
                    .AddToDB())
            .AddToDB();

        var powerAngelicFormSprout = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicFormSprout")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerAngelicFormSprout", Resources.PowerAngelicFormSprout, 256, 128))
            .SetUsesFixed(ActivationTime.Action, RechargeRate.LongRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionAngelicForm,
                                ConditionForm.ConditionOperation.Add)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFlyingAdaptive,
                                ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(
                new MagicEffectFinishedByMeAngelicForm(),
                new ValidatorsValidatePowerUse(ValidatorsCharacter.HasNoneOfConditions(ConditionFlyingAdaptive)))
            .AddToDB();

        var powerAngelicFormDismiss = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}AngelicFormDismiss")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerAngelicFormDismiss", Resources.PowerAngelicFormDismiss, 256, 128))
            .SetUsesFixed(ActivationTime.BonusAction)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                conditionAngelicForm,
                                ConditionForm.ConditionOperation.Remove)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(
                                ConditionDefinitions.ConditionFlyingAdaptive,
                                ConditionForm.ConditionOperation.Remove)
                            .Build())
                    .Build())
            .AddCustomSubFeatures(
                new ValidatorsValidatePowerUse(ValidatorsCharacter.HasAnyOfConditions(ConditionFlyingAdaptive)))
            .AddToDB();

        var featureSetAngelicForm = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}AngelicForm")
            .SetGuiPresentation($"Power{Name}AngelicFormSprout", Category.Feature)
            .AddFeatureSet(powerAngelicFormSprout, powerAngelicFormDismiss)
            .AddToDB();

        // LEVEL 15

        // Warding Light

        var actionAffinityWardingLight = FeatureDefinitionActionAffinityBuilder
            .Create($"ActionAffinity{Name}WardingLight")
            .SetGuiPresentationNoContent(true)
            .SetAuthorizedActions(Id.BlockAttack)
            .AddToDB();

        var featureWardingLight = FeatureDefinitionBuilder
            .Create($"Feature{Name}WardingLight")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(new PhysicalAttackInitiatedOnMeOrAllyWardingLight())
            .AddToDB();

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.RangerLightBearer, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpells,
                featureSetLight,
                powerBlessedWarrior,
                attributeModifierLifeBringerBase,
                attributeModifierLifeBringerAdditive,
                powerLifeBringer)
            .AddFeaturesAtLevel(7,
                featureSetBlessedGlow)
            .AddFeaturesAtLevel(11,
                featureSetAngelicForm)
            .AddFeaturesAtLevel(15,
                actionAffinityWardingLight,
                featureWardingLight)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Ranger;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRangerArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    //
    // Blessed Warrior
    //

    private sealed class PhysicalAttackInitiatedByMeBlessedWarrior : IPhysicalAttackInitiatedByMe
    {
        private readonly ConditionDefinition _conditionDefinition;

        public PhysicalAttackInitiatedByMeBlessedWarrior(ConditionDefinition conditionDefinition)
        {
            _conditionDefinition = conditionDefinition;
        }

        public IEnumerator OnAttackInitiatedByMe(
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            ActionModifier attackModifier,
            RulesetAttackMode attackMode)
        {
            var rulesetDefender = defender.RulesetCharacter;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                !rulesetDefender.HasAnyConditionOfType(_conditionDefinition.Name))
            {
                yield break;
            }

            // change all damage types to radiant
            attackMode.effectDescription = EffectDescriptionBuilder
                .Create(attackMode.EffectDescription)
                .Build();

            var effectDescription = attackMode.EffectDescription;

            foreach (var damageForm in effectDescription.EffectForms
                         .Where(x => x.FormType == EffectForm.EffectFormType.Damage))
            {
                damageForm.DamageForm.damageType = DamageTypeRadiant;
            }

            // add additional radiant damage form
            var classLevel = attacker.RulesetCharacter.GetClassLevel(CharacterClassDefinitions.Ranger);
            var pos = attackMode.EffectDescription.EffectForms.FindIndex(x =>
                x.FormType == EffectForm.EffectFormType.Damage);

            if (pos >= 0)
            {
                effectDescription.EffectForms.Insert(pos,
                    EffectFormBuilder.DamageForm(DamageTypeRadiant, classLevel < 11 ? 1 : 2, DieType.D8));
            }

            var rulesetCondition =
                rulesetDefender.AllConditions.FirstOrDefault(x => x.ConditionDefinition == _conditionDefinition);

            if (rulesetCondition != null)
            {
                rulesetDefender.RemoveCondition(rulesetCondition);
            }
        }
    }

    //
    // Blessed Glow
    //

    private sealed class MagicEffectFinishedByMeBlessedGlow : IMagicEffectFinishedByMe
    {
        private readonly FeatureDefinitionPower _powerBlessedGlow;

        public MagicEffectFinishedByMeBlessedGlow(FeatureDefinitionPower featureDefinitionPower)
        {
            _powerBlessedGlow = featureDefinitionPower;
        }

        public IEnumerator OnMagicEffectFinishedByMe(CharacterActionMagicEffect action, BaseDefinition power)
        {
            var gameLocationBattleService = ServiceRepository.GetService<IGameLocationBattleService>()
                as GameLocationBattleManager;

            if (gameLocationBattleService is not { IsBattleInProgress: true })
            {
                yield break;
            }

            var attacker = action.ActingCharacter;
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker.GetRemainingPowerCharges(_powerBlessedGlow) <= 0)
            {
                yield break;
            }

            var gameLocationActionService =
                ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (gameLocationActionService == null)
            {
                yield break;
            }

            var reactionParams =
                new CharacterActionParams(attacker, (Id)ExtraActionId.DoNothingFree)
                {
                    StringParameter = "Reaction/&CustomReactionBlessedGlowDescription"
                };
            var previousReactionCount = gameLocationActionService.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("BlessedGlow", reactionParams);

            gameLocationActionService.AddInterruptRequest(reactionRequest);

            yield return gameLocationBattleService.WaitForReactions(
                attacker, gameLocationActionService, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            rulesetAttacker.UpdateUsageForPower(_powerBlessedGlow, _powerBlessedGlow.CostPerUse);

            var actionParams = action.ActionParams.Clone();
            var usablePower = UsablePowersProvider.Get(_powerBlessedGlow, rulesetAttacker);

            actionParams.ActionDefinition = DatabaseHelper.ActionDefinitions.SpendPower;
            actionParams.RulesetEffect = ServiceRepository.GetService<IRulesetImplementationService>()
                .InstantiateEffectPower(rulesetAttacker, usablePower, false)
                .AddAsActivePowerToSource();
            actionParams.TargetCharacters.SetRange(gameLocationBattleService.Battle.AllContenders
                .Where(x => x.IsOppositeSide(attacker.Side)
                            && x.RulesetCharacter is { IsDeadOrDyingOrUnconscious: false })
                .Where(enemy => rulesetAttacker.DistanceTo(enemy.RulesetActor) <= 5)
                .ToList());

            // different follow up pattern [not adding to ResultingActions] as it doesn't work after a reaction
            ServiceRepository.GetService<ICommandService>()?.ExecuteAction(actionParams, null, false);
        }
    }

    //
    // Angelic Form
    //

    private sealed class MagicEffectFinishedByMeAngelicForm : IMagicEffectFinishedByMe
    {
        public IEnumerator OnMagicEffectFinishedByMe(CharacterActionMagicEffect action, BaseDefinition power)
        {
            var rulesetCharacter = action.ActingCharacter.RulesetCharacter;
            var classLevel = rulesetCharacter.GetClassLevel(CharacterClassDefinitions.Ranger);

            rulesetCharacter.ReceiveTemporaryHitPoints(
                classLevel, DurationType.Minute, 1, TurnOccurenceType.EndOfTurn, rulesetCharacter.Guid);

            yield break;
        }
    }

    //
    // Warding Light
    //

    private sealed class PhysicalAttackInitiatedOnMeOrAllyWardingLight : IPhysicalAttackInitiatedOnMeOrAlly
    {
        public IEnumerator OnAttackInitiatedOnMeOrAlly(
            GameLocationBattleManager __instance,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter ally,
            ActionModifier attackModifier,
            RulesetAttackMode attackerAttackMode)
        {
            if (__instance is not { IsBattleInProgress: true })
            {
                yield break;
            }

            yield return __instance.Battle.AllContenders
                .Where(opposingContender =>
                    opposingContender.IsOppositeSide(attacker.Side) &&
                    opposingContender != defender &&
                    opposingContender.CanReact() &&
                    __instance.IsWithinXCells(opposingContender, defender, 6) &&
                    opposingContender.GetActionStatus(Id.BlockAttack, ActionScope.Battle, ActionStatus.Available) ==
                    ActionStatus.Available)
                .ToList() // avoid enumerator changes
                .Select(opposingContender => __instance.PrepareAndReact(
                    opposingContender, attacker, attacker, Id.BlockAttack, attackModifier,
                    additionalTargetCharacter: defender))
                .GetEnumerator();
        }
    }
}
