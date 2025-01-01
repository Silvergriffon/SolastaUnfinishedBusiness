﻿using System;
using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Feats;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Properties;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionProficiencys;

namespace SolastaUnfinishedBusiness.Models;

internal static partial class Tabletop2024Context
{
    private static readonly FeatureDefinitionFeatureSet FeatureSetDruidPrimalOrder = FeatureDefinitionFeatureSetBuilder
        .Create("FeatureSetDruidPrimalOrder")
        .SetGuiPresentation(Category.Feature)
        .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion)
        .SetFeatureSet(
            FeatureDefinitionFeatureSetBuilder
                .Create("FeatureSetDruidPrimalOrderMagician")
                .SetGuiPresentation(Category.Feature)
                .SetFeatureSet(
                    FeatureDefinitionPointPoolBuilder
                        .Create("PointPoolDruidPrimalOrderMagician")
                        .SetGuiPresentationNoContent(true)
                        .SetSpellOrCantripPool(HeroDefinitions.PointsPoolType.Cantrip, 1, extraSpellsTag: "PrimalOrder")
                        .AddCustomSubFeatures(new ModifyAbilityCheckDruidPrimalOrder())
                        .AddToDB())
                .AddToDB(),
            FeatureDefinitionFeatureSetBuilder
                .Create("FeatureSetDruidPrimalOrderWarden")
                .SetGuiPresentation(Category.Feature)
                .SetFeatureSet(
                    FeatureDefinitionProficiencyBuilder
                        .Create("ProficiencyDruidPrimalOrderWardenArmor")
                        .SetGuiPresentationNoContent(true)
                        .SetProficiencies(ProficiencyType.Armor, EquipmentDefinitions.MediumArmorCategory)
                        .AddToDB(),
                    FeatureDefinitionProficiencyBuilder
                        .Create("ProficiencyDruidPrimalOrderWardenWeapon")
                        .SetGuiPresentationNoContent(true)
                        .SetProficiencies(ProficiencyType.Weapon, EquipmentDefinitions.MartialWeaponCategory)
                        .AddToDB())
                .AddToDB())
        .AddToDB();

    private static readonly List<string> DruidWeaponsCategories =
        [.. ProficiencyDruidWeapon.Proficiencies, "ConjuredWeaponType"];

    private static readonly FeatureDefinitionPower FeatureDefinitionPowerNatureShroud = FeatureDefinitionPowerBuilder
        .Create("PowerRangerNatureShroud")
        .SetGuiPresentation(Category.Feature, Invisibility)
        .SetUsesAbilityBonus(ActivationTime.BonusAction, RechargeRate.LongRest, AttributeDefinitions.Wisdom)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Round, 0, TurnOccurenceType.StartOfTurn)
                .SetEffectForms(EffectFormBuilder.ConditionForm(ConditionDefinitions.ConditionInvisible))
                .SetParticleEffectParameters(PowerDruidCircleBalanceBalanceOfPower)
                .Build())
        .AddToDB();

    private static readonly FeatureDefinitionPower PowerDruidWildResurgenceShape = FeatureDefinitionPowerBuilder
        .Create("PowerDruidWildResurgenceShape")
        .SetGuiPresentation(Category.Feature,
            Sprites.GetSprite("PowerGainSlot", Resources.PowerGainSlot, 128, 64))
        .SetUsesFixed(ActivationTime.NoCost, RechargeRate.LongRest)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.UntilLongRest)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectForms(EffectFormBuilder.ConditionForm(
                    ConditionDefinitionBuilder
                        .Create("ConditionDruidWildResurgenceShape")
                        .SetGuiPresentationNoContent(true)
                        .SetSilent(Silent.WhenAddedOrRemoved)
                        .SetFeatures(FeatureDefinitionMagicAffinitys.MagicAffinityAdditionalSpellSlot1)
                        .AddToDB()))
                .Build())
        .AddCustomSubFeatures(new ClassFeats.SpendWildShapeUse())
        .AddToDB();

    private static readonly FeatureDefinitionPower PowerDruidWildResurgenceSlot = FeatureDefinitionPowerBuilder
        .Create("PowerDruidWildResurgenceSlot")
        .SetGuiPresentation(Category.Feature,
            Sprites.GetSprite("PowerGainWildShape", Resources.PowerGainWildShape, 128, 64))
        .SetUsesFixed(ActivationTime.NoCost, RechargeRate.TurnStart)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .Build())
        .AddCustomSubFeatures(new ClassFeats.GainWildShapeCharges(1, 1))
        .AddToDB();

    private static readonly FeatureDefinitionFeatureSet FeatureSetDruidWildResurgence =
        FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetDruidWildResurgence")
            .SetGuiPresentation(Category.Feature)
            .SetFeatureSet(PowerDruidWildResurgenceShape, PowerDruidWildResurgenceSlot)
            .AddToDB();

    private static readonly FeatureDefinitionPower PowerDruidNatureMagician = FeatureDefinitionPowerBuilder
        .Create("PowerDruidNatureMagician")
        .SetGuiPresentation(Category.Feature,
            Sprites.GetSprite("PowerGainSlot", Resources.PowerGainSlot, 128, 64))
        .SetUsesFixed(ActivationTime.NoCost, RechargeRate.LongRest)
        .SetEffectDescription(
            EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.UntilLongRest)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .Build())
        .AddCustomSubFeatures(new ClassFeats.SpendWildShapeUse())
        .AddToDB();
    
    private static readonly FeatureDefinitionFeatureSet FeatureSetDruidArchDruid =
        FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetDruidArchDruid")
            .SetGuiPresentation(Category.Feature)
            .SetFeatureSet(PowerDruidNatureMagician)
            .AddToDB();
    
    private static void LoadDruidWildshape()
    {
        PowerDruidWildShape.AddCustomSubFeatures(
            HasModifiedUses.Marker,
            new ModifyPowerPoolAmount
            {
                PowerPool = PowerDruidWildShape,
                Type = PowerPoolBonusCalculationType.Wildshape2024,
                Attribute = DruidClass
            });
    }

    internal static void SwitchDruidArchDruid()
    {
        Druid.FeatureUnlocks.RemoveAll(x =>
            x.FeatureDefinition == FeatureSetDruidArchDruid ||
            x.FeatureDefinition == Level20Context.MagicAffinityArchDruid);

        Druid.FeatureUnlocks.Add(Main.Settings.EnableDruidArchDruid2024
            ? new FeatureUnlockByLevel(FeatureSetDruidArchDruid, 20)
            : new FeatureUnlockByLevel(Level20Context.MagicAffinityArchDruid, 20));

        Druid.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }
    
    private static void SwitchDruidElementalFury()
    {
    }

    internal static void SwitchDruidMetalArmor()
    {
        var active = Main.Settings.EnableDruidMetalArmor2024;

        if (active)
        {
            ProficiencyDruidArmor.ForbiddenItemTags.Clear();
        }
        else
        {
            if (!ProficiencyDruidArmor.ForbiddenItemTags.Contains(
                    TagsDefinitions.ItemTagMetal))
            {
                ProficiencyDruidArmor.ForbiddenItemTags.Add(
                    TagsDefinitions.ItemTagMetal);
            }
        }
    }

    internal static void SwitchDruidPrimalOrder()
    {
        Druid.FeatureUnlocks.RemoveAll(x => x.FeatureDefinition == FeatureSetDruidPrimalOrder);
        ProficiencyDruidArmor.Proficiencies.Remove(EquipmentDefinitions.MediumArmorCategory);

        if (Main.Settings.EnableDruidPrimalOrder2024)
        {
            Druid.FeatureUnlocks.Add(new FeatureUnlockByLevel(FeatureSetDruidPrimalOrder, 1));
        }
        else
        {
            ProficiencyDruidArmor.Proficiencies.Add(EquipmentDefinitions.MediumArmorCategory);
        }

        Druid.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchDruidWeaponProficiency()
    {
        ProficiencyDruidWeapon.proficiencies =
            Main.Settings.EnableDruidWeaponProficiency2024
                ? [WeaponCategoryDefinitions.SimpleWeaponCategory.Name]
                : DruidWeaponsCategories;
    }

    internal static void SwitchDruidWildResurgence()
    {
        Druid.FeatureUnlocks.RemoveAll(x => x.FeatureDefinition == FeatureSetDruidWildResurgence);

        if (Main.Settings.EnableDruidWildResurgence2024)
        {
            Druid.FeatureUnlocks.Add(new FeatureUnlockByLevel(FeatureSetDruidWildResurgence, 5));
        }

        Druid.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchDruidWildshape()
    {
        PowerFighterSecondWind.rechargeRate = Main.Settings.EnableDruidWildshape2024
            ? RechargeRate.LongRest
            : RechargeRate.ShortRest;
    }

    private sealed class ModifyAbilityCheckDruidPrimalOrder : IModifyAbilityCheck
    {
        public void MinRoll(
            RulesetCharacter character,
            int baseBonus,
            string abilityScoreName,
            string proficiencyName,
            List<TrendInfo> advantageTrends,
            List<TrendInfo> modifierTrends,
            ref int rollModifier,
            ref int minRoll)
        {
            if (abilityScoreName is not AttributeDefinitions.Intelligence ||
                proficiencyName is not (SkillDefinitions.Arcana or SkillDefinitions.Nature))
            {
                return;
            }

            var wisdom = character.TryGetAttributeValue(AttributeDefinitions.Wisdom);
            var wisMod = AttributeDefinitions.ComputeAbilityScoreModifier(wisdom);
            var modifier = Math.Max(wisMod, 1);

            rollModifier += modifier;

            modifierTrends.Add(new TrendInfo(modifier, FeatureSourceType.CharacterFeature,
                "FeatureSetDruidPrimalOrderMagician", null));
        }
    }
}
