﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Interfaces;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionSubclassChoices;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttributeModifiers;

namespace SolastaUnfinishedBusiness.Models;

internal static partial class Tabletop2024Context
{
    private static readonly FeatureDefinitionPointPool PointPoolClericThaumaturgeCantrip =
        FeatureDefinitionPointPoolBuilder
            .Create("PointPoolClericThaumaturgeCantrip")
            .SetGuiPresentation(Category.Feature)
            .SetSpellOrCantripPool(HeroDefinitions.PointsPoolType.Cantrip, 1, SpellListDefinitions.SpellListCleric,
                "Thaumaturge")
            .AddCustomSubFeatures(new ModifyAbilityCheckThaumaturge())
            .AddToDB();

    private static readonly FeatureDefinitionFeatureSet FeatureSetClericDivineOrder = FeatureDefinitionFeatureSetBuilder
        .Create("FeatureSetClericDivineOrder")
        .SetGuiPresentation(Category.Feature)
        .SetMode(FeatureDefinitionFeatureSet.FeatureSetMode.Exclusion)
        .SetFeatureSet(
            PointPoolClericThaumaturgeCantrip,
            FeatureDefinitionFeatureSetBuilder
                .Create("FeatureSetClericProtector")
                .SetGuiPresentation(Category.Feature)
                .SetFeatureSet(
                    FeatureDefinitionProficiencyBuilder
                        .Create("ProficiencyClericProtectorArmor")
                        .SetGuiPresentationNoContent(true)
                        .SetProficiencies(RuleDefinitions.ProficiencyType.Armor,
                            EquipmentDefinitions.HeavyArmorCategory)
                        .AddToDB(),
                    FeatureDefinitionProficiencyBuilder
                        .Create("ProficiencyClericProtectorWeapons")
                        .SetGuiPresentationNoContent(true)
                        .SetProficiencies(RuleDefinitions.ProficiencyType.Weapon,
                            EquipmentDefinitions.MartialWeaponCategory)
                        .AddToDB())
                .AddToDB())
        .AddToDB();

    private static readonly FeatureDefinitionFeatureSet FeatureSetClericDivineIntervention =
        FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetClericDivineIntervention")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

    private static readonly FeatureDefinitionFeatureSet FeatureSetClericDivineInterventionImproved =
        FeatureDefinitionFeatureSetBuilder
            .Create("FeatureSetClericDivineInterventionImproved")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

