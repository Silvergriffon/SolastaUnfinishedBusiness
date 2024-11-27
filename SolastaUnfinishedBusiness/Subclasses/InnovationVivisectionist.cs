using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Classes;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using UnityEngine.AddressableAssets;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Subclasses.CommonBuilders;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class InnovationVivisectionist : AbstractSubclass
{
    private const string Name = "InnovationVivisectionist";

    public InnovationVivisectionist()
    {
        //
        // MAIN
        //

        // LEVEL 03

        // Auto Prepared Spells

        var autoPreparedSpells = FeatureDefinitionAutoPreparedSpellsBuilder
            .Create($"AutoPreparedSpells{Name}")
            .SetGuiPresentation(Category.Feature)
            .SetSpellcastingClass(InventorClass.Class)
            .SetAutoTag("InventorVivisectionist")
            .AddPreparedSpellGroup(3, Bless, InflictWounds)
            .AddPreparedSpellGroup(5, EnhanceAbility, LesserRestoration)
            .AddPreparedSpellGroup(9, RemoveCurse, Revivify)
            .AddPreparedSpellGroup(13, DeathWard, IdentifyCreatures)
            .AddPreparedSpellGroup(17, Contagion, RaiseDead)
            .AddToDB();

        // Medical Accuracy

        var additionalDamageMedicalAccuracy = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{Name}MedicalAccuracy")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag("MedicalAccuracy")
            .SetDamageDice(DieType.D6, 1)
            .SetAdvancement(AdditionalDamageAdvancement.ClassLevel, 1, 1, 4, 3)
            .SetRequiredProperty(RestrictedContextRequiredProperty.Weapon)
            .SetTriggerCondition(AdditionalDamageTriggerCondition.AdvantageOrNearbyAlly)
            .SetFrequencyLimit(FeatureLimitedUsage.OncePerTurn)
            .SetAttackModeOnly()
            //.AddCustomSubFeatures(ClassHolder.Inventor)
            .AddToDB();

        // Emergency Surgery

        var powerEmergencySurgery = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}EmergencySurgery")
            .SetGuiPresentation(Category.Feature,
                Sprites.GetSprite("PowerEmergencySurgery", Resources.PowerEmergencySurgery, 256, 128))
            .SetUsesProficiencyBonus(ActivationTime.Action)
            .SetExplicitAbilityScore(AttributeDefinitions.Intelligence)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Touch, 0, TargetType.IndividualsUnique)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetBonusMode(AddBonusMode.AbilityBonus)
                            .SetDiceAdvancement(LevelSourceType.ClassLevel, 1, 1, 4, 7)
                            .SetHealingForm(
                                HealingComputation.Dice, 0, DieType.D6, 1, false, HealingCap.MaximumHitPoints)
                            .Build())
                    .SetCasterEffectParameters(new AssetReference())
                    .SetImpactEffectParameters(PowerTraditionOpenHandWholenessOfBody
                        .EffectDescription.EffectParticleParameters.effectParticleReference)
                    .Build())
            //.AddCustomSubFeatures(ClassHolder.Inventor)
            .AddToDB();

        // LEVEL 05

        // Extra Attack

        // Emergency Cure

        var powerEmergencyCure = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}EmergencyCure")
            .SetGuiPresentation(Category.Feature, PowerOathOfJugementPurgeCorruption)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .AddToDB();

        var powerEmergencyCureLesserRestoration = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}EmergencyCureLesserRestoration")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.BonusAction, powerEmergencyCure)
            .SetEffectDescription(LesserRestoration.EffectDescription)
            .AddToDB();

        var powerEmergencyCureRemoveCurse = FeatureDefinitionPowerSharedPoolBuilder
            .Create($"Power{Name}EmergencyCureRemoveCurse")
            .SetGuiPresentation(Category.Feature)
            .SetSharedPool(ActivationTime.BonusAction, powerEmergencyCure)
            .SetEffectDescription(RemoveCurse.EffectDescription)
            .AddToDB();

        PowerBundle.RegisterPowerBundle(powerEmergencyCure, false,
            powerEmergencyCureLesserRestoration, powerEmergencyCureRemoveCurse);

        // LEVEL 09

        // Stable Surgery

        var dieRollModifierStableSurgery = FeatureDefinitionDieRollModifierBuilder
            .Create($"DieRollModifier{Name}StableSurgery")
            .SetGuiPresentation(Category.Feature)
            .SetModifiers(
                RollContext.HealValueRoll,
                0,
                2,
                0,
                $"Feature/&DieRollModifier{Name}StableSurgeryReroll")
            .AddToDB();

        // Organ Donation

        var powerOrganDonation = FeatureDefinitionPowerBuilder
            .Create($"Power{Name}OrganDonation")
            .SetGuiPresentation(Category.Feature)
            .SetUsesFixed(ActivationTime.NoCost, RechargeRate.ShortRest)
            .SetShowCasting(false)
            .AddToDB();

        powerOrganDonation.AddCustomSubFeatures(
            ModifyPowerVisibility.Hidden,
            new OnReducedToZeroHpByMeOrganDonation(powerOrganDonation, powerEmergencySurgery, powerEmergencyCure));

        // LEVEL 15

        // Master Emergency Surgery

        var powerMasterEmergencySurgery = FeatureDefinitionPowerBuilder
            .Create(powerEmergencySurgery, $"Power{Name}MasterEmergencySurgery")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetUsesProficiencyBonus(ActivationTime.BonusAction)
            .SetOverriddenPower(powerEmergencySurgery)
            .AddToDB();

        // Master Emergency Cure

        var powerMasterEmergencyCure = FeatureDefinitionPowerBuilder
            .Create(powerEmergencyCure, $"Power{Name}MasterEmergencyCure")
            .SetOrUpdateGuiPresentation(Category.Feature)
            .SetUsesProficiencyBonus(ActivationTime.NoCost)
            .SetOverriddenPower(powerEmergencyCure)
            .AddToDB();

        var powerMasterEmergencyCureLesserRestoration = FeatureDefinitionPowerSharedPoolBuilder
            .Create(powerEmergencyCureLesserRestoration, $"Power{Name}MasterEmergencyCureLesserRestoration")
            .SetSharedPool(ActivationTime.NoCost, powerMasterEmergencyCure)
            .AddToDB();

        var powerMasterEmergencyCureRemoveCurse = FeatureDefinitionPowerSharedPoolBuilder
            .Create(powerEmergencyCureRemoveCurse, $"Power{Name}MasterEmergencyCureRemoveCurse")
            .SetSharedPool(ActivationTime.NoCost, powerMasterEmergencyCure)
            .AddToDB();

        PowerBundle.RegisterPowerBundle(powerMasterEmergencyCure, false,
            powerMasterEmergencyCureLesserRestoration, powerMasterEmergencyCureRemoveCurse);

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.InventorVivisectionist, 256))
            .AddFeaturesAtLevel(3,
                autoPreparedSpells,
                additionalDamageMedicalAccuracy,
                powerEmergencySurgery)
            .AddFeaturesAtLevel(5,
                PowerCasterFightingWarMagic,
                powerEmergencyCure)
            .AddFeaturesAtLevel(9,
                dieRollModifierStableSurgery,
                powerOrganDonation)
            .AddFeaturesAtLevel(15,
                powerMasterEmergencySurgery,
                powerMasterEmergencyCure)
            .AddToDB();
    }

    internal override CharacterSubclassDefinition Subclass { get; }
    internal override CharacterClassDefinition Klass => InventorClass.Class;
    internal override FeatureDefinitionSubclassChoice SubclassChoice => InventorClass.SubclassChoice;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private class OnReducedToZeroHpByMeOrganDonation(
        FeatureDefinitionPower powerOrganDonation,
        FeatureDefinitionPower powerEmergencySurgery,
        FeatureDefinitionPower powerEmergencyCure)
        : IOnReducedToZeroHpByMe
    {
        public IEnumerator HandleReducedToZeroHpByMe(
            GameLocationCharacter attacker,
            GameLocationCharacter downedCreature,
            RulesetAttackMode attackMode,
            RulesetEffect activeEffect)
        {
            var rulesetAttacker = attacker.RulesetCharacter;
            var usablePower = PowerProvider.Get(powerOrganDonation, rulesetAttacker);
            var usablePowerEmergencyCure = PowerProvider.Get(powerEmergencyCure, rulesetAttacker);
            var usablePowerEmergencySurgery = PowerProvider.Get(powerEmergencySurgery, rulesetAttacker);

            if (rulesetAttacker.GetRemainingUsesOfPower(usablePower) == 0 ||
                !attacker.OncePerTurnIsValid(powerOrganDonation.Name) ||
                (usablePowerEmergencyCure.MaxUses == usablePowerEmergencyCure.RemainingUses &&
                 usablePowerEmergencySurgery.MaxUses == usablePowerEmergencySurgery.RemainingUses))
            {
                yield break;
            }

            attacker.UsedSpecialFeatures.TryAdd(powerOrganDonation.Name, 0);

            yield return attacker.MyReactToSpendPower(
                usablePower,
                attacker,
                "OrganDonation",
                reactionValidated: ReactionValidated);

            yield break;

            void ReactionValidated()
            {
                rulesetAttacker.RepayPowerUse(usablePowerEmergencyCure);
                rulesetAttacker.RepayPowerUse(usablePowerEmergencySurgery);
            }
        }
    }
}
