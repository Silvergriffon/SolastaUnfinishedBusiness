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
            if (!__result)
            {
                return;
            }

            var power = function.DeviceFunctionDescription?.FeatureDefinitionPower;

            if (!power)
            {
                return;
            }

            //PATCH: avoids infinite use of potions with 2024 potion bonus actions enabled 
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