    internal static void SwitchClericDivineIntervention()
    {
        var divineInterventions = new[]
        {
            ("DomainBattle", "PowerClericDivineInterventionPaladin"), //
            ("DomainDefiler", "PowerClericDivineInterventionPaladin"), // UB
            ("DomainElementalCold", "PowerClericDivineInterventionWizard"), //
            ("DomainElementalFire", "PowerClericDivineInterventionWizard"), //
            ("DomainElementalLighting", "PowerClericDivineInterventionWizard"), //
            ("DomainInsight", "PowerClericDivineInterventionCleric"), //
            ("DomainLaw", "PowerClericDivineInterventionPaladin"), //
            ("DomainLife", "PowerClericDivineInterventionCleric"), //
            ("DomainMischief", "PowerClericDivineInterventionWizard"), //
            ("DomainNature", "PowerClericDivineInterventionWizard"), // UB
            ("DomainOblivion", "PowerClericDivineInterventionCleric"), //
            ("DomainSmith", "PowerClericDivineInterventionPaladin"), // UB
            ("DomainSun", "PowerClericDivineInterventionWizard"), //
            ("DomainTempest", "PowerClericDivineInterventionPaladin") // UB
        };

        foreach (var (subclassName, powerName) in divineInterventions)
        {
            var subClass = GetDefinition<CharacterSubclassDefinition>(subclassName);
            var power = GetDefinition<FeatureDefinitionPower>(powerName);

            subClass.FeatureUnlocks.RemoveAll(x =>
                x.FeatureDefinition == power ||
                x.FeatureDefinition == FeatureSetClericDivineIntervention);

            subClass.FeatureUnlocks.Add(Main.Settings.EnableClericDivineOrder2024
                ? new FeatureUnlockByLevel(FeatureSetClericDivineIntervention, 10)
                : new FeatureUnlockByLevel(power, 10));
        }

        foreach (var (subclassName, powerName) in divineInterventions)
        {
            var subClass = GetDefinition<CharacterSubclassDefinition>(subclassName);
            var improvementPowerName = powerName.Replace("Intervention", "InterventionImprovement");
            var improvementPower = GetDefinition<FeatureDefinitionPower>(improvementPowerName);

            subClass.FeatureUnlocks.RemoveAll(x =>
                x.FeatureDefinition == improvementPower ||
                x.FeatureDefinition == FeatureSetClericDivineInterventionImproved);

            subClass.FeatureUnlocks.Add(Main.Settings.EnableClericDivineOrder2024
                ? new FeatureUnlockByLevel(FeatureSetClericDivineInterventionImproved, 20)
                : new FeatureUnlockByLevel(improvementPower, 20));
        }

        Cleric.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchClericDivineOrder()
    {
        Cleric.FeatureUnlocks
            .RemoveAll(x => x.FeatureDefinition == FeatureSetClericDivineOrder);

        if (Main.Settings.EnableClericDivineOrder2024)
        {
            Cleric.FeatureUnlocks.Add(new FeatureUnlockByLevel(FeatureSetClericDivineOrder, 1));
        }

        Cleric.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchClericChannelDivinity()
    {
        if (Main.Settings.EnableClericChannelDivinity2024)
        {
            AttributeModifierClericChannelDivinity.modifierValue = 2;
            AttributeModifierClericChannelDivinity.GuiPresentation.description =
                "Feature/&PaladinChannelDivinityDescription";
        }
        else
        {
            AttributeModifierClericChannelDivinity.modifierValue = 1;
            AttributeModifierClericChannelDivinity.GuiPresentation.description =
                "Feature/&ClericChannelDivinityDescription";
        }

        Cleric.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchClericDomainLearningLevel()
    {
        var domains = DatabaseRepository.GetDatabase<CharacterSubclassDefinition>()
            .Where(x => x.Name.StartsWith("Domain"))
            .ToList();

        var fromLevel = 3;
        var toLevel = 1;

        if (Main.Settings.EnableClericToLearnDomainAtLevel3)
        {
            fromLevel = 1;
            toLevel = 3;
        }

        foreach (var featureUnlock in domains
                     .SelectMany(school => school.FeatureUnlocks
                         .Where(featureUnlock => featureUnlock.level == fromLevel)))
        {
            featureUnlock.level = toLevel;
        }

        // handle level 2 grants
        var featuresGrantedAt2 = new[]
        {
            ("DomainBattle", "PowerDomainBattleDecisiveStrike"), //
            ("DomainDefiler", "FeatureSetDomainDefilerDefileLife"), // UB
            ("DomainElementalCold", "PowerDomainElementalIceLance"), //
            ("DomainElementalFire", "PowerDomainElementalFireBurst"), //
            ("DomainElementalLighting", "PowerDomainElementalLightningBlade"), //
            ("DomainInsight", "PowerDomainInsightForeknowledge"), //
            ("DomainLaw", "PowerDomainLawForceOfLaw"), //
            ("DomainLife", "PowerDomainLifePreserveLife"), //
            ("DomainMischief", "PowerDomainMischiefStrikeOfChaos"), //
            ("DomainNature", "FeatureSetDomainNatureCharmAnimalsAndPlants"), // UB
            ("DomainOblivion", "CampAffinityDomainOblivionPeacefulRest"), //
            ("DomainOblivion", "PowerDomainOblivionHeraldOfPain"), //
            ("DomainSmith", "FeatureSetDomainSmithDefileLife"), // UB
            ("DomainSun", "PowerDomainSunHeraldOfTheSun"), //
            ("DomainTempest", "FeatureSetDomainTempestDestructiveWrath") // UB
        };

        var level = Main.Settings.EnableClericToLearnDomainAtLevel3 ? 3 : 2;

        foreach (var (subClassName, featureName) in featuresGrantedAt2)
        {
            var subClass = GetDefinition<CharacterSubclassDefinition>(subClassName);
            var feature = GetDefinition<FeatureDefinition>(featureName);

            subClass.FeatureUnlocks.FirstOrDefault(x => x.FeatureDefinition == feature)!.level = level;
        }

        // change spell casting level on Cleric itself
        Cleric.FeatureUnlocks
            .FirstOrDefault(x =>
                x.FeatureDefinition == SubclassChoiceClericDivineDomains)!.level = toLevel;

        foreach (var domain in domains)
        {
            domain.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
        }

        Cleric.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    private sealed class ModifyAbilityCheckThaumaturge : IModifyAbilityCheck
    {
        public void MinRoll(
            RulesetCharacter character,
            int baseBonus,
            string abilityScoreName,
            string proficiencyName,
            List<RuleDefinitions.TrendInfo> advantageTrends,
            List<RuleDefinitions.TrendInfo> modifierTrends,
            ref int rollModifier, ref int minRoll)
        {
            if (abilityScoreName is not AttributeDefinitions.Intelligence ||
                proficiencyName is not (SkillDefinitions.Arcana or SkillDefinitions.Religion))
            {
                return;
            }

            var wisdom = character.TryGetAttributeValue(AttributeDefinitions.Wisdom);
            var wisMod = AttributeDefinitions.ComputeAbilityScoreModifier(wisdom);
            var modifier = Math.Max(wisMod, 1);

            rollModifier += modifier;

            modifierTrends.Add(new RuleDefinitions.TrendInfo(modifier,
                RuleDefinitions.FeatureSourceType.CharacterFeature,
                PointPoolClericThaumaturgeCantrip.Name, PointPoolClericThaumaturgeCantrip));
        }
    }
}
