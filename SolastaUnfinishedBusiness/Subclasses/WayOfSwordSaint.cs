﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Behaviors;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.Interfaces;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Patches;
using SolastaUnfinishedBusiness.Properties;
using SolastaUnfinishedBusiness.Validators;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionAttackModifiers;

namespace SolastaUnfinishedBusiness.Subclasses;

[UsedImplicitly]
public sealed class WayOfSwordSaint : AbstractSubclass
{
    private const string Name = "WayOfSwordSaint";

    public WayOfSwordSaint()
    {
        //
        // LEVEL 03
        //

        // Path of The Sword Saint

        // kept name for backward compatibility
        var invocationPoolMonkWeaponSpecialization = CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolMonkWeaponSpecialization")
            .SetGuiPresentation("InvocationPoolMonkWeaponSpecializationLearn", Category.Feature)
            .Setup(InvocationPoolTypeCustom.Pools.MonkWeaponSpecialization, 2)
            .AddToDB();

        // kept name for backward compatibility
        var invocationPoolMonkWeaponSpecializationAdd = CustomInvocationPoolDefinitionBuilder
            .Create("InvocationPoolMonkWeaponSpecializationAdd")
            .SetGuiPresentationNoContent(true)
            .Setup(InvocationPoolTypeCustom.Pools.MonkWeaponSpecialization)
            .AddToDB();
        
        LoadMonkWeaponSpecialization();

        var featureSetPathOfTheSwordSaint = FeatureDefinitionFeatureSetBuilder
            .Create($"FeatureSet{Name}PathOfTheSwordSaint")
            .SetGuiPresentation(Category.Feature)
            .AddCustomSubFeatures(invocationPoolMonkWeaponSpecialization)
            .AddToDB();

        // One With The Blade

        var attributeModifierOneWithTheBlade = FeatureDefinitionAttributeModifierBuilder
            .Create($"AttributeModifier{Name}OneWithTheBlade")
            .SetGuiPresentation(Category.Feature)
            .SetModifier(FeatureDefinitionAttributeModifier.AttributeModifierOperation.Additive,
                AttributeDefinitions.ArmorClass, 2)
            .SetSituationalContext(ExtraSituationalContext.HasMeleeWeaponInMainHandWithFreeOffhand)
            .AddCustomSubFeatures(
                new ModifyWeaponAttackModeOneWithTheBlade(),
                new AddExtraMainHandAttack(
                    ActionDefinitions.ActionType.Bonus,
                    ValidatorsCharacter.HasMeleeWeaponInMainHandAndFreeOffhand,
                    ValidatorsCharacter.HasAnyOfConditions(ConditionMonkMartialArtsUnarmedStrikeBonus)))
            .AddToDB();

        //
        // LEVEL 06
        //

        // Deft Strike

        var featureDeftStrike = FeatureDefinitionBuilder
            .Create($"Feature{Name}DeftStrike")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        //
        // LEVEL 11
        //

        // Swift Strike

        var featureSwiftStrike = FeatureDefinitionBuilder
            .Create($"Feature{Name}SwiftStrike")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        //
        // LEVEL 17
        //

        //  Master of the Blade 

        var featureMasterOfTheBlade = FeatureDefinitionBuilder
            .Create($"Feature{Name}MasterOfTheBlade")
            .SetGuiPresentation(Category.Feature)
            .AddToDB();

        featureMasterOfTheBlade.AddCustomSubFeatures(
            new PhysicalAttackFinishedByMeMasterOfTheBlade(featureMasterOfTheBlade));

        // MAIN

        Subclass = CharacterSubclassDefinitionBuilder
            .Create(Name)
            .SetGuiPresentation(Category.Subclass, Sprites.GetSprite(Name, Resources.WayOfTheSwordSaint, 256))
            .AddFeaturesAtLevel(3, featureSetPathOfTheSwordSaint, attributeModifierOneWithTheBlade)
            .AddFeaturesAtLevel(6, featureDeftStrike, invocationPoolMonkWeaponSpecializationAdd)
            .AddFeaturesAtLevel(11, featureSwiftStrike, invocationPoolMonkWeaponSpecializationAdd)
            .AddFeaturesAtLevel(17, featureMasterOfTheBlade, invocationPoolMonkWeaponSpecializationAdd)
            .AddToDB();
    }

    internal override CharacterClassDefinition Klass => CharacterClassDefinitions.Monk;

    internal override CharacterSubclassDefinition Subclass { get; }

    internal override FeatureDefinitionSubclassChoice SubclassChoice =>
        FeatureDefinitionSubclassChoices.SubclassChoiceMonkMonasticTraditions;

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    internal override DeityDefinition DeityDefinition { get; }

