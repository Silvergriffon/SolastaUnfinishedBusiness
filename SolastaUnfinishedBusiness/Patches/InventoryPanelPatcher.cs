﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Models;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class InventoryPanelPatcher
{
    //PATCH: Enable Inventory Filtering and Sorting
    [HarmonyPatch(typeof(InventoryPanel), nameof(InventoryPanel.OnBeginShow))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class OnBeginShow_Patch
    {
        [UsedImplicitly]
        public static void Prefix(InventoryPanel __instance)
        {
            InventoryManagementContext.RefreshControlsVisibility();

            if (InventoryManagementContext.Enabled && __instance.MainContainerPanel)
            {
                InventoryManagementContext.Refresh(__instance.MainContainerPanel, true);
            }
        }
    }

    //PATCH: Enable Inventory Filtering and Sorting
    [HarmonyPatch(typeof(InventoryPanel), nameof(InventoryPanel.Bind))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Bind_Patch
    {
        [UsedImplicitly]
        public static void Prefix(InventoryPanel __instance)
        {
            // NOTE: don't use MainContainerPanel?. which bypasses Unity object lifetime check
            if (InventoryManagementContext.Enabled && __instance.MainContainerPanel)
            {
                InventoryManagementContext.BindInventory(__instance.MainContainerPanel);
            }
        }
    }

    //PATCH: Enable Inventory Filtering and Sorting
    [HarmonyPatch(typeof(InventoryPanel), nameof(InventoryPanel.Unbind))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class Unbind_Patch
    {
        [UsedImplicitly]
        public static void Prefix(InventoryPanel __instance)
        {
            if (__instance.MainContainerPanel)
            {
                InventoryManagementContext.UnbindInventory(__instance.MainContainerPanel);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryPanel), nameof(InventoryPanel.RefreshSlotsList))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class RefreshSlotsList_Patch
    {
        [UsedImplicitly]
        public static void Postfix(InventoryPanel __instance)
        {
            //PATCH: support for customized filtering of items for ItemProperty effect form
            CustomItemFilter.FilterItems(__instance);
        }
    }

    //PATCH: enable CTRL click-drag to bypass quest items checks on drop
    [HarmonyPatch(typeof(InventoryPanel), nameof(InventoryPanel.EndInteraction))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class EndInteraction_Patch
    {
        [UsedImplicitly]
        public static void Prefix(InventoryPanel __instance, out bool __state)
        {
            __state = SettingsContext.InputModManagerInstance.EnableCtrlClickDragToBypassQuestItemsOnDrop &&
                      (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                      __instance.DraggedItem.ItemDefinition.ItemTags.Remove(TagsDefinitions.ItemTagQuest);
        }

        [UsedImplicitly]
        public static void Postfix(InventoryPanel __instance, bool __state)
        {
            if (__state)
            {
                __instance.DraggedItem.ItemDefinition.ItemTags.Add(TagsDefinitions.ItemTagQuest);
            }
        }
    }
}
