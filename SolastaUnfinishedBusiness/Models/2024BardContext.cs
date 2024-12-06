﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using static ActionDefinitions;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPowers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.CharacterClassDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionRestHealingModifiers;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionPointPools;

namespace SolastaUnfinishedBusiness.Models;

internal static partial class Tabletop2024Context
{
    private static readonly ConditionDefinition ConditionBardCounterCharmSavingThrowAdvantage =
        ConditionDefinitionBuilder
            .Create("ConditionBardCounterCharmSavingThrowAdvantage")
            .SetGuiPresentation(PowerBardCountercharm.GuiPresentation)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .SetFeatures(
                FeatureDefinitionSavingThrowAffinityBuilder
                    .Create(FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinityAdvantageToAll,
                        "SavingThrowAffinityBardCounterCharmAdvantage")
                    .SetGuiPresentation(PowerBardCountercharm.GuiPresentation)
                    .AddToDB())
            .SetSpecialInterruptions(ConditionInterruption.SavingThrow)
            .AddToDB();

    private static void LoadOneDndEnableBardCounterCharmAsReactionAtLevel7()
    {
        PowerBardCountercharm.AddCustomSubFeatures(
            new ModifyPowerVisibility((_, _, _) => !Main.Settings.EnableBardCounterCharmAsReactionAtLevel7),
            new TryAlterOutcomeSavingThrowBardCounterCharm());
    }

