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
            //PATCH: Attempt to fix vanilla bug allowing infinite casts of bonus action scrolls
            var scroll = function.UsableItemDefinition.UsableDeviceDescription.UsableDeviceTags.Contains("Scroll");

            if (scroll && function.DeviceFunctionDescription.SpellDefinition.ActivationTime == ActivationTime.BonusAction)
            {
                __result = GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Bonus) != ActionStatus.Spent &&
                    GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Bonus) != ActionStatus.Unavailable;
                return;
            }

            if (scroll && function.DeviceFunctionDescription.SpellDefinition.ActivationTime == ActivationTime.Action)
            {
                __result = GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Main) != ActionStatus.Spent &&
                    GameLocationCharacter.GetFromActor(character).GetActionTypeStatus(ActionType.Main) != ActionStatus.Unavailable;
                return;
            }

            if (!__result)
            {
                return;
            }

            var power = function.DeviceFunctionDescription?.FeatureDefinitionPower;

            if (!power)
            {
                return;
            }

            __result = character.CanUsePower(power, false);
        }
    }
}
