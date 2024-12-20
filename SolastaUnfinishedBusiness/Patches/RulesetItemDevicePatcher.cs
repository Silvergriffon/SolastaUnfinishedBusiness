using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using static ActionDefinitions;
using static RuleDefinitions;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class RulesetItemDevicePatcher
{
    [HarmonyPatch(typeof(RulesetItemDevice), nameof(RulesetItemDevice.IsFunctionAvailable))]
    [UsedImplicitly]
    public static class IsFunctionAvailable_Patch
    {
        [UsedImplicitly]
        public static void Postfix(
            ref bool __result,
            RulesetDeviceFunction function,
            RulesetCharacter character)
        {
            //PATCH: fix vanilla bug allowing infinite casts of bonus action scrolls
            var scroll = function.UsableItemDefinition.UsableDeviceDescription.UsableDeviceTags.Contains("Scroll");

            if (scroll && function.DeviceFunctionDescription.SpellDefinition.ActivationTime == ActivationTime.BonusAction)
            {
                __result = GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Bonus) != ActionStatus.Spent &&
                            GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Bonus) != ActionStatus.Unavailable &&
                                !GameLocationCharacter.GetFromActor(character).UsedMainSpell;
                return;
            }

            if (scroll && function.DeviceFunctionDescription.SpellDefinition.ActivationTime == ActivationTime.Action)
            {
                __result = GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Main) != ActionStatus.Spent &&
                            GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Main) != ActionStatus.Unavailable &&
                                !GameLocationCharacter.GetFromActor(character).UsedBonusSpell;
                return;
            }

            var power = function.DeviceFunctionDescription?.FeatureDefinitionPower;

            if (!power)
            {
                return;
            }

            //PATCH: avoids infinite use of power devices with bonus actions enabled 
            if (function.DeviceFunctionDescription.FeatureDefinitionPower.ActivationTime == ActivationTime.BonusAction)
            {
                __result = GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Bonus) != ActionStatus.Spent &&
                            GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Bonus) != ActionStatus.Unavailable;

                return;
            }

            if (function.DeviceFunctionDescription.FeatureDefinitionPower.ActivationTime == ActivationTime.Action)
            {
                __result = GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Main) != ActionStatus.Spent &&
                            GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Main) != ActionStatus.Unavailable;

                return;
            }

            __result = character.CanUsePower(power, false);
        }
    }
}
