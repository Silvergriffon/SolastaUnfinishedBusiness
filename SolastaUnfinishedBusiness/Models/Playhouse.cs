using System.Linq;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Behaviors.Specific;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;


namespace SolastaUnfinishedBusiness.Models;

internal class Playhouse
{
    internal static void LateLoad()
    {
        UnlockBackerItems();
    }


    private static void UnlockBackerItems()
    {
        GamingPlatformManager.automaticallyUnlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.BackerItems);
        GamingPlatformManager.automaticallyUnlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.DigitalBackerContent);
        GamingPlatformManager.automaticallyUnlockedContentPacks.Add(GamingPlatformDefinitions.ContentPack.LoadedDice);

    }

}
