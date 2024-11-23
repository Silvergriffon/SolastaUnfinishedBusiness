using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolastaUnfinishedBusiness.Api;

namespace SolastaUnfinishedBusiness.Models;
internal class Playhouse
{
    internal static void LateLoad()
    {
        TorsoNoDefaultVisual();
        UnlockBackerItems();
     }

    private static void TorsoNoDefaultVisual()
    {
        DatabaseHelper.SlotTypeDefinitions.TorsoSlot.hasDefaultVisual = false;
    }

    private static void UnlockBackerItems()
    {
        DatabaseHelper.LootPackDefinitions.Backers_Lootpack_Items.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;
        DatabaseHelper.LootPackDefinitions.Backers_Lootpack_Items_B_Cumulative.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;
        DatabaseHelper.LootPackDefinitions.Backers_Lootpack_Items_C_Cumulative.contentPack = GamingPlatformDefinitions.ContentPack.BaseGame;

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
