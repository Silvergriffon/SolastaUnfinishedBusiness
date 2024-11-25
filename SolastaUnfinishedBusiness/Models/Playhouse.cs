using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ItemDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.LootPackDefinitions;

namespace SolastaUnfinishedBusiness.Models;
internal class Playhouse
{
    internal static void LateLoad()
    {
        TorsoNoDefaultVisual();
        UnlockBackerItems();
     }

    internal static void TorsoNoDefaultVisual()
    {
            DatabaseHelper.SlotTypeDefinitions.TorsoSlot.hasDefaultVisual = !Main.Settings.DisableTorsoDefaultVisuals;
    }

    private static void UnlockBackerItems()
    {
        Backers_Lootpack_Items.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;
        Backers_Lootpack_Items_B_Cumulative.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;
        Backers_Lootpack_Items_C_Cumulative.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;

        foreach (var item in DatabaseRepository.GetDatabase<ItemDefinition>()
                    .Where(x => x.ContentPack == GamingPlatformDefinitions.ContentPack.BackerItems ||
                    x.ContentPack == GamingPlatformDefinitions.ContentPack.DigitalBackerContent ||
                    x.ContentPack == GamingPlatformDefinitions.ContentPack.LoadedDice))
        {
            item.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;
        }

        DatabaseHelper.SpellDefinitions.BurningHands_B.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;
        DatabaseHelper.SpellDefinitions.SacredFlame_B.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;           
    } 
}
