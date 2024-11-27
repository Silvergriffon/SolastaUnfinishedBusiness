﻿using System.Collections;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class MartialRoyalKnight : AbstractSubclass
{
    private const string Name = "RoyalKnight";
    private const string ConditionInspiringSurge = $"Condition{Name}InspiringSurge";
    private const string ConditionSpiritedSurge = $"Condition{Name}SpiritedSurge";

    public MartialRoyalKnight()
    {
        // LEVEL 03

        var powerRallyingCry = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}RallyingCry")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerRallyingCry", Resources.PowerRallyingCry, 256, 128))
            .SetUsesAbilityBonus(ActivationTime.BonusAction, RechargeRate.ShortRest, AttributeDefinitions.Charisma)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create(PowerDomainLifePreserveLife.EffectDescription)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetHealingForm(
                                HealingComputation.Dice,
                                0,
                                DieType.D1,
                                4,
                                false,
                                HealingCap.MaximumHitPoints,
                                EffectForm.LevelApplianceType.MultiplyBonus)
                            .Build())
                    .Build())
            .SetOverriddenPower(PowerFighterSecondWind)
            .AddToDB();

        powerRallyingCry.EffectDescription.targetFilteringTag = TargetFilteringTag.No;

        // LEVEL 07

        var abilityCheckAffinityRoyalEnvoy = FeatureDefinitionAbilityCheckAffinityBuilder
            .Create($"AbilityCheckAffinity{Name}RoyalEnvoy")
            .SetGuiPresentation($"FeatureSet{Name}RoyalEnvoy", Category.Feature)
            .BuildAndSetAffinityGroups(
                CharacterAbilityCheckAffinity.HalfProficiencyWhenNotProficient,
                abilityProficiencyPairs: (AttributeDefinitions.Charisma, string.Empty))
            .AddToDB();

        var featureSetRoyalEnvoy = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}RoyalEnvoy")
            .SetGuiPresentation(Category.Feature)
            .AddFeatureSet(
                abilityCheckAffinityRoyalEnvoy,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityCreedOfSolasta)
            .AddToDB();

        // LEVEL 10

        var conditionInspiringSurge = ConditionDefinitionBuilder
            .Create(ConditionInspiringSurge)
            .SetGuiPresentation($"Power{Name}InspiringSurge", Category.Feature, ConditionDefinitions.ConditionSunbeam)
            .AddFeatures(FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain)
            .AddToDB();

        var powerInspiringSurge = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}InspiringSurge")
            .SetGuiPresentation(Category.Feature, SpellDefinitions.Heroism)
            .SetUsesFixed(ActivationTime.BonusAction, RechargeRate.ShortRest)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetDurationData(DurationType.Round, 1)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionInspiringSurge, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .SetParticleEffectParameters(PowerFighterActionSurge)
                    .Build())
            .AddToDB();

        powerInspiringSurge.EffectDescription.effectParticleParameters.targetParticleReference =
            SpellDefinitions.Heroism.EffectDescription.effectParticleParameters.conditionStartParticleReference;

        // LEVEL 15

        const string TEXT = "PowerRoyalKnightInspiringProtection";

        var powerRoyalKnightInspiringProtection = FeatureDefinitionPowerBuilder
            .Create("PowerRoyalKnightInspiringProtection")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.LongRest, 1, 3)
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        var powerRoyalKnightInspiringProtectionAura = FeatureDefinitionPowerBuilder
            .Create("PowerRoyalKnightInspiringProtectionAura")
            .SetGuiPresentation(TEXT, Category.Feature)
            .SetUsesFixed(ActivationTime.PermanentUnlessIncapacitated)
            .SetShowCasting(false)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Sphere, 12)
                    .SetDurationData(DurationType.Permanent)
                    .SetRecurrentEffect(
                        RecurrentEffect.OnActivation | RecurrentEffect.OnEnter | RecurrentEffect.OnTurnStart)
                    .SetEffectForms(EffectFormBuilder.ConditionForm(
                        ConditionDefinitionBuilder
                            .Create("ConditionRoyalKnightInspiringProtectionAura")
                            .SetGuiPresentationNoContent(true)
                            .SetSilent(Silent.WhenAddedOrRemoved)
                            .AddCustomSubFeatures(
                                new TryAlterOutcomeSavingThrowInspiringProtection(powerRoyalKnightInspiringProtection))
                            .AddToDB()))
                    .Build())
            .AddCustomSubFeatures(ModifyPowerVisibility.Hidden)
            .AddToDB();

        var featureSetRoyalKnightInspiringProtection = FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetRoyalKnightInspiringProtection")
            .SetGuiPresentation(TEXT, Category.Feature)
            .AddFeatureSet(powerRoyalKnightInspiringProtectionAura, powerRoyalKnightInspiringProtection)
            .AddToDB();

        // LEVEL 18

        const string POWER_SPIRITED_SURGE = $"Power{Name}SpiritedSurge";

        var combatAffinitySpiritedSurge = FeatureDefinitionCombatAffinityBuilder
            .Create($"CombatAffinity{Name}SpiritedSurge")
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature, Gui.NoLocalization)
            .SetMyAttackAdvantage(AdvantageType.Advantage)
            .AddToDB();

        var abilityCheckAffinitySpiritedSurge = FeatureDefinitionAbilityCheckAffinityBuilder
            .Create($"AbilityCheckAffinity{Name}SpiritedSurge")
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature, Gui.NoLocalization)
            .BuildAndSetAffinityGroups(CharacterAbilityCheckAffinity.Advantage, AttributeDefinitions.AbilityScoreNames)
            .AddToDB();

        var conditionSpiritedSurge = ConditionDefinitionBuilder
            .Create(ConditionSpiritedSurge)
            .SetGuiPresentation(POWER_SPIRITED_SURGE, Category.Feature, ConditionDefinitions.ConditionSunbeam)
            .SetFeatures(
                FeatureDefinitionAdditionalActions.AdditionalActionSurgedMain,
                abilityCheckAffinitySpiritedSurge,
                combatAffinitySpiritedSurge,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityAdvantageToAll)
            .AddToDB();

        var powerSpiritedSurge = FeatureDefinitionPowerBuilder
            .Create(powerInspiringSurge, POWER_SPIRITED_SURGE)
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique)
                    .SetDurationData(DurationType.Round, 1)
                    .SetParticleEffectParameters(PowerFighterActionSurge)
                    .ExcludeCaster()
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionSpiritedSurge, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .Build())
            .SetOverriddenPower(powerInspiringSurge)
            .AddToDB();

        powerSpiritedSurge.EffectDescription.effectParticleParameters.targetParticleReference =
            SpellDefinitions.Heroism.EffectDescription.effectParticleParameters.conditionStartParticleReference;

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create($"Martial{Name}")
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.MartialRoyalKnight, 256))
            .AddFeaturesAtLevel(3, powerRallyingCry)
            .AddFeaturesAtLevel(7, featureSetRoyalEnvoy)
            .AddFeaturesAtLevel(10, powerInspiringSurge)
            .AddFeaturesAtLevel(15, featureSetRoyalKnightInspiringProtection)
            .AddFeaturesAtLevel(18, powerSpiritedSurge)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Fighter;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private class TryAlterOutcomeSavingThrowInspiringProtection(FeatureDefinitionPower powerInspiringProtection)
        : ITryAlterOutcomeSavingThrow
    {
        public IEnumerator OnTryAlterOutcomeSavingThrow(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            SavingThrowData savingThrowData,
            bool hasHitVisual)
        {
            var rulesetDefender = defender.RulesetActor;

            if (rulesetDefender is not { IsDeadOrDyingOrUnconscious: false } ||
                !rulesetDefender.TryGetConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect,
                    "ConditionRoyalKnightInspiringProtectionAura",
                    out var activeCondition))
            {
                yield break;
            }

            RulesetEntity.TryGetEntity<RulesetCharacter>(activeCondition.SourceGuid, out var rulesetHelper);

            var originalHelper = GameLocationCharacter.GetFromActor(rulesetHelper);
            var usablePower = PowerProvider.Get(powerInspiringProtection, rulesetHelper);

            if (savingThrowData.SaveOutcome != RollOutcome.Failure ||
                !originalHelper.CanReact() ||
                !originalHelper.CanPerceiveTarget(defender) ||
                rulesetHelper.GetRemainingUsesOfPower(usablePower) == 0)
            {
                yield break;
            }

            // any reaction within a saving flow must use the yielder as waiter
            yield return originalHelper.MyReactToSpendPower(
                usablePower,
                originalHelper,
                "RoyalKnightInspiringProtection",
                FormatReactionDescription(savingThrowData.Title, attacker, defender, originalHelper),
                ReactionValidated,
                battleManager);

            yield break;

            void ReactionValidated()
            {
                helper.SpendActionType(ActionDefinitions.ActionType.Reaction);

                TryAlterOutcomeSavingThrow.TryRerollSavingThrow(attacker, defender, savingThrowData, hasHitVisual);
            }
        }

        private static string FormatReactionDescription(
            string sourceTitle,
            [CanBeNull] GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper)
        {
            var text = defender == helper ? "Self" : "Ally";

            return $"SpendPowerRoyalKnightInspiringProtectionDescription{text}"
                .Formatted(Category.Reaction, defender.Name, attacker?.Name ?? ReactionRequestCustom.EnvTitle,
                    sourceTitle);
        }
    }
}