    internal static void SwitchOneDndEnableBardCounterCharmAsReactionAtLevel7()
    {
        var level = Main.Settings.EnableBardCounterCharmAsReactionAtLevel7 ? 7 : 6;

        Bard.FeatureUnlocks.FirstOrDefault(x => x.FeatureDefinition == PowerBardCountercharm)!.level = level;
        if (Main.Settings.EnableBardCounterCharmAsReactionAtLevel7)
        {
            PowerBardCountercharm.GuiPresentation.description = "Feature/&PowerBardCountercharmExtendedDescription";
            PowerBardCountercharm.activationTime = ActivationTime.NoCost;
        }
        else
        {
            PowerBardCountercharm.GuiPresentation.description = "Feature/&PowerBardCountercharmDescription";
            PowerBardCountercharm.activationTime = ActivationTime.Action;
        }

        Bard.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchOneDndEnableBardExpertiseOneLevelBefore()
    {
        var level = Main.Settings.EnableBardExpertiseOneLevelBefore ? 2 : 3;

        foreach (var featureUnlock in Bard.FeatureUnlocks
                     .Where(x => x.FeatureDefinition == PointPoolBardExpertiseLevel3))
        {
            featureUnlock.level = level;
        }

        level = Main.Settings.EnableBardExpertiseOneLevelBefore ? 9 : 10;

        foreach (var featureUnlock in Bard.FeatureUnlocks
                     .Where(x => x.FeatureDefinition == PointPoolBardExpertiseLevel10))
        {
            featureUnlock.level = level;
        }

        if (Main.Settings.EnableBardExpertiseOneLevelBefore)
        {
            PointPoolBardExpertiseLevel3.GuiPresentation.description = "Feature/&BardExpertiseExtendedDescription";
            PointPoolBardExpertiseLevel10.GuiPresentation.description = "Feature/&BardExpertiseExtendedDescription";
        }
        else
        {
            PointPoolBardExpertiseLevel3.GuiPresentation.description = "Feature/&BardExpertiseDescription";
            PointPoolBardExpertiseLevel10.GuiPresentation.description = "Feature/&BardExpertiseDescription";
        }

        Bard.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchOneDndChangeBardicInspirationDurationToOneHour()
    {
        if (Main.Settings.ChangeBardicInspirationDurationToOneHour)
        {
            ConditionDefinitions.ConditionBardicInspiration.durationType = DurationType.Hour;
            ConditionDefinitions.ConditionBardicInspiration.durationParameter = 1;
        }
        else
        {
            ConditionDefinitions.ConditionBardicInspiration.durationType = DurationType.Minute;
            ConditionDefinitions.ConditionBardicInspiration.durationParameter = 10;
        }
    }

    internal static void SwitchOneDndRemoveBardSongOfRestAt2()
    {
        Bard.FeatureUnlocks.RemoveAll(x =>
            x.FeatureDefinition == RestHealingModifierBardSongOfRest);

        if (!Main.Settings.RemoveBardSongOfRestAt2)
        {
            Bard.FeatureUnlocks.Add(new FeatureUnlockByLevel(RestHealingModifierBardSongOfRest, 2));
        }

        Bard.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchOneDndRemoveBardMagicalSecretAt14And18()
    {
        Bard.FeatureUnlocks.RemoveAll(x =>
            x.FeatureDefinition == PointPoolBardMagicalSecrets14 ||
            x.FeatureDefinition == Level20Context.PointPoolBardMagicalSecrets18);

        if (!Main.Settings.RemoveBardMagicalSecretAt14And18)
        {
            Bard.FeatureUnlocks.AddRange(
                new FeatureUnlockByLevel(PointPoolBardMagicalSecrets14, 14),
                new FeatureUnlockByLevel(Level20Context.PointPoolBardMagicalSecrets18, 18));
        }

        Bard.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchOneDndEnableBardSuperiorInspirationAtLevel18()
    {
        Bard.FeatureUnlocks.RemoveAll(x =>
            x.FeatureDefinition == Level20Context.FeatureBardSuperiorInspiration ||
            x.FeatureDefinition == Level20Context.FeatureBardSuperiorInspiration2024);

        Bard.FeatureUnlocks.Add(
            Main.Settings.EnableBardSuperiorInspirationAtLevel18
                ? new FeatureUnlockByLevel(Level20Context.FeatureBardSuperiorInspiration2024, 18)
                : new FeatureUnlockByLevel(Level20Context.FeatureBardSuperiorInspiration, 20));

        Bard.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    internal static void SwitchOneDndEnableBardWordsOfCreationAtLevel20()
    {
        Bard.FeatureUnlocks.RemoveAll(x =>
            x.FeatureDefinition == Level20Context.AutoPreparedSpellsBardWordOfCreation);

        if (Main.Settings.EnableBardWordsOfCreationAtLevel20)
        {
            Bard.FeatureUnlocks.Add(
                new FeatureUnlockByLevel(Level20Context.AutoPreparedSpellsBardWordOfCreation, 20));
        }

        Bard.FeatureUnlocks.Sort(Sorting.CompareFeatureUnlock);
    }

    private sealed class TryAlterOutcomeSavingThrowBardCounterCharm : ITryAlterOutcomeSavingThrow
    {
        public IEnumerator OnTryAlterOutcomeSavingThrow(
            GameLocationBattleManager battleManager,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            GameLocationCharacter helper,
            SavingThrowData savingThrowData,
            bool hasHitVisual)
        {
            if (!Main.Settings.EnableBardCounterCharmAsReactionAtLevel7)
            {
                yield break;
            }

            if (savingThrowData.SaveOutcome != RollOutcome.Success &&
                !helper.IsOppositeSide(defender.Side) &&
                helper.CanReact() &&
                helper.IsWithinRange(defender, 6) &&
                HasCharmedOrFrightened(savingThrowData.EffectDescription.EffectForms))
            {
                yield return helper.MyReactToDoNothing(
                    ExtraActionId.DoNothingFree, // cannot use DoNothingReaction here as we reroll in validate
                    defender,
                    "BardCounterCharm",
                    FormatReactionDescription(savingThrowData.Title, attacker, defender, helper),
                    ReactionValidated);
            }

            yield break;

            static bool HasCharmedOrFrightened(List<EffectForm> effectForms)
            {
                return effectForms.Any(x =>
                    x.FormType == EffectForm.EffectFormType.Condition &&
                    (x.ConditionForm.ConditionDefinition.IsSubtypeOf(ConditionDefinitions.ConditionCharmed.Name) ||
                     x.ConditionForm.ConditionDefinition.IsSubtypeOf(ConditionDefinitions.ConditionFrightened.Name)));
            }

            void ReactionValidated()
            {
                var rulesetDefender = defender.RulesetCharacter;

                rulesetDefender.InflictCondition(
                    ConditionBardCounterCharmSavingThrowAdvantage.Name,
                    DurationType.Round,
                    1,
                    TurnOccurenceType.StartOfTurn,
                    AttributeDefinitions.TagEffect,
                    rulesetDefender.guid,
                    rulesetDefender.CurrentFaction.Name,
                    1,
                    ConditionBardCounterCharmSavingThrowAdvantage.Name,
                    0,
                    0,
                    0);

                // we need to manually spend the reaction here as rolling the saving again below
                helper.SpendActionType(ActionType.Reaction);
                helper.RulesetCharacter.LogCharacterUsedPower(PowerBardCountercharm);
                EffectHelpers.StartVisualEffect(helper, defender, PowerBardCountercharm,
                    EffectHelpers.EffectType.Caster);
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

            return $"CustomReactionBardCounterCharmDescription{text}".Formatted(
                Category.Reaction, defender.Name, attacker?.Name ?? ReactionRequestCustom.EnvTitle, sourceTitle);
        }
    }
}
