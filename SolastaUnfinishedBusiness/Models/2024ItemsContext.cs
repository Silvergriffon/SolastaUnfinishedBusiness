using System.Linq;

namespace SolastaUnfinishedBusiness.Models;

internal static partial class Tabletop2024Context
{
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
            foreach (var poison in DatabaseRepository.GetDatabase<FeatureDefinitionPower>()
                         .Where(a =>
                             a.Name.StartsWith("PowerFunctionApplyPoison")))
            {
                poison.activationTime = RuleDefinitions.ActivationTime.BonusAction;
            }
        }
        else
        {
            foreach (var poison in DatabaseRepository.GetDatabase<FeatureDefinitionPower>()
                         .Where(a =>
                             a.Name.StartsWith("PowerFunctionApplyPoison")))
            {
                poison.activationTime = RuleDefinitions.ActivationTime.Action;
            }
        }
    }
}
