﻿using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Models;
using static SolastaUnfinishedBusiness.Models.CraftingContext;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ItemDefinitions;

namespace SolastaUnfinishedBusiness.ItemCrafting;

internal static class ItemRecipeGenerationHelper
{
    internal static void AddRecipesFromItemCollection(ItemCollection itemCollection, bool isArmor = false)
    {
        foreach (var baseItem in itemCollection.BaseWeapons)
        {
            foreach (var itemData in itemCollection.MagicToCopy)
            {
                // Generate new items
                var newItem = isArmor
                    ? ItemBuilder.BuildNewMagicArmor(baseItem, itemData.Name, itemData.Item)
                    : ItemBuilder.BuildNewMagicWeapon(baseItem, itemData.Name, itemData.Item);
                var ingredients = new List<ItemDefinition> { baseItem };

                ingredients.AddRange(itemData.Recipe.Ingredients
                    .Where(ingredient =>
                        !itemCollection.PossiblePrimedItemsToReplace.Contains(ingredient.ItemDefinition))
                    .Select(x => x.ItemDefinition));

                var craftingManual = RecipeHelper.BuildRecipeManual(newItem, itemData.Recipe.CraftingHours,
                    itemData.Recipe.CraftingDC, ingredients.ToArray());

                if (!RecipeBooks.ContainsKey(baseItem.Name))
                {
                    RecipeBooks.Add(baseItem.Name, new List<ItemDefinition>());
                }

                RecipeBooks[baseItem.Name].Add(craftingManual);

                if (!Main.Settings.CraftingInStore.Contains(baseItem.Name))
                {
                    continue;
                }

                MerchantContext.AddItem(craftingManual, ShopItemType.ShopCrafting);
            }
        }
    }

    internal static void AddIngredientEnchanting()
    {
        var recipes = new List<RecipeDefinition>();
        var enchantedToIngredient = new Dictionary<ItemDefinition, ItemDefinition>
        {
            { Ingredient_Enchant_MithralStone, _300_GP_Opal },
            { Ingredient_Enchant_Crystal_Of_Winter, _100_GP_Pearl },
            { Ingredient_Enchant_Blood_Gem, _500_GP_Ruby },
            { Ingredient_Enchant_Soul_Gem, _1000_GP_Diamond },
            { Ingredient_Enchant_Slavestone, _100_GP_Emerald },
            { Ingredient_Enchant_Cloud_Diamond, _1000_GP_Diamond },
            { Ingredient_Enchant_Stardust, _100_GP_Pearl },
            { Ingredient_Enchant_Doom_Gem, _50_GP_Sapphire },
            { Ingredient_Enchant_Shard_Of_Fire, _500_GP_Ruby },
            { Ingredient_Enchant_Shard_Of_Ice, _50_GP_Sapphire },
            { Ingredient_Enchant_LifeStone, _1000_GP_Diamond },
            { Ingredient_Enchant_Diamond_Of_Elai, _100_GP_Emerald },
            { Ingredient_PrimordialLavaStones, _20_GP_Amethyst },
            { Ingredient_Enchant_Blood_Of_Solasta, Ingredient_Acid },
            { Ingredient_Enchant_Medusa_Coral, _300_GP_Opal },
            { Ingredient_Enchant_PurpleAmber, _50_GP_Sapphire },
            { Ingredient_Enchant_Heartstone, _300_GP_Opal },
            { Ingredient_Enchant_SpiderQueen_Venom, Ingredient_BadlandsSpiderVenomGland }
        };

        foreach (var item in enchantedToIngredient.Keys)
        {
            var recipeName = "RecipeEnchant" + item.Name;

            recipes.Add(RecipeDefinitionBuilder
                .Create(recipeName)
                .SetGuiPresentation(item.GuiPresentation)
                .AddIngredients(enchantedToIngredient[item])
                .SetCraftedItem(item)
                .SetCraftingCheckData(16, 16, DatabaseHelper.ToolTypeDefinitions.EnchantingToolType)
                .AddToDB());
        }

        const string GROUP_KEY = "EnchantingIngredients";

        RecipeBooks.Add(GROUP_KEY, new List<ItemDefinition>());

        foreach (var craftingManual in recipes
                     .Select(recipe => ItemBuilder.BuilderCopyFromItemSetRecipe(
                         CraftingManualRemedy,
                         "CraftingManual" + recipe.Name,
                         recipe,
                         Main.Settings.RecipeCost,
                         CraftingManualRemedy.GuiPresentation)))
        {
            RecipeBooks[GROUP_KEY].Add(craftingManual);

            if (!Main.Settings.CraftingInStore.Contains(GROUP_KEY))
            {
                continue;
            }

            MerchantContext.AddItem(craftingManual, ShopItemType.ShopCrafting);
        }
    }

