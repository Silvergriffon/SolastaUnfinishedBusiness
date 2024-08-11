﻿using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.CustomUI;
using UnityEngine;
using static ActionDefinitions;

namespace SolastaUnfinishedBusiness.Models;

internal static class CustomReactionsContext
{
    private static bool _forcePreferredCantrip; //used by actual feature
    private static bool _forcePreferredCantripUI; //used for local UI state

    internal static void Load()
    {
        MakeReactDefinition(ReactionRequestWarcaster.Name);
        MakeReactDefinition(ReactionRequestSpendBundlePower.Name);
    }

    private static void MakeReactDefinition(string name)
    {
        ReactionDefinitionBuilder
            .Create(name)
            .SetGuiPresentation(Category.Reaction)
            .AddToDB();
    }

    internal static void SaveReadyActionPreferredCantrip(
        [CanBeNull] CharacterActionParams actionParams,
        ReadyActionType readyActionType)
    {
        if (actionParams != null && readyActionType == ReadyActionType.Cantrip)
        {
            actionParams.BoolParameter4 = _forcePreferredCantripUI;
        }
    }

    internal static void ReadReadyActionPreferredCantrip(CharacterActionParams actionParams)
    {
        if (actionParams is { ReadyActionType: ReadyActionType.Cantrip })
        {
            _forcePreferredCantrip = actionParams.BoolParameter4;
        }
    }

    internal static void SetupForcePreferredToggle(RectTransform parent)
    {
        PersonalityFlagToggle toggle;

        if (parent.childCount < 3)
        {
            // ReSharper disable once Unity.UnknownResource
            var prefab = Resources.Load<GameObject>("Gui/Prefabs/CharacterEdition/PersonalityFlagToggle");
            var asset = Object.Instantiate(prefab, parent, false);

            asset.name = "ForcePreferredToggle";

            var transform = asset.GetComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.anchoredPosition = new Vector2(0f, 1);
            transform.localPosition = new Vector3(0, -30);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);

            var title = parent.GetChild(0);

            title.localPosition = new Vector3(-100, 55);

            var group = parent.GetChild(1).GetComponent<RectTransform>();

            group.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
            group.localPosition = new Vector3(-100, 5);

            toggle = asset.GetComponent<PersonalityFlagToggle>();

            var guiLabel = toggle.titleLabel;

            guiLabel.Text = "UI/&ForcePreferredCantripTitle";

            var tooltip = toggle.tooltip;

            tooltip.Content = "UI/&ForcePreferredCantripDescription";

            toggle.PersonalityFlagDefinition = DatabaseHelper.GetDefinition<PersonalityFlagDefinition>("Authority");

            toggle.PersonalityFlagSelected = (_, _, state) =>
            {
                _forcePreferredCantripUI = state;
                tooltip.Content = "UI/&ForcePreferredCantripDescription";
            };
        }
        else
        {
            toggle = parent.FindChildRecursive("ForcePreferredToggle").GetComponent<PersonalityFlagToggle>();
        }

        toggle.Refresh(_forcePreferredCantripUI, true);
        toggle.tooltip.Content = "UI/&ForcePreferredCantripDescription";
    }

    internal static bool CheckAndModifyCantrips(List<SpellDefinition> readied, SpellDefinition preferred)
    {
        if (!_forcePreferredCantrip)
        {
            return readied.Contains(preferred);
        }

        readied.RemoveAll(c => c != preferred);

        return readied.Count != 0;
    }
}