    private static void LoadMonkWeaponSpecialization()
    {
        var weaponTypeDefinitions = new List<WeaponTypeDefinition>
        {
            CustomWeaponsContext.KatanaWeaponType,
            WeaponTypeDefinitions.BattleaxeType,
            WeaponTypeDefinitions.LongswordType,
            WeaponTypeDefinitions.MorningstarType,
            WeaponTypeDefinitions.RapierType,
            WeaponTypeDefinitions.ScimitarType,
            WeaponTypeDefinitions.WarhammerType
        };

        foreach (var weaponTypeDefinition in weaponTypeDefinitions)
        {
            var weaponTypeName = weaponTypeDefinition.Name;

            var featureMonkWeaponSpecialization = FeatureDefinitionProficiencyBuilder
                .Create($"FeatureMonkWeaponSpecialization{weaponTypeName}")
                .SetGuiPresentationNoContent(true)
                .SetProficiencies(ProficiencyType.Weapon, weaponTypeName)
                .AddCustomSubFeatures(new WeaponSpecialization { WeaponType = weaponTypeDefinition })
                .AddToDB();

                featureMonkWeaponSpecialization.AddCustomSubFeatures(
                    new AddTagToWeapon(
                        TagsDefinitions.WeaponTagFinesse,
                        TagsDefinitions.Criticity.Important,
                        ValidatorsWeapon.IsOfWeaponType(weaponTypeDefinition))
                );

            // ensure we get dice upgrade on these
            AttackModifierMonkMartialArtsImprovedDamage.AddCustomSubFeatures(
                new WeaponSpecializationDiceUpgrade(weaponTypeDefinition));

            _ = CustomInvocationDefinitionBuilder
                .Create($"CustomInvocationMonkWeaponSpecialization{weaponTypeName}")
                .SetGuiPresentation(
                    weaponTypeDefinition.GuiPresentation.Title,
                    weaponTypeDefinition.GuiPresentation.Description,
                    CustomWeaponsContext.GetStandardWeaponOfType(weaponTypeDefinition.Name))
                .SetPoolType(InvocationPoolTypeCustom.Pools.MonkWeaponSpecialization)
                .SetGrantedFeature(featureMonkWeaponSpecialization)
                .AddCustomSubFeatures(ModifyInvocationVisibility.Marker)
                .AddToDB();
        }
    }

    //
    // Path of the Sword Saint
    //

    internal sealed class WeaponSpecialization
    {
        internal WeaponTypeDefinition WeaponType { get; set; }
    }

    private sealed class WeaponSpecializationDiceUpgrade : IValidateContextInsteadOfRestrictedProperty
    {
        private readonly WeaponTypeDefinition _weaponTypeDefinition;

        internal WeaponSpecializationDiceUpgrade(WeaponTypeDefinition weaponTypeDefinition)
        {
            _weaponTypeDefinition = weaponTypeDefinition;
        }

        public (OperationType, bool) ValidateContext(
            BaseDefinition definition,
            IRestrictedContextProvider provider,
            RulesetCharacter character,
            ItemDefinition itemDefinition,
            bool rangedAttack, RulesetAttackMode attackMode,
            RulesetEffect rulesetEffect)
        {
            var attackModeWeaponType =
                (attackMode?.SourceDefinition as ItemDefinition)?.WeaponDescription.WeaponTypeDefinition;

            return (OperationType.Or,
                character.GetSubFeaturesByType<WeaponSpecializationDiceUpgrade>().Exists(
                    x => x._weaponTypeDefinition == attackModeWeaponType));
        }
    }

    //
    // One With The Blade
    //

    // set attacks number to 2 to allow a mix of unarmed / melee weapon attacks otherwise game engine will consume bonus action
    // the patch on CharacterActionItemForm.Refresh finishes the trick by hiding the number of attacks with a proper hide tag
    private sealed class ModifyWeaponAttackModeOneWithTheBlade : IModifyWeaponAttackMode
    {
        public void ModifyWeaponAttackMode(
            RulesetCharacter rulesetCharacter,
            RulesetAttackMode attackMode,
            RulesetItem weapon,
            bool canAddAbilityDamageBonus)
        {
            var character = GameLocationCharacter.GetFromActor(rulesetCharacter);

            if (character is not { UsedBonusAttacks: 0 } ||
                attackMode.ActionType != ActionDefinitions.ActionType.Bonus ||
                !rulesetCharacter.HasConditionOfCategoryAndType(
                    AttributeDefinitions.TagEffect, ConditionFlurryOfBlows) ||
                !rulesetCharacter.IsMonkWeapon(attackMode.SourceDefinition as ItemDefinition))
            {
                return;
            }

            attackMode.AttacksNumber = 2;
            attackMode.AddAttackTagAsNeeded(
                CharacterActionItemFormPatcher.Refresh_Patch.HideAttacksNumberOnActionPanel);
        }
    }

    //
    // Master of the Blade
    //

    private sealed class PhysicalAttackFinishedByMeMasterOfTheBlade(FeatureDefinition featureMasterOfTheBlade)
        : IPhysicalAttackFinishedByMe
    {
        public IEnumerator OnPhysicalAttackFinishedByMe(
            GameLocationBattleManager battleManager,
            CharacterAction action,
            GameLocationCharacter attacker,
            GameLocationCharacter defender,
            RulesetAttackMode attackMode,
            RollOutcome rollOutcome,
            int damageAmount)
        {
            var rulesetAttacker = attacker.RulesetCharacter;

            if (rollOutcome is RollOutcome.Success or RollOutcome.CriticalSuccess ||
                !attacker.OncePerTurnIsValid(featureMasterOfTheBlade.Name) ||
                ValidatorsWeapon.IsUnarmed(attackMode) ||
                !rulesetAttacker.IsMonkWeapon(attackMode.SourceDefinition as ItemDefinition))
            {
                yield break;
            }

            attacker.SetSpecialFeatureUses(featureMasterOfTheBlade.Name, 0);
            rulesetAttacker.LogCharacterUsedFeature(featureMasterOfTheBlade);

            attacker.MyExecuteActionAttack(
                ActionDefinitions.Id.AttackFree,
                defender,
                attackMode,
                action.ActionParams.ActionModifiers[0]);
        }
    }
}