    internal static void AddPrimingRecipes()
    {
        var primedToBase = new Dictionary<ItemDefinition, ItemDefinition>
        {
            { Primed_Battleaxe, Battleaxe },
            { Primed_Breastplate, Breastplate },
            { Primed_ChainMail, ChainMail },
            { Primed_ChainShirt, ChainShirt },
            { Primed_Dagger, Dagger },
            { Primed_Greataxe, Greataxe },
            { Primed_Greatsword, Greatsword },
            { Primed_HalfPlate, HalfPlate },
            { Primed_HeavyCrossbow, HeavyCrossbow },
            { Primed_HideArmor, HideArmor },
            { Primed_LeatherDruid, LeatherDruid },
            { Primed_Leather_Armor, Leather },
            { Primed_LightCrossbow, LightCrossbow },
            { Primed_Longbow, Longbow },
            { Primed_Longsword, Longsword },
            { Primed_Mace, Mace },
            { Primed_Maul, Maul },
            { Primed_Morningstar, Morningstar },
            { Primed_Plate, Plate },
            { Primed_Rapier, Rapier },
            { Primed_ScaleMail, ScaleMail },
            { Primed_Scimitar, Scimitar },
            { Primed_Shortbow, Shortbow },
            { Primed_Shortsword, Shortsword },
            { Primed_Spear, Spear },
            { Primed_StuddedLeather, StuddedLeather },
            { Primed_Warhammer, Warhammer }
        };
        var recipes = primedToBase.Keys.Select(item => RecipeHelper.BuildPrimeRecipe(primedToBase[item], item));

        const string GROUP_KEY = "PrimedItems";

        RecipeBooks.Add(GROUP_KEY, new List<ItemDefinition>());

        foreach (var recipe in recipes)
        {
            var craftingManual = ItemBuilder.BuilderCopyFromItemSetRecipe(
                CraftingManual_Enchant_Longsword_Warden,
                "CraftingManual" + recipe.Name,
                recipe, Main.Settings.RecipeCost,
                CraftingManual_Enchant_Longsword_Warden.GuiPresentation);

            RecipeBooks[GROUP_KEY].Add(craftingManual);

            if (!Main.Settings.CraftingInStore.Contains(GROUP_KEY))
            {
                continue;
            }

            MerchantContext.AddItem(craftingManual, ShopItemType.ShopCrafting);
        }
    }

    internal static void AddFactionItems()
    {
        var recipes = new List<RecipeDefinition>();
        var forgeryToIngredient = new Dictionary<ItemDefinition, ItemDefinition>
        {
            { CAERLEM_TirmarianHolySymbol, Art_Item_50_GP_JadePendant },
            { BONEKEEP_MagicRune, Art_Item_25_GP_EngraveBoneDice },
            { CaerLem_Gate_Plaque, Art_Item_25_GP_SilverChalice }
        };

        foreach (var item in forgeryToIngredient.Keys)
        {
            var recipeName = "RecipeForgery" + item.Name;

            recipes.Add(RecipeDefinitionBuilder
                .Create(recipeName)
                .AddIngredients(forgeryToIngredient[item])
                .SetCraftedItem(item)
                .SetCraftingCheckData(16, 16, DatabaseHelper.ToolTypeDefinitions.ArtisanToolSmithToolsType)
                .SetGuiPresentation(item.GuiPresentation)
                .AddToDB());
        }

        var scrollForgeries = new Dictionary<ItemDefinition, ItemDefinition>
        {
            { BONEKEEP_AkshasJournal, Ingredient_AngryViolet },
            { ABJURATION_TOWER_Manifest, Ingredient_ManacalonOrchid },
            { ABJURATION_MastersmithLoreDocument, Ingredient_RefinedOil },
            { CAERLEM_Inquisitor_Document, Ingredient_AbyssMoss },
            { ABJURATION_TOWER_Poem, Ingredient_LilyOfTheBadlands },
            { ABJURATION_TOWER_ElvenWars, Ingredient_BloodDaffodil },
            { CAERLEM_Daliat_Document, Ingredient_Skarn }
        };

        foreach (var item in scrollForgeries.Keys)
        {
            var recipeName = "RecipeForgery" + item.Name;

            recipes.Add(RecipeDefinitionBuilder
                .Create(recipeName)
                .AddIngredients(scrollForgeries[item])
                .SetCraftedItem(item)
                .SetCraftingCheckData(16, 16, DatabaseHelper.ToolTypeDefinitions.ScrollKitType)
                .SetGuiPresentation(item.GuiPresentation)
                .AddToDB());
        }

        const string GROUP_KEY = "RelicForgeries";

        RecipeBooks.Add(GROUP_KEY, new List<ItemDefinition>());

        foreach (var craftingManual in recipes.Select(recipe => ItemBuilder.BuilderCopyFromItemSetRecipe(
                     CraftingManualRemedy, "CraftingManual" + recipe.Name,
                     recipe, Main.Settings.RecipeCost,
                     CraftingManualRemedy.GuiPresentation)))
        {
            RecipeBooks[GROUP_KEY].Add(craftingManual);

            if (!Main.Settings.CraftingInStore.Contains(GROUP_KEY))
            {
                continue;
            }

            MerchantContext.AddItem(craftingManual, ShopItemType.ShopCrafting);
        }
    }
}
