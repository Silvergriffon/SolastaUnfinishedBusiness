using System.Linq;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.Validators;
using static ActionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ItemDefinitions;

namespace SolastaUnfinishedBusiness.Models;

internal static partial class Tabletop2024Context
{
    private static readonly FeatureDefinitionActionAffinity ActionAffinityPoisonBonusAction =
        FeatureDefinitionActionAffinityBuilder
            .Create("ActionAffinityPoisonBonusAction")
            .SetGuiPresentationNoContent(true)
            .AddCustomSubFeatures(
                new ValidateDeviceFunctionUse((_, device, _) =>
                    device.UsableDeviceDescription.UsableDeviceTags.Contains("Poison")))
            .SetAuthorizedActions(Id.UseItemBonus)
            .AddToDB();

    private static readonly ItemPropertyDescription ItemPropertyPoisonBonusAction =
        new(RingFeatherFalling.StaticProperties[0])
        {
            appliesOnItemOnly = false,
            type = ItemPropertyDescription.PropertyType.Feature,
            featureDefinition = ActionAffinityPoisonBonusAction,
            conditionDefinition = null,
            knowledgeAffinity = EquipmentDefinitions.KnowledgeAffinity.ActiveAndHidden
        };

    internal static void SwitchPotionsBonusAction()
    {
        if (Main.Settings.EnablePotionsBonusAction2024)
        {
            foreach (var power in DatabaseRepository.GetDatabase<FeatureDefinitionPower>()
                         .Where(a =>
                             a.Name == "PowerFunctionAntitoxin" ||
                             a.Name.StartsWith("PowerFunctionPotion")))
            {
                power.activationTime = RuleDefinitions.ActivationTime.BonusAction;
            }
        }
        else
        {
            foreach (var power in DatabaseRepository.GetDatabase<FeatureDefinitionPower>()
                         .Where(a =>
                             a.Name == "PowerFunctionAntitoxin" ||
                             a.Name.StartsWith("PowerFunctionPotion")))
            {
                power.activationTime = RuleDefinitions.ActivationTime.Action;
            }
        }
    }

    internal static void SwitchPoisonsBonusAction()
    {
        if (Main.Settings.EnablePoisonsBonusAction2024)
        {
            foreach (var poison in DatabaseRepository.GetDatabase<ItemDefinition>()
                         .Where(a =>
                             a.UsableDeviceDescription != null &&
                             a.UsableDeviceDescription.usableDeviceTags.Contains("Poison")))
            {
                poison.StaticProperties.TryAdd(ItemPropertyPoisonBonusAction);
            }
        }
        else
        {
            foreach (var poison in DatabaseRepository.GetDatabase<ItemDefinition>()
                         .Where(a =>
                             a.UsableDeviceDescription != null &&
                             a.UsableDeviceDescription.usableDeviceTags.Contains("Poison")))
            {
                poison.StaticProperties.Clear();
            }
        }
    }
}
