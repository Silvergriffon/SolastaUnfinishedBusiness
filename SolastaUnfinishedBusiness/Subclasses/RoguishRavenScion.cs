﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Properties;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class RoguishRavenScion : AbstractSubclass
{
    private const string Name = "RavenScion";

    public RoguishRavenScion()
    {
        //
        // LEVEL 03
        //

        // Ranged Specialist

        var featureSetRavenSharpShooter = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}RangedSpecialist")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                // proficient with all two handed range weapons
                FeatureDefinitionProficiencyBuilder
                    .Create($"Proficiency{Name}RangeWeapon")
                    .SetGuiPresentationNoContent(true)
                    .SetProficiencies(
                        ProficiencyType.Weapon,
                        WeaponTypeDefinitions.HeavyCrossbowType.Name,
                        WeaponTypeDefinitions.LongbowType.Name)
                    .AddToDB(),
                FeatureDefinitionProficiencyBuilder
                    .Create($"Proficiency{Name}FightingStyle")
                    .SetGuiPresentationNoContent(true)
                    .SetProficiencies(ProficiencyType.FightingStyle, "Archery")
                    .AddToDB())
            .AddToDB();

        // Sniper's Aim

        var additionalDamageSniperAim = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}SniperAim")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag("SniperAim")
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetDamageValueDetermination(ExtraAdditionalDamageValueDetermination.CharacterLevel)
            .SetRequiredProperty(RestrictedContextRequiredProperty.Weapon)
            .AddCustomSubFeatures(
                new RogueClassHolder(),
                new ValidateContextInsteadOfRestrictedProperty(
                    (_, _, _, _, _, mode, _) => (OperationType.Set, ValidatorsWeapon.IsTwoHandedRanged(mode))))
            .AddToDB();

        //
        // LEVEL 09
        //

        // Heart-Seeking Shot

        var powerHeartSeekingShot = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}HeartSeekingShot")
            .SetGuiPresentation(Category.Feature, PowerMartialCommanderRousingShout)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Minute, 1, TurnOccurenceType.StartOfTurn)
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(
                        ConditionDefinitionBuilder
                            .Create($"Condition{Name}HeartSeekingShot")
                            .SetGuiPresentation(Category.Condition, ConditionDefinitions.ConditionGuided)
                            .SetPossessive()
                            .SetSpecialInterruptions(ConditionInterruption.Attacks)
                            .AddCustomSubFeatures(new ModifyAttackOutcomeHeartSeekingShot())
                            .AddToDB()))
                    .SetParticleEffectParameters(PowerPactChainImp)
                    .Build())
            .AddToDB();

        powerHeartSeekingShot.EffectDescription.EffectParticleParameters.conditionStartParticleReference =
            new AssetReference();
        powerHeartSeekingShot.EffectDescription.EffectParticleParameters.conditionEndParticleReference =
            new AssetReference();

        // Killing Spree

        var featureRavenKillingSpree = FeatureDefinitionBuilder
            .Create($"Feature{Name}KillingSpree")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(
                // bonus range attack from main and can use sniper's aim again during this turn
                new OnReducedToZeroHpByMeKillingSpree(ConditionDefinitionBuilder
                    .Create($"Condition{Name}KillingSpree")
                    .SetGuiPresentationNoContent(true)
                    .SetSilent(Silent.WhenAddedOrRemoved)
                    .SetFeatures(FeatureDefinitionAdditionalActionBuilder
                        .Create($"AdditionalAction{Name}KillingSpree")
                        .SetGuiPresentationNoContent(true)
                        .SetActionType(ActionDefinitions.ActionType.Main)
                        .SetRestrictedActions(ActionDefinitions.Id.AttackMain)
                        .SetMaxAttacksNumber(1)
                        .AddCustomSubFeatures(AdditionalActionAttackValidator.TwoHandedRanged)
                        .AddToDB())
                    .AddToDB()))
            .AddToDB();

        //
        // LEVEL 13
        //

        // Deadly Focus

        var powerDeadlyFocus = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}DeadlyFocus")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.Reaction, RechargeRate.ShortRest)
            .SetReactionContext(ExtraReactionContext.Custom)
            .AddToDB();

        powerDeadlyFocus.AddCustomSubFeatures(new TryAlterOutcomePhysicalAttackDeadlyAim(powerDeadlyFocus));

        //
        // LEVEL 17
        //

        // Perfect Shot

        var dieRollModifierRavenPerfectShot = FeatureDefinitionDieRollModifierBuilder
            .Create($"DieRollModifier{Name}PerfectShot")
            .SetGuiPresentation(Category.Feature)
            .SetModifiers(RollContext.AttackDamageValueRoll, 1, 2, 1,
                "Feature/&DieRollModifierRavenPainMakerReroll")
            .AddCustomSubFeatures(new RoguishRaven.RavenRerollAnyDamageDieMarker())
            .AddToDB();

        //
        // MAIN
        //

        Subclass = CharacterSubclassDefinitionBuilder
            .Create($"Roguish{Name}")
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.RoguishRaven, 256))
            .AddFeaturesAtLevel(3, featureSetRavenSharpShooter, additionalDamageSniperAim)
            .AddFeaturesAtLevel(9, featureRavenKillingSpree, powerHeartSeekingShot)
            .AddFeaturesAtLevel(13, powerDeadlyFocus)
            .AddFeaturesAtLevel(17, dieRollModifierRavenPerfectShot)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Rogue;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceRogueRoguishArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private sealed class RogueClassHolder : IClassHoldingFeature
    {
        public CharacterClassDefinition Class => CharacterClassDefinitions.Rogue;
    }

    //
    // Killing Spree
    //

    private sealed class OnReducedToZeroHpByMeKillingSpree : IOnReducedToZeroHpByMe
    {
        private readonly ConditionDefinition _condition;

        public OnReducedToZeroHpByMeKillingSpree(ConditionDefinition condition)
        {
            _condition = condition;
        }

        public IEnumerator HandleReducedToZeroHpByMe(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            if (activeEffect != null || !ValidatorsWeapon.IsTwoHandedRanged(attackMode))
            {
                yield break;
            }

            var rulesetAttacker = attacker.RulesetCharacter;

            if (rulesetAttacker.HasAnyConditionOfType(_condition.Name))
            {
                yield break;
            }

            if (Gui.Battle?.ActiveContender != attacker)
            {
                yield break;
            }

            attacker.UsedSpecialFeatures.Remove($"AdditionalDamage{Name}SniperAim");

            rulesetAttacker.InflictCondition(
                _condition.Name,
                DurationType.Round,
                0,
                TurnOccurenceType.StartOfTurn,
                AttributeDefinitions.TagCombat,
                rulesetAttacker.guid,
                rulesetAttacker.CurrentFaction.Name,
                1,
                _condition.Name,
                0,
                0,
                0);
        }
    }

    //
    // Heart-Seeking Shot
    //

    private class ModifyAttackOutcomeHeartSeekingShot : IModifyAttackOutcome
    {
        public void OnAttackOutcome(
            RulesetCharacter __instance,
            ref int __result,
            int toHitBonus,
            RulesetActor target,
            BaseDefinition attackMethod,
            List<TrendInfo> toHitTrends,
            bool ignoreAdvantage,
            List<TrendInfo> advantageTrends,
            bool rangeAttack,
            bool opportunity,
            int rollModifier,
            ref RollOutcome outcome,
            ref int successDelta,
            int predefinedRoll,
            bool testMode,
            ActionDefinitions.ReactionCounterAttackType reactionCounterAttackType)
        {
            if (outcome is not (RollOutcome.Success or RollOutcome.CriticalSuccess))
            {
                return;
            }

            if (attackMethod is not ItemDefinition itemDefinition
                || !ValidatorsWeapon.IsTwoHandedRanged(itemDefinition))
            {
                return;
            }

            outcome = RollOutcome.CriticalSuccess;
        }
    }

    //
    // Deadly Focus
    //

    private class TryAlterOutcomePhysicalAttackDeadlyAim : ITryAlterOutcomePhysicalAttack
    {
        private readonly FeatureDefinitionPower _power;

        public TryAlterOutcomePhysicalAttackDeadlyAim(FeatureDefinitionPower power)
        {
            _power = power;
        }

        public IEnumerator OnAttackTryAlterOutcome(
            GameLocationBattleManager battle,
            CharacterAction action,
            GameLocationCharacter me,
            GameLocationCharacter target,
            ActionModifier attackModifier)
        {
            var attackMode = action.actionParams.attackMode;
            var rulesetAttacker = me.RulesetCharacter;

            if (rulesetAttacker is not { IsDeadOrDyingOrUnconscious: false }
                || rulesetAttacker.GetRemainingPowerCharges(_power) <= 0
                || !ValidatorsWeapon.IsTwoHandedRanged(attackMode))
            {
                yield break;
            }

            var manager = ServiceRepository.GetService<IGameLocationActionService>() as GameLocationActionManager;

            if (manager == null)
            {
                yield break;
            }

            var reactionParams = new CharacterActionParams(me, (ActionDefinitions.Id)ExtraActionId.DoNothingFree);
            var previousReactionCount = manager.PendingReactionRequestGroups.Count;
            var reactionRequest = new ReactionRequestCustom("RavenScionDeadlyFocus", reactionParams);

            manager.AddInterruptRequest(reactionRequest);

            yield return battle.WaitForReactions(me, manager, previousReactionCount);

            if (!reactionParams.ReactionValidated)
            {
                yield break;
            }

            rulesetAttacker.UpdateUsageForPower(_power, _power.CostPerUse);

            var totalRoll = (action.AttackRoll + attackMode.ToHitBonus).ToString();
            var rollCaption = action.AttackRoll == 1
                ? "Feedback/&RollCheckCriticalFailureTitle"
                : "Feedback/&CriticalAttackFailureOutcome";

            rulesetAttacker.LogCharacterUsedPower(
                _power,
                $"Feedback/&Trigger{Name}RerollLine",
                false,
                (ConsoleStyleDuplet.ParameterType.Base, $"{action.AttackRoll}+{attackMode.ToHitBonus}"),
                (ConsoleStyleDuplet.ParameterType.FailedRoll, Gui.Format(rollCaption, totalRoll)));

            var advantageTrends =
                new List<TrendInfo> { new(1, FeatureSourceType.CharacterFeature, _power.Name, _power) };

            var roll = rulesetAttacker.RollAttack(
                attackMode.toHitBonus,
                target.RulesetCharacter,
                attackMode.sourceDefinition,
                attackModifier.attackToHitTrends,
                false,
                advantageTrends,
                attackMode.ranged,
                false,
                attackModifier.attackRollModifier,
                out var outcome,
                out var successDelta,
                -1,
                // testMode true avoids the roll to display on combat log as the original one will get there with altered results
                true);

            attackModifier.ignoreAdvantage = false;
            attackModifier.attackAdvantageTrends = advantageTrends;
            action.AttackRollOutcome = outcome;
            action.AttackSuccessDelta = successDelta;
            action.AttackRoll = roll;
        }
    }
}
