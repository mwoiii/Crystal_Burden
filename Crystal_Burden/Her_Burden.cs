using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.UIElements;
using static RoR2.Console;
using Object = System.Object;
using Random = System.Random;
using UnityObject = UnityEngine.Object;
using UnityRandom = UnityEngine.Random;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Crystal_Burden
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.content_management", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.items", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.bepis.r2api.language", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.OkIgotIt.Her_Burden", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Maiesen.BodyBlend", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("com.Kalkune.Crystal_Burden", "Crystal_Burden", "1.5.12")]

    public class Crystal_Burden : BaseUnityPlugin
    {
        public static AssetBundle bundle;
        public static ItemDef HerBurden;
        public static ItemDef HerRecluse;
        public static ItemDef HerFury;
        public static ItemDef HerTorpor;
        public static ItemDef HerRancor;
        public static ItemDef HerPanic;
        public static ItemDef HBItemPicker;
        public static EquipmentDef HerGamble;
        public static BuffDef HerGambleBuff;
        public static BuffDef HerGambleDeBuff;
        public static ArtifactDef HerCurse;
        public static ItemDef VariantOnSurvivor;
        public static List<PickupIndex> TransformedList = new List<PickupIndex>();
        public static PickupIndex CurrentTransformedItem = PickupIndex.none;
        public static ItemTag variantTag;
        public static float Hbbv;
        public static float Hbdbv;
        public static bool HerBurdenInstalled = false;
        public static bool BodyBlend = false;
        public static string NameToken = "";
        public static ConfigEntry<bool> ItemVisibility { get; set; }
        public static ConfigEntry<bool> LuckEffect { get; set; }
        public static ConfigEntry<bool> GiveOriginalItem { get; set; }
        public static ConfigEntry<bool> ToggleDebuffs { get; set; }
        public static ConfigEntry<bool> VariantDropCount { get; set; }
        public static ConfigEntry<bool> VariantsAffectSize { get; set; }
        public static ConfigEntry<bool> Nsfw { get; set; }
        public static ConfigEntry<bool> LuckInverseEffect { get; set; }
        public static ConfigEntry<bool> ParticleTrail { get; set; }
        public static ConfigEntry<bool> TogglePearlCleanse { get; set; }
        public static ConfigEntry<string> Artist { get; set; }
        public static ConfigEntry<string> VariantShownOnSurvivor { get; set; }
        public static ConfigEntry<float> MaxSize { get; set; }
        public static ConfigEntry<float> SizeMultiplier { get; set; }
        public static ConfigEntry<float> DecreaseItemDropPercentage { get; set; }
        public static ConfigEntry<float> MinimumDropChance { get; set; }
        public static ConfigEntry<int> ChanceChangePickup { get; set; }
        public static ConfigEntry<int> ChanceEnemyDrop { get; set; }
        public static ConfigEntry<int> LimitDecreaseItemDropPercentage { get; set; }
        public static ConfigEntry<float> Hbbuff { get; set; }
        public static ConfigEntry<float> Hbdebuff { get; set; }
        public static ConfigEntry<string> Hbversion { get; set; }

        public void Awake()
        {
            Log.Init(Logger);
            SoftDependencies.Init();
            variantTag = R2API.ItemAPI.AddItemTag("BurdenVariant");
            ItemVisibility = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Item Visibility", true, "Changes if Burden Variants shows on the Survivor");
            LuckEffect = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Luck Effect", true, "Changes if luck effects chance to pickup Burden Variants once you have a Variant");
            GiveOriginalItem = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Give Original Item", false, "Changes if you receive the original item along with a Burden Variant (Lunar Tier Exclusive)");
            ToggleDebuffs = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Debuffs", true, "Changes if debuffs are applied or not. If false, Burden Variants will change to legendaries and makes them have a chance to drop on kill");
            VariantDropCount = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Variant Drop Count", true, "Changes if all Burden Variants are in the drop list separately or together. If false, Variants will only have one entry into the drop list");
            LuckInverseEffect = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Luck Inverse Effect", true, "Changes if luck is inverted for the \"Toggle Luck Effect\". If true, luck will be inverted");
            TogglePearlCleanse = Config.Bind<bool>("1. Her Burden Toggle", "Toggle Cleanse into Pearls", false, "Changes if Variants gets cleansed into Pearls rather than the original item.");
            if (HerBurdenInstalled || File.ReadAllText(Paths.ConfigPath + "\\com.Kalkune.Crystal_Burden.cfg").Contains("[0. Her Burden NSFW Toggle]"))
            {
                Nsfw = Config.Bind<bool>("0. Her Burden NSFW Toggle", "Toggles Her Burden NSFW", true, "Changes if Her Burden is NSFW or SFW. If false, Her Burden will switch to SFW models");
                ParticleTrail = Config.Bind<bool>("2. Her Burden Toggle", "Toggle Particle Trail", false, "Changes if the particle trail is visible or not visible");
                Artist = Config.Bind<string>("2. Her Burden General", "Artist", "Hush", new ConfigDescription("Decides which artist to use", new AcceptableValueList<string>("Hush", "aka6")));
                VariantsAffectSize = Config.Bind<bool>("2. Her Burden Toggle", "Toggle Variants Affect Size", false, "Changes if other Burden Variants increase item display size");
                VariantShownOnSurvivor = Config.Bind<string>("2. Her Burden General", "Variant Size Increase", "Burden", new ConfigDescription("Changes which Variant gets its size increased", new AcceptableValueList<string>("Burden", "Recluse", "Fury", "Torpor", "Rancor", "Panic")));
            }
            MaxSize = Config.Bind<float>("1. Her Burden Size", "Max size of Variant", 2, "Changes the max size of all Variants on the Survivor");
            SizeMultiplier = Config.Bind<float>("1. Her Burden Size", "Size Multiplier for Variant", 20, "Changes how many of each Variant are required to get to the max size");
            DecreaseItemDropPercentage = Config.Bind<float>("1. Her Burden Mechanics", "Decrease Item Drop Percentage", .8f, "The rate of the drop chance percentage decreases towards \"Minimum Drop Chance\" (Lunar Tier Exclusive)");
            MinimumDropChance = Config.Bind<float>("1. Her Burden Mechanics", "Minimum Drop Chance", 10, "Minimum Variant Drop chance, doesn't override \"Chance to change pickup to Her Burden Variants\" if \"Minimum Drop Chance\" is a higher value than \"Chance to change pickup to Her Burden Variants\" (Lunar Tier Exclusive)");
            ChanceChangePickup = Config.Bind<int>("1. Her Burden Mechanics", "Chance to change pickup to Her Burden Variants", 100, "Chance to change other items to Burden Variants on pickup once you have a Variant (Lunar Tier Exclusive)");
            ChanceEnemyDrop = Config.Bind<int>("1. Her Burden Mechanics", "Chance for enemies to drop Her Burden Variants", 5, "Chance for enemies to drop Burden Variants once you have a Variant (Legendary Tier Exclusive)");
            LimitDecreaseItemDropPercentage = Config.Bind<int>("1. Her Burden Mechanics", "Item Limit Amount to Decrease Item Drop Percentage", 20, "Variants needed to start decreasing drop chance. \"Decrease Item Drop Percentage\" and \"Minimum Drop Chance\" are part of this config (Lunar Tier Exclusive)");
            Hbbuff = Config.Bind<float>("1. Her Burden Mechanics", "Buff", 1.05f, "Changes the increase of buff of the item exponentially per item");
            Hbdebuff = Config.Bind<float>("1. Her Burden Mechanics", "Debuff", 0.975f, "Changes the decrease of debuff of the item exponentially per item");
            Hbversion = Config.Bind<string>("Dev. Development Config", "Version", "1.5.2", "I don't recommend changing this value");
            Hbbv = (float)Math.Round((Hbbuff.Value - 1f) * 100f, 2);
            Hbdbv = (float)Math.Round(Math.Abs((Hbdebuff.Value - 1f) * 100f), 2);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Crystal_Burden.Resources.herburden"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
            }

            ConfigChanges.Init();
            MiscItems.Init();
            HerBurdenItem.Init();
            HerRecluseItem.Init();
            HerFuryItem.Init();
            HerTorporItem.Init();
            HerRancorItem.Init();
            HerPanicItem.Init();
            HerGambleEquipment.Init();
            HerCurseArtifact.Init();

            //ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;

            //maxHealth-moveSpeed   Her Burden
            //armor-regen           Her Recluse
            //attackSpeed-maxHealth Her Fury
            //regen-attackSpeed     Her Torpor
            //damage-armor          Her Rancor
            //moveSpeed-damage      Her Panic

            IL.RoR2.ItemCatalog.SetItemDefs += (il) =>
            {
                var c = new ILCursor(il);

                if (c.TryGotoNext(x => x.MatchLdcI4(14), x => x.MatchStloc(6)))
                {
                    c.Index += 2;
                    c.Emit(OpCodes.Ldc_I4, 20);
                    c.Emit(OpCodes.Stloc, 6);
                }
            };

            RoR2Application.onLoad += () =>
            {
                var heretic = Resources.Load<GameObject>("prefabs/characterbodies/hereticbody");
                if (heretic)
                {
                    var Parent = heretic.GetComponentInChildren<ChildLocator>();
                    if (Parent)
                    {
                        var thighLChild = Parent.FindChild("ThighL");
                        var thighRChild = Parent.FindChild("ThighR");
                        var headChild = Parent.FindChild("Chest");

                        HG.ArrayUtils.ArrayAppend(ref Parent.transformPairs, new ChildLocator.NameTransformPair
                        {
                            name = "KneeL",
                            transform = thighLChild.Find("HipPart1_L/HipPart2_L/Knee_L")
                        });
                        HG.ArrayUtils.ArrayAppend(ref Parent.transformPairs, new ChildLocator.NameTransformPair
                        {
                            name = "KneeR",
                            transform = thighRChild.Find("HipPart1_R/HipPart2_R/Knee_R")
                        });
                        HG.ArrayUtils.ArrayAppend(ref Parent.transformPairs, new ChildLocator.NameTransformPair
                        {
                            name = "ElbowL",
                            transform = headChild.Find("Scapula_L/Shoulder_L/ShoulderPart1_L/ShoulderPart2_L/Elbow_L")
                        });
                    }
                }
            };
            
            WhoKnows();
            Hook CharacterBodyUpdate = new Hook(typeof(CharacterBody).GetMethod("Update", BindingFlags.Public | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("CharacterBody_Update"));
            Hook CharacterMasterOnBodyStart = new Hook(typeof(CharacterMaster).GetMethod("OnBodyStart", BindingFlags.Public | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("CharacterMaster_OnBodyStart"));
            Hook DuplicatingOnEnter = new Hook(typeof(EntityStates.Duplicator.Duplicating).GetMethod("OnEnter", BindingFlags.Public | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("Duplicating_OnEnter"));
            Hook EquipmentSlotPerformEquipmentAction = new Hook(typeof(EquipmentSlot).GetMethod("PerformEquipmentAction", BindingFlags.NonPublic | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("EquipmentSlot_PerformEquipmentAction"));
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            Hook PickupDropletControllerCreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 = new Hook(typeof(PickupDropletController).GetMethod("CreatePickupDroplet", new Type[] { typeof(GenericPickupController.CreatePickupInfo), typeof(Vector3), typeof(Vector3) }), typeof(Crystal_Burden).GetMethod("PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3"));
            Hook CharacterMasterRespawnExtraLife = new Hook(typeof(CharacterMaster).GetMethod("RespawnExtraLife", BindingFlags.Public | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("CharacterMaster_RespawnExtraLife"));
            Hook PickupPickerControllerSetOptionsFromPickupForCommandArtifact = new Hook((MethodBase)typeof(PickupPickerController).GetMethod("SetOptionsFromPickupForCommandArtifact", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(UniquePickup) }, null), PickupPickerController_SetOptionsFromPickupForCommandArtifact);
            Hook InventoryGiveItem_ItemIndex_int = new Hook(typeof(Inventory).GetMethod("GiveItem", new Type[] { typeof(ItemIndex), typeof(int) }), typeof(Crystal_Burden).GetMethod("Inventory_GiveItem_ItemIndex_int"));
            Hook InventoryRemoveItem_ItemIndex_int = new Hook(typeof(Inventory).GetMethod("RemoveItem", new Type[] { typeof(ItemIndex), typeof(int) }), typeof(Crystal_Burden).GetMethod("Inventory_RemoveItem_ItemIndex_int"));
            Hook ItemDefAttemptGrant = new Hook(typeof(ItemDef).GetMethod("AttemptGrant"), typeof(Crystal_Burden).GetMethod("ItemDef_AttemptGrant"));
            ILHook IL_LunarItemOrEquipmentCostTypeHelper_PayCost_TakeOne = new ILHook(typeof(CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper).GetMethod("<PayCost>g__TakeOne|3_0", BindingFlags.NonPublic | BindingFlags.Static), LunarItemOrEquipmentCostTypeHelper_PayCost_TakeOne);
            Hook PurchaseInteraction_GetInteractability = new Hook(typeof(PurchaseInteraction).GetMethod("GetInteractability", BindingFlags.Public | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("PurchaseInteraction_GetInteractability"));
            Hook On_ShopTerminalBehavior_DropPickup = new Hook(typeof(ShopTerminalBehavior).GetMethod("DropPickup", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(bool) }, null), ShopTerminalBehavior_DropPickup);
            Hook CharacterBody_RecalculateStats = new Hook(typeof(CharacterBody).GetMethod("RecalculateStats", BindingFlags.Public | BindingFlags.Instance), typeof(Crystal_Burden).GetMethod("CharacterBody_RecalculateStats"));
        }

        public static void CharacterBody_RecalculateStats(Action<CharacterBody> orig, CharacterBody self)
        {
            if (!self?.inventory)
            {
                orig(self);
                return;
            }
            self.acceleration += (self.inventory.GetItemCountEffective(HerPanic)*4);
            orig(self);
        }

        public static void ShopTerminalBehavior_DropPickup(Action<ShopTerminalBehavior, bool> orig, ShopTerminalBehavior self, bool isDuplicated)
        {
            if (CurrentTransformedItem != PickupIndex.none)
            {
                self.pickup.pickupIndex = CurrentTransformedItem;
                CurrentTransformedItem = PickupIndex.none;
            }
            orig(self, isDuplicated);
        }

        public static Interactability PurchaseInteraction_GetInteractability(Func<PurchaseInteraction, Interactor, Interactability> orig, PurchaseInteraction self, Interactor activator)
        {
            if (self.displayNameToken.ToLower() == "shrine_cleanse_name" && CurrentTransformedItem != PickupIndex.none && !TogglePearlCleanse.Value)
                return Interactability.Disabled;
            return orig(self, activator);
        }

        public static void LunarItemOrEquipmentCostTypeHelper_PayCost_TakeOne(ILContext il) 
        { 
            var altBehaviourDelegate = new Action<Inventory, ItemIndex, CostTypeDef.PayCostResults>((inventory, itemIndex, result) => {
                if (TransformedList.Count != 0 && ItemCatalog.GetItemDef(itemIndex).ContainsTag(variantTag) && !TogglePearlCleanse.Value) {
                    int ItemsToRemove;
                    CurrentTransformedItem = TransformedList[UnityRandom.Range(0, TransformedList.Count)];
                    ItemTier TransformedTier = PickupCatalog.GetPickupDef(CurrentTransformedItem).itemTier;
                    TransformedList.Remove(CurrentTransformedItem);
                    if (TransformedTier == ItemTier.Tier1 || TransformedTier == ItemTier.VoidTier1)
                        ItemsToRemove = 1;
                    else if (TransformedTier == ItemTier.Tier2 || TransformedTier == ItemTier.VoidTier2)
                        ItemsToRemove = 2;
                    else if (TransformedTier == ItemTier.Tier3 || TransformedTier == ItemTier.VoidTier3)
                        ItemsToRemove = 5;
                    else if (TransformedTier == ItemTier.Lunar)
                        ItemsToRemove = 1;
                    else if (TransformedTier == ItemTier.Boss || TransformedTier == ItemTier.VoidBoss)
                        ItemsToRemove = 1;
                    else
                        ItemsToRemove = 1;
                    for (int i = 0; i < ItemsToRemove; i++) {
                        List<ItemIndex> items = new List<ItemIndex>();
                        if (inventory.GetItemCountPermanent(HerBurden.itemIndex) > 0)
                            items.Add(HerBurden.itemIndex);
                        if (inventory.GetItemCountPermanent(HerRecluse.itemIndex) > 0)
                            items.Add(HerRecluse.itemIndex);
                        if (inventory.GetItemCountPermanent(HerFury.itemIndex) > 0)
                            items.Add(HerFury.itemIndex);
                        if (inventory.GetItemCountPermanent(HerTorpor.itemIndex) > 0)
                            items.Add(HerTorpor.itemIndex);
                        if (inventory.GetItemCountPermanent(HerRancor.itemIndex) > 0)
                            items.Add(HerRancor.itemIndex);
                        if (inventory.GetItemCountPermanent(HerPanic.itemIndex) > 0)
                            items.Add(HerPanic.itemIndex);
                        ItemIndex itemRemoved = items[UnityRandom.Range(0, items.Count)];
                        Inventory.ItemTransformation itemTransformation = new Inventory.ItemTransformation {
                            allowWhenDisabled = true,
                            forbidTempItems = true,
                            minToTransform = 1,
                            maxToTransform = 1,
                            originalItemIndex = itemIndex,
                            newItemIndex = ItemIndex.None
                        };
                        if (itemTransformation.TryTransform(inventory, out var result2)) {
                            result.AddTakenItemsFromTransformation(in result2);
                        }
                        break;
                    }
                }
                else {
                    Inventory.ItemTransformation itemTransformation = new Inventory.ItemTransformation {
                        allowWhenDisabled = true,
                        forbidTempItems = true,
                        minToTransform = 1,
                        maxToTransform = 1,
                        originalItemIndex = itemIndex,
                        newItemIndex = ItemIndex.None
                    };
                    if (itemTransformation.TryTransform(inventory, out var result2)) {
                        result.AddTakenItemsFromTransformation(in result2);
                    }
                }
            });

            ILCursor c = new ILCursor(il);
            ILLabel skipOGLabel = c.DefineLabel();
            var displayClass = typeof(RoR2.CostTypeCatalog.LunarItemOrEquipmentCostTypeHelper).GetNestedType("<>c__DisplayClass3_0", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var inventoryField = displayClass.GetField("inventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var resultField = displayClass.GetField("result", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (c.TryGotoNext(x => x.MatchLdloc(2)) &&
                c.TryGotoNext(x => x.MatchLdloc(3)) &&
                c.TryGotoNext(MoveType.After, x => x.MatchBge(out _))) {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldfld, inventoryField);
                c.Emit(OpCodes.Ldloc_S, c.Body.Variables[5]);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldfld, resultField);
                c.EmitDelegate(altBehaviourDelegate);
                c.Emit(OpCodes.Br, skipOGLabel);
                if (c.TryGotoNext(x => x.MatchCallOrCallvirt<CostTypeDef.PayCostResults>("AddTakenItemsFromTransformation")) &&
                    c.TryGotoNext(MoveType.Before, x => x.MatchRet())) {
                    c.MarkLabel(skipOGLabel);
                }
                else {
                    Log.Error("Part 2 of PayCost ILHook failed");
                }
            }
            else {
                Log.Error("Part 1 of PayCost ILHook failed");
            }
        }

        public static void ItemDef_AttemptGrant(On.RoR2.ItemDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
        {
            CharacterBody body = context.body;
            Inventory inventory = body.inventory;
            GenericPickupController self = context.controller;
            context.shouldNotify = false;
            PickupIndex originalItem = self.pickup.pickupIndex;

            if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
            {
                orig(ref context);

                //Handle the size change with scripts
                Size(1, body, false);
                Size(2, body, false);
                return;
            }

            if (ToggleDebuffs.Value)
            {
                int TotalDrops;
                float DropChance = ChanceChangePickup.Value;
                int VariantCount = inventory.GetItemCountPermanent(HerBurden.itemIndex) + inventory.GetItemCountPermanent(HerRecluse.itemIndex) + inventory.GetItemCountPermanent(HerFury.itemIndex) + inventory.GetItemCountPermanent(HerTorpor.itemIndex) + inventory.GetItemCountPermanent(HerRancor.itemIndex) + inventory.GetItemCountPermanent(HerPanic.itemIndex);

                float TempMaxOverflow = VariantCount - LimitDecreaseItemDropPercentage.Value;
                if (TempMaxOverflow < 0)
                    TempMaxOverflow = 0;

                if (DropChance > MinimumDropChance.Value && VariantCount > LimitDecreaseItemDropPercentage.Value)
                {
                    DropChance *= Mathf.Pow(DecreaseItemDropPercentage.Value, TempMaxOverflow);
                    if (DropChance < MinimumDropChance.Value)
                        DropChance = MinimumDropChance.Value;
                }

                CharacterMaster cmluck = new CharacterMaster();
                cmluck.luck = inventory.GetComponent<CharacterMaster>().luck;
                if (LuckEffect.Value)
                {
                    if (LuckInverseEffect.Value)
                        cmluck.luck = -cmluck.luck;
                    DropChance += cmluck.luck;
                }
                for (TotalDrops = 0; DropChance > 100; TotalDrops++)
                    DropChance -= 100;

                bool changepickup = false;
                if (inventory.GetItemCountPermanent(HerBurden.itemIndex) > 0 || inventory.GetItemCountPermanent(HerRecluse.itemIndex) > 0 || inventory.GetItemCountPermanent(HerFury.itemIndex) > 0 || inventory.GetItemCountPermanent(HerTorpor.itemIndex) > 0 || inventory.GetItemCountPermanent(HerRancor.itemIndex) > 0 || inventory.GetItemCountPermanent(HerPanic.itemIndex) > 0)
                    changepickup = true;
                bool blacklist = false;
                if (self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerBurden.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerRecluse.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerFury.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerTorpor.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerRancor.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerPanic.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapWhite.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapGreen.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapRed.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ScrapYellow.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ArtifactKey.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.LunarTrinket.itemIndex))
                    blacklist = true;
                if (!blacklist && GiveOriginalItem.Value)
                    if (self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.Pearl.itemIndex) || self.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ShinyPearl.itemIndex))
                        blacklist = true;

                if (Util.CheckRoll(DropChance))
                    TotalDrops++;
                if (changepickup == true && TotalDrops > 0 && blacklist == false)
                {
                    ItemTier itemTier = self.pickup.pickupIndex.pickupDef.itemTier;
                    if (itemTier == ItemTier.Tier2 || itemTier == ItemTier.VoidTier2 || itemTier == ItemTier.Boss || itemTier == ItemTier.VoidBoss)
                        TotalDrops += 1;
                    else if (itemTier == ItemTier.Tier3 || itemTier == ItemTier.VoidTier3)
                        TotalDrops += 4;
                    if (GiveOriginalItem.Value)
                    {
                        orig(ref context);
                        GenericPickupController.SendPickupMessage(body.master, self.pickup);
                    }
                    for (int x = TotalDrops; x > 0; x--)
                    {
                        switch (Mathf.FloorToInt(UnityRandom.Range(0, 6)))
                        {
                            case 0:
                                self.pickup = new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerBurden.itemIndex), decayValue = self.pickup.decayValue };
                                break;
                            case 1:
                                self.pickup = new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRecluse.itemIndex), decayValue = self.pickup.decayValue };
                                break;
                            case 2:
                                self.pickup = new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerFury.itemIndex), decayValue = self.pickup.decayValue };
                                break;
                            case 3:
                                self.pickup = new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerTorpor.itemIndex), decayValue = self.pickup.decayValue };
                                break;
                            case 4:
                                self.pickup = new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRancor.itemIndex), decayValue = self.pickup.decayValue };
                                break;
                            case 5:
                                self.pickup = new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerPanic.itemIndex), decayValue = self.pickup.decayValue };
                                break;
                        }
                        orig(ref context);
                        //GenericPickupController.SendPickupMessage(body.master, self.pickupIndex);
                        CharacterMasterNotificationQueue.PushItemTransformNotification(body.master, PickupCatalog.GetPickupDef(originalItem).itemIndex, self.pickup.pickupIndex.pickupDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Suppressed);
                    }
                    if (!TogglePearlCleanse.Value)
                        TransformedList.Add(originalItem);

                    //Handle the size change with scripts
                    Size(1, body, false);
                    Size(2, body, false);
                    context.shouldNotify = false;
                    return;
                }
            }

            orig(ref context);

            //Handle the size change with scripts
            Size(1, body, false);
            Size(2, body, false);
        }

        public static void Inventory_RemoveItem_ItemIndex_int(Action<Inventory, ItemIndex, int> orig, Inventory self, ItemIndex itemIndex, int count)
        {
            CharacterMaster master = self.gameObject.GetComponent<CharacterMaster>();
            if (!master)
                return;
            CharacterBody body = master.GetBody();

            bool hasOtherVariants = false;
            bool hasVariantOnSurvivor = false;
            List<ItemIndex> items = new List<ItemIndex>();

            if (self.GetItemCountPermanent(VariantOnSurvivor.itemIndex) > 0)
                hasVariantOnSurvivor = true;

            if (self.GetItemCountPermanent(HerBurden.itemIndex) > 0 && HerBurden.itemIndex != VariantOnSurvivor.itemIndex)
                items.Add(HerBurden.itemIndex);
            if (self.GetItemCountPermanent(HerRecluse.itemIndex) > 0 && HerRecluse.itemIndex != VariantOnSurvivor.itemIndex)
                items.Add(HerRecluse.itemIndex);
            if (self.GetItemCountPermanent(HerFury.itemIndex) > 0 && HerFury.itemIndex != VariantOnSurvivor.itemIndex)
                items.Add(HerFury.itemIndex);
            if (self.GetItemCountPermanent(HerTorpor.itemIndex) > 0 && HerTorpor.itemIndex != VariantOnSurvivor.itemIndex)
                items.Add(HerTorpor.itemIndex);
            if (self.GetItemCountPermanent(HerRancor.itemIndex) > 0 && HerRancor.itemIndex != VariantOnSurvivor.itemIndex)
                items.Add(HerRancor.itemIndex);
            if (self.GetItemCountPermanent(HerPanic.itemIndex) > 0 && HerPanic.itemIndex != VariantOnSurvivor.itemIndex)
                items.Add(HerPanic.itemIndex);
            if (items.Count > 0)
                hasOtherVariants = true;

            orig(self, itemIndex, count);

            if (hasVariantOnSurvivor && self.GetItemCountPermanent(VariantOnSurvivor.itemIndex) == 0)
                Size(3, body, true);

            for (int i = items.Count - 1; i >= 0; i--)
                if (self.GetItemCountPermanent(items[i]) == 0)
                    items.Remove(items[i]);

            if (hasOtherVariants && items.Count == 0)
                Size(4, body, true);
        }

        public static void Inventory_GiveItem_ItemIndex_int(Action<Inventory, ItemIndex, int> orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);

            CharacterMaster master = self.gameObject.GetComponent<CharacterMaster>();
            if (!master)
                return;
            CharacterBody body = master.GetBody();
            //Handle the size change with scripts
            Size(1, body, false);
            Size(2, body, false);
        }

        public static void UpdateBodyBlend(CharacterBody self)
        {
            if (!Nsfw?.Value ?? true)
                return;
            if (self?.modelLocator?.modelTransform?.gameObject == null)
                return;
            if (!self.inventory)
                return;
            float VariantCount = self.inventory.GetItemCountEffective(HerBurden.itemIndex);
            if (VariantsAffectSize?.Value ?? false)
                VariantCount += self.inventory.GetItemCountEffective(HerRecluse.itemIndex) + self.inventory.GetItemCountEffective(HerFury.itemIndex) + self.inventory.GetItemCountEffective(HerTorpor.itemIndex) + self.inventory.GetItemCountEffective(HerRancor.itemIndex) + self.inventory.GetItemCountEffective(HerPanic.itemIndex);
            if (VariantCount > SizeMultiplier.Value)
                VariantCount = SizeMultiplier.Value;
            if (SizeMultiplier.Value > VariantCount && VariantCount != 0)
            {
                var progress = 1f - ((SizeMultiplier.Value - VariantCount) / SizeMultiplier.Value);
                self.SetBlendValue("Breasts", progress, "Burden");
            }
            else if (VariantCount == SizeMultiplier.Value)
                self.SetBlendValue("Breasts", 1f, "Burden");
            else
                self.SetBlendValue("Breasts", 0f, "Burden");
        }

        public static void PickupPickerController_SetOptionsFromPickupForCommandArtifact(Action<PickupPickerController, UniquePickup> orig, PickupPickerController self, UniquePickup uniquePickup)
        {
            orig(self, uniquePickup);
            if (uniquePickup.pickupIndex == PickupCatalog.FindPickupIndex(HBItemPicker.itemIndex))
            {
                PickupPickerController.Option[] array;
                List<PickupIndex> list = new List<PickupIndex>
                {
                    PickupCatalog.FindPickupIndex(HerBurden.itemIndex),
                    PickupCatalog.FindPickupIndex(HerRecluse.itemIndex),
                    PickupCatalog.FindPickupIndex(HerFury.itemIndex),
                    PickupCatalog.FindPickupIndex(HerTorpor.itemIndex),
                    PickupCatalog.FindPickupIndex(HerRancor.itemIndex),
                    PickupCatalog.FindPickupIndex(HerPanic.itemIndex)
                };
                _ = new PickupIndex();
                array = new PickupPickerController.Option[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    PickupIndex pickupIndex2 = list[i];
                    array[i] = new PickupPickerController.Option
                    {
                        available = true,
                        pickup = new UniquePickup() { pickupIndex = pickupIndex2, decayValue = uniquePickup.decayValue },
                    };
                }
                self.SetOptionsServer(array);
            }
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Command) && RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
            {
                if (PickupCatalog.GetPickupDef(uniquePickup.pickupIndex).equipmentIndex != EquipmentIndex.None)
                {
                    PickupPickerController.Option[] array;
                    List<PickupIndex> list = new List<PickupIndex>
                    {
                        PickupCatalog.FindPickupIndex(HerGamble.equipmentIndex)
                    };
                    _ = new PickupIndex();
                    array = new PickupPickerController.Option[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        PickupIndex pickupIndex2 = list[i];
                        array[i] = new PickupPickerController.Option
                        {
                            available = true,
                            pickup = new UniquePickup() { pickupIndex = pickupIndex2 },
                        };
                    }
                    self.SetOptionsServer(array);
                }
                else
                {
                    PickupPickerController.Option[] array;
                    List<PickupIndex> list = new List<PickupIndex>
                    {
                        PickupCatalog.FindPickupIndex(HerBurden.itemIndex),
                        PickupCatalog.FindPickupIndex(HerRecluse.itemIndex),
                        PickupCatalog.FindPickupIndex(HerFury.itemIndex),
                        PickupCatalog.FindPickupIndex(HerTorpor.itemIndex),
                        PickupCatalog.FindPickupIndex(HerRancor.itemIndex),
                        PickupCatalog.FindPickupIndex(HerPanic.itemIndex)
                    };
                    _ = new PickupIndex();
                    array = new PickupPickerController.Option[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        PickupIndex pickupIndex2 = list[i];
                        array[i] = new PickupPickerController.Option
                        {
                            available = true,
                            pickup = new UniquePickup() { pickupIndex = pickupIndex2, decayValue = uniquePickup.decayValue },
                        };
                    }
                    self.SetOptionsServer(array);
                }
            }
        }

        public void Start()
        {
            if (!Nsfw?.Value ?? true)
            {
                VariantOnSurvivor = HerBurden;
                return;
            }
            if (VariantShownOnSurvivor.Value == "Burden")
                VariantOnSurvivor = HerBurden;
            else if (VariantShownOnSurvivor.Value == "Recluse")
                VariantOnSurvivor = HerRecluse;
            else if (VariantShownOnSurvivor.Value == "Fury")
                VariantOnSurvivor = HerFury;
            else if (VariantShownOnSurvivor.Value == "Torpor")
                VariantOnSurvivor = HerTorpor;
            else if (VariantShownOnSurvivor.Value == "Rancor")
                VariantOnSurvivor = HerRancor;
            else if (VariantShownOnSurvivor.Value == "Panic")
                VariantOnSurvivor = HerPanic;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new Content());
        }

        public static void CharacterMaster_RespawnExtraLife(Action<CharacterMaster> orig, CharacterMaster self)
        {
            CharacterBody body = self.GetBody();
            Size(3, body, true);
            Size(4, body, true);
            orig(self);

            if (self.playerCharacterMasterController)
            {
                Size(5, body, false);
                Size(6, body, false);
            }
        }

        public static void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3(Action<GenericPickupController.CreatePickupInfo, Vector3, Vector3> orig, GenericPickupController.CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse) && PickupCatalog.GetPickupDef(pickupInfo.pickup.pickupIndex).equipmentIndex != EquipmentIndex.None && pickupInfo.pickup.pickupIndex != PickupCatalog.FindPickupIndex(HerGamble.equipmentIndex))
            {
                PickupDropletController.CreatePickupDroplet(new UniquePickup(PickupCatalog.FindPickupIndex(HerGamble.equipmentIndex)), position, velocity, false);
                return;
            }
            bool burdenvariant = false;
            if (PickupCatalog.GetPickupDef(pickupInfo.pickup.pickupIndex).itemIndex != ItemIndex.None && ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickupInfo.pickup.pickupIndex).itemIndex).ContainsTag(variantTag))
                burdenvariant = true;
            bool blacklist = false;
            if (pickupInfo.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.ArtifactKey.itemIndex) || pickupInfo.pickup.pickupIndex == PickupCatalog.FindPickupIndex(RoR2Content.Items.LunarTrinket.itemIndex))
                blacklist = true;
            if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse) && PickupCatalog.GetPickupDef(pickupInfo.pickup.pickupIndex).itemIndex != ItemIndex.None && !burdenvariant && VariantDropCount.Value && !blacklist)
            {
                switch (Mathf.FloorToInt(UnityRandom.Range(0, 6)))
                {
                    case 0:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerBurden.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 1:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRecluse.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 2:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerFury.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 3:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerTorpor.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 4:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRancor.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 5:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerPanic.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                }
                return;
            }
            if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse) && PickupCatalog.GetPickupDef(pickupInfo.pickup.pickupIndex).itemIndex != ItemIndex.None && !burdenvariant && !VariantDropCount.Value && !blacklist)
            {
                PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerBurden.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                return;
            }
            if (pickupInfo.pickup.pickupIndex == PickupCatalog.FindPickupIndex(HerBurden.itemIndex) && !VariantDropCount.Value)
            {
                switch (Mathf.FloorToInt(UnityRandom.Range(0, 6)))
                {
                    case 0:
                        orig(pickupInfo, position, velocity);
                        break;
                    case 1:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRecluse.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 2:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerFury.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 3:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerTorpor.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 4:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRancor.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                    case 5:
                        PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerPanic.itemIndex), decayValue = pickupInfo.pickup.decayValue }, position, velocity, pickupInfo.duplicated, pickupInfo.recycled);
                        break;
                }
                return;
            }
            orig(pickupInfo, position, velocity);
        }

        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (ToggleDebuffs.Value)
                return;
            if (!report.attacker || !report.attackerBody)
                return;
            if (!report.victimMaster)
                return;
            if (report.victimMaster.minionOwnership.ownerMaster)
                return;
            if (report.attackerBody.inventory.GetItemCountEffective(HerBurden.itemIndex) == 0 && report.attackerBody.inventory.GetItemCountEffective(HerRecluse.itemIndex) == 0 && report.attackerBody.inventory.GetItemCountEffective(HerFury.itemIndex) == 0 && report.attackerBody.inventory.GetItemCountEffective(HerTorpor.itemIndex) == 0 && report.attackerBody.inventory.GetItemCountEffective(HerRancor.itemIndex) == 0 && report.attackerBody.inventory.GetItemCountEffective(HerPanic.itemIndex) == 0)
                return;

            int TotalDrops;
            float DropChance = ChanceEnemyDrop.Value;
            CharacterMaster cmluck = new CharacterMaster();
            cmluck.luck = report.attackerMaster.luck;
            if (LuckEffect.Value)
            {
                if (LuckInverseEffect.Value)
                    cmluck.luck = -cmluck.luck;
                DropChance += cmluck.luck;
            }
            for (TotalDrops = 0; DropChance > 100; TotalDrops++)
                DropChance -= 100;

            if (Util.CheckRoll(DropChance))
                TotalDrops++;

            float decayValue = 1f;
            int permanentVariants = report.attackerBody.inventory.GetItemCountPermanent(HerBurden.itemIndex) + report.attackerBody.inventory.GetItemCountPermanent(HerRecluse.itemIndex) + report.attackerBody.inventory.GetItemCountPermanent(HerFury.itemIndex) + report.attackerBody.inventory.GetItemCountPermanent(HerTorpor.itemIndex) + report.attackerBody.inventory.GetItemCountPermanent(HerRancor.itemIndex) + report.attackerBody.inventory.GetItemCountPermanent(HerPanic.itemIndex);
            if (permanentVariants > 0) {
                decayValue = 0f;
            }

            if (TotalDrops > 0 && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Command))
            {
                for (int x = TotalDrops; x > 0; x--)
                    PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HBItemPicker.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                return;
            }
            if (TotalDrops > 0 && !VariantDropCount.Value)
                for (int x = TotalDrops; x > 0; x--)
                    PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerBurden.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
            if (TotalDrops > 0 && VariantDropCount.Value)
            {
                for (int x = TotalDrops; x > 0; x--)
                    switch (Mathf.FloorToInt(UnityRandom.Range(0, 6)))
                    {
                        case 0:
                            PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerBurden.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                            break;
                        case 1:
                            PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRecluse.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                            break;
                        case 2:
                            PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerFury.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                            break;
                        case 3:
                            PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerTorpor.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                            break;
                        case 4:
                            PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerRancor.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                            break;
                        case 5:
                            PickupDropletController.CreatePickupDroplet(new UniquePickup() { pickupIndex = PickupCatalog.FindPickupIndex(HerPanic.itemIndex), decayValue = decayValue }, report.victimBody.corePosition, Vector3.up * 20f, false, false);
                            break;
                    }
            }
        }

        public static bool EquipmentSlot_PerformEquipmentAction(Func<EquipmentSlot, EquipmentDef, bool> orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (self && self.characterBody)
            {
                CharacterBody body = self.characterBody;
                if (body && equipmentDef == HerGamble && !body.HasBuff(HerGambleBuff) && !body.HasBuff(HerGambleDeBuff))
                {
                    if (Util.CheckRoll(80, body.master))
                        body.AddTimedBuff(HerGambleBuff.buffIndex, 8f);
                    else
                        body.AddTimedBuff(HerGambleDeBuff.buffIndex, 8f);
                    return true;
                }
            }
            return orig(self, equipmentDef);
        }

        public static void Duplicating_OnEnter(Action<EntityStates.Duplicator.Duplicating> orig, EntityStates.Duplicator.Duplicating self)
        {
            if (!ToggleDebuffs.Value)
            {
                orig(self);
                return;
            }
            var lastActivator = self.GetComponent<PurchaseInteraction>().lastActivator;
            if (!lastActivator)
                return;
            var body = lastActivator.GetComponent<CharacterBody>();
            int itemcount = 0;
            List<ItemIndex> items = new List<ItemIndex>();
            if (body.inventory.GetItemCountPermanent(HerBurden.itemIndex) > 0)
                items.Add(HerBurden.itemIndex);
            if (body.inventory.GetItemCountPermanent(HerRecluse.itemIndex) > 0)
                items.Add(HerRecluse.itemIndex);
            if (body.inventory.GetItemCountPermanent(HerFury.itemIndex) > 0)
                items.Add(HerFury.itemIndex);
            if (body.inventory.GetItemCountPermanent(HerTorpor.itemIndex) > 0)
                items.Add(HerTorpor.itemIndex);
            if (body.inventory.GetItemCountPermanent(HerRancor.itemIndex) > 0)
                items.Add(HerRancor.itemIndex);
            if (body.inventory.GetItemCountPermanent(HerPanic.itemIndex) > 0)
                items.Add(HerPanic.itemIndex);
            for (int i = 0; i < items.Count; i++)
                itemcount += body.inventory.GetItemCountPermanent(items[i]);
            if (itemcount > 1 && GiveOriginalItem.Value)
                body.inventory.RemoveItemPermanent(items[Mathf.FloorToInt(UnityRandom.Range(0, items.Count))], 1);

            orig(self);
        }

        public static void CharacterMaster_OnBodyStart(Action<CharacterMaster, CharacterBody> orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);
            if (self.playerCharacterMasterController)
            {
                Size(1, body, false);
                Size(2, body, false);
            }
        }

        //This hook just updates the stack count
        public static void CharacterBody_Update(Action<CharacterBody> orig, CharacterBody self)
        {
            orig(self);
            if ((VariantsAffectSize?.Value ?? false) && (Nsfw?.Value ?? false))
                Size(9, self, true);
            else if ((!VariantsAffectSize?.Value ?? true) || (!Nsfw?.Value ?? true))
                Size(7, self, true);
            Size(8, self, true);
            if (BodyBlend)
                UpdateBodyBlend(self);
        }
        public static void Size(int operation, CharacterBody body, bool truefalse)
        {
            if (!ItemVisibility.Value)
                return;
            if (!body || !body.inventory)
                return;
            if (!body.master)
                return;
            ItemDisplayRuleSet IDRS = body.master.bodyPrefab.GetComponentInChildren<CharacterModel>()?.itemDisplayRuleSet;
            if (!IDRS)
                return;
            if (body.baseNameToken == "BROTHER_BODY_NAME")
                return;

            if (operation % 2 == 1)
            {
                if (truefalse)
                    if (!body.gameObject.GetComponent<BodySizeScript>())
                        return;
                if (!truefalse)
                    if (body.gameObject.GetComponent<BodySizeScript>())
                        return;
            }
            if (operation % 2 == 0)
            {
                if (truefalse)
                    if (!body.gameObject.GetComponent<FakeBodySizeScript>())
                        return;
                if (!truefalse)
                    if (body.gameObject.GetComponent<FakeBodySizeScript>())
                        return;
            }

            int burdenCount = 0;
            if (VariantOnSurvivor != HerBurden && (!Nsfw?.Value ?? true) && !IDRS.GetItemDisplayRuleGroup(HerBurden.itemIndex).isEmpty)
                burdenCount = body.inventory.GetItemCountEffective(HerBurden.itemIndex);
            else if (VariantOnSurvivor != HerBurden && (Nsfw?.Value ?? false) && body.inventory.GetItemCountEffective(HerBurden.itemIndex) > 0 && !IDRS.GetItemDisplayRuleGroup(HerBurden.itemIndex).isEmpty)
                burdenCount = 1;
            int recluseCount = 0;
            if (VariantOnSurvivor != HerRecluse && (!Nsfw?.Value ?? true) && !IDRS.GetItemDisplayRuleGroup(HerRecluse.itemIndex).isEmpty)
                recluseCount = body.inventory.GetItemCountEffective(HerRecluse.itemIndex);
            else if (VariantOnSurvivor != HerRecluse && (Nsfw?.Value ?? false) && body.inventory.GetItemCountEffective(HerRecluse.itemIndex) > 0 && !IDRS.GetItemDisplayRuleGroup(HerRecluse.itemIndex).isEmpty)
                recluseCount = 1;
            int furyCount = 0;
            if (VariantOnSurvivor != HerFury && (!Nsfw?.Value ?? true) && !IDRS.GetItemDisplayRuleGroup(HerFury.itemIndex).isEmpty)
                furyCount = body.inventory.GetItemCountEffective(HerFury.itemIndex);
            else if (VariantOnSurvivor != HerFury && (Nsfw?.Value ?? false) && body.inventory.GetItemCountEffective(HerFury.itemIndex) > 0 && !IDRS.GetItemDisplayRuleGroup(HerFury.itemIndex).isEmpty)
                furyCount = 1;
            int torporCount = 0;
            if (VariantOnSurvivor != HerTorpor && (!Nsfw?.Value ?? true) && !IDRS.GetItemDisplayRuleGroup(HerTorpor.itemIndex).isEmpty)
                torporCount = body.inventory.GetItemCountEffective(HerTorpor.itemIndex);
            else if (VariantOnSurvivor != HerTorpor && (Nsfw?.Value ?? false) && body.inventory.GetItemCountEffective(HerTorpor.itemIndex) > 0 && !IDRS.GetItemDisplayRuleGroup(HerTorpor.itemIndex).isEmpty)
                torporCount = 1;
            int rancorCount = 0;
            if (VariantOnSurvivor != HerRancor && (!Nsfw?.Value ?? true) && !IDRS.GetItemDisplayRuleGroup(HerRancor.itemIndex).isEmpty)
                rancorCount = body.inventory.GetItemCountEffective(HerRancor.itemIndex);
            else if (VariantOnSurvivor != HerRancor && (Nsfw?.Value ?? false) && body.inventory.GetItemCountEffective(HerRancor.itemIndex) > 0 && !IDRS.GetItemDisplayRuleGroup(HerRancor.itemIndex).isEmpty)
                rancorCount = 1;
            int panicCount = 0;
            if (VariantOnSurvivor != HerPanic && (!Nsfw?.Value ?? true) && !IDRS.GetItemDisplayRuleGroup(HerPanic.itemIndex).isEmpty)
                panicCount = body.inventory.GetItemCountEffective(HerPanic.itemIndex);
            else if (VariantOnSurvivor != HerPanic && (Nsfw?.Value ?? false) && body.inventory.GetItemCountEffective(HerPanic.itemIndex) > 0 && !IDRS.GetItemDisplayRuleGroup(HerPanic.itemIndex).isEmpty)
                panicCount = 1;
            int total = burdenCount + recluseCount + furyCount + torporCount + rancorCount + panicCount;

            if (operation % 2 == 1)
                if (body.inventory.GetItemCountEffective(VariantOnSurvivor) == 0 || IDRS.GetItemDisplayRuleGroup(VariantOnSurvivor.itemIndex).isEmpty)
                    return;
            if (operation % 2 == 0)
                if (burdenCount == 0 && recluseCount == 0 && furyCount == 0 && torporCount == 0 && rancorCount == 0 && panicCount == 0)
                    return;

            switch (operation)
            {
                case 1:
                    body.gameObject.AddComponent<BodySizeScript>();
                    body.gameObject.GetComponent<BodySizeScript>().SetBodyMultiplier(body.baseNameToken);
                    break;
                case 2:
                    body.gameObject.AddComponent<FakeBodySizeScript>();
                    body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                    break;
                case 3:
                    DestroyImmediate(body.gameObject.GetComponent<BodySizeScript>());
                    break;
                case 4:
                    DestroyImmediate(body.gameObject.GetComponent<FakeBodySizeScript>());
                    break;
                case 5:
                    body.gameObject.AddComponent<BodySizeScript>();
                    body.gameObject.GetComponent<BodySizeScript>().SetBodyMultiplier(body.baseNameToken);
                    body.gameObject.GetComponent<BodySizeScript>().UpdateStacks(body.inventory.GetItemCountEffective(VariantOnSurvivor));
                    break;
                case 6:
                    if (burdenCount > 0)
                    {
                        body.gameObject.AddComponent<FakeBodySizeScript>();
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("burden", burdenCount, body);
                    }
                    if (recluseCount > 0)
                    {
                        body.gameObject.AddComponent<FakeBodySizeScript>();
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("recluse", recluseCount, body);
                    }
                    if (furyCount > 0)
                    {
                        body.gameObject.AddComponent<FakeBodySizeScript>();
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("fury", furyCount, body);
                    }
                    if (torporCount > 0)
                    {
                        body.gameObject.AddComponent<FakeBodySizeScript>();
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("torpor", torporCount, body);
                    }
                    if (rancorCount > 0)
                    {
                        body.gameObject.AddComponent<FakeBodySizeScript>();
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("rancor", rancorCount, body);
                    }
                    if (panicCount > 0)
                    {
                        body.gameObject.AddComponent<FakeBodySizeScript>();
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("panic", panicCount, body);
                    }
                    break;
                case 7:
                    body.gameObject.GetComponent<BodySizeScript>().SetBodyMultiplier(body.baseNameToken);
                    body.gameObject.GetComponent<BodySizeScript>().UpdateStacks(body.inventory.GetItemCountEffective(VariantOnSurvivor));
                    break;
                case 8:
                    if (burdenCount > 0)
                    {
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("burden", burdenCount, body);
                    }
                    if (recluseCount > 0)
                    {
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("recluse", recluseCount, body);
                    }
                    if (furyCount > 0)
                    {
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("fury", furyCount, body);
                    }
                    if (torporCount > 0)
                    {
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("torpor", torporCount, body);
                    }
                    if (rancorCount > 0)
                    {
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("rancor", rancorCount, body);
                    }
                    if (panicCount > 0)
                    {
                        body.gameObject.GetComponent<FakeBodySizeScript>().SetBodyMultiplier(body.baseNameToken, body);
                        body.gameObject.GetComponent<FakeBodySizeScript>().UpdateStacks("panic", panicCount, body);
                    }
                    break;
                case 9:
                    int temp = body.inventory.GetItemCountEffective(VariantOnSurvivor);
                    total += temp;
                    body.gameObject.GetComponent<BodySizeScript>().SetBodyMultiplier(body.baseNameToken);
                    body.gameObject.GetComponent<BodySizeScript>().UpdateStacks(total);
                    break;
            }
        }
        public void WhoKnows()
        {
            IL.RoR2.CharacterBody.RecalculateStats += (il) =>
            {
                ILCursor c = new ILCursor(il);
                ILLabel dioend = il.DefineLabel(); //end label

                try
                {
                    c.Index = 0;

                    // find the section that the code will be injected					
                    c.GotoNext(
                        MoveType.Before,
                        x => x.MatchLdarg(0), // 1388	0D5A	ldarg.0
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs).GetField("Weak")), // 1389	0D5B	ldc.i4.s	0x21
                        x => x.MatchCallOrCallvirt<CharacterBody>("HasBuff") // 1390	0D5D	call	instance bool RoR2.CharacterBody::HasBuff(valuetype RoR2.BuffIndex)
                    );

                    if (c.Index != 0)
                    {
                        c.Index++;

                        // this block is just "If artifact isn't enabled, jump to dioend label". In an item case, this should be the part where you check if you have the items or not.
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, bool>>((cb) =>
                        {
                            if (cb.master && cb.master.inventory)
                            {
                                int items = cb.master.inventory.GetItemCountEffective(HerBurden.itemIndex);
                                int items2 = cb.master.inventory.GetItemCountEffective(HerRecluse.itemIndex);
                                int items3 = cb.master.inventory.GetItemCountEffective(HerFury.itemIndex);
                                int items4 = cb.master.inventory.GetItemCountEffective(HerTorpor.itemIndex);
                                int items5 = cb.master.inventory.GetItemCountEffective(HerRancor.itemIndex);
                                int items6 = cb.master.inventory.GetItemCountEffective(HerPanic.itemIndex);
                                if (items > 0 || items2 > 0 || items3 > 0 || items4 > 0 || items5 > 0 || items6 > 0) return true;
                                return false;
                            }
                            return false;
                        });
                        c.Emit(OpCodes.Brfalse, dioend);

                        // this.maxHealth
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_maxHealth")); // 1414 0DA3	call	instance float32 RoR2.CharacterBody::get_maxHealth()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.maxHealth *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float healthmultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                if (cb.master.inventory.GetItemCountEffective(RoR2Content.Items.ShieldOnly) > 0)
                                    return healthmultiplier;
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerBurden.itemIndex);
                                if (itemCount > 0)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        healthmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount);
                                    else
                                        healthmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerFury.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        healthmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        healthmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return healthmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_maxHealth", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.maxShield
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_maxShield")); // 1414 0DA3	call	instance float32 RoR2.CharacterBody::get_maxHealth()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.maxShield *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float shieldmultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                if (cb.master.inventory.GetItemCountEffective(RoR2Content.Items.ShieldOnly) == 0)
                                    return shieldmultiplier;
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerBurden.itemIndex);
                                if (itemCount > 0)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        shieldmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount);
                                    else
                                        shieldmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerFury.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        shieldmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        shieldmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return shieldmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_maxShield", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.attackSpeed
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_attackSpeed")); // 1426 0DC7	call	instance float32 RoR2.CharacterBody::get_attackSpeed()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.attackSpeed *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float attackSpeedmultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerFury.itemIndex);
                                if (itemCount > 0)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        attackSpeedmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount);
                                    else
                                        attackSpeedmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerTorpor.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        attackSpeedmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        attackSpeedmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return attackSpeedmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_attackSpeed", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.moveSpeed
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_moveSpeed")); // 1406	0D8A	call	instance float32 RoR2.CharacterBody::get_moveSpeed()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.moveSpeed *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float moveSpeedmultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerPanic.itemIndex);
                                if (itemCount > 0)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        moveSpeedmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount);
                                    else
                                        moveSpeedmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerBurden.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        moveSpeedmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        moveSpeedmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return moveSpeedmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_moveSpeed", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.armor2
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_armor")); // 1438 0DE8	call	instance float32 RoR2.CharacterBody::get_armor()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.armor *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float armormultiplier = 0;
                            if (cb.master && cb.master.inventory)
                            {
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerRecluse.itemIndex);
                                if (itemCount > 0)
                                {
                                    armormultiplier += 5;
                                }
                            }
                            return armormultiplier;
                        });
                        c.Emit(OpCodes.Add);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_armor", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.armor
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_armor")); // 1438 0DE8	call	instance float32 RoR2.CharacterBody::get_armor()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.armor *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float armormultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerRecluse.itemIndex);
                                if (itemCount > 1)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        armormultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount - 1);
                                    else
                                        armormultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount - 1);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerRancor.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        armormultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        armormultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return armormultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_armor", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.damage
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_damage")); // 1444 0DFD	call	instance float32 RoR2.CharacterBody::get_damage()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.damage *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float damagemultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerRancor.itemIndex);
                                if (itemCount > 0)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        damagemultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount);
                                    else
                                        damagemultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerPanic.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        damagemultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        damagemultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return damagemultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_damage", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.regen2
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_regen")); // 1450 0E0F	call	instance float32 RoR2.CharacterBody::get_regen()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.regen *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float regenmultiplier = 0;
                            if (cb.master && cb.master.inventory)
                            {
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerTorpor.itemIndex);
                                if (itemCount > 0)
                                {
                                    regenmultiplier += Math.Min(5, itemCount);

                                }
                            }
                            return regenmultiplier;
                        });
                        c.Emit(OpCodes.Add);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_regen", BindingFlags.Instance | BindingFlags.NonPublic));

                        // this.regen
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("get_regen")); // 1450 0E0F	call	instance float32 RoR2.CharacterBody::get_regen()

                        // get the inventory count for the item, calculate multiplier, return a float value
                        // This is essentially `this.regen *= multiplier;`
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<CharacterBody, float>>((cb) =>
                        {
                            float regenmultiplier = 1;
                            if (cb.master && cb.master.inventory)
                            {
                                int localmultiplier = 1;
                                if (RunArtifactManager.instance.IsArtifactEnabled(HerCurse))
                                    localmultiplier++;
                                int itemCount = cb.master.inventory.GetItemCountEffective(HerTorpor.itemIndex);
                                if (itemCount > 0)
                                {
                                    if (cb.GetBuffCount(HerGambleBuff.buffIndex) > 0)
                                        regenmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier * 2, itemCount);
                                    else
                                        regenmultiplier *= Mathf.Pow(1 + (Hbbuff.Value - 1) * localmultiplier, itemCount);
                                }
                                int itemCount2 = cb.master.inventory.GetItemCountEffective(HerRecluse.itemIndex);
                                if (itemCount2 > 0 && ToggleDebuffs.Value)
                                {
                                    if (cb.GetBuffCount(HerGambleDeBuff.buffIndex) > 0)
                                        regenmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier * 2, itemCount2);
                                    else
                                        regenmultiplier *= Mathf.Pow(1 - (1 - Hbdebuff.Value) * localmultiplier, itemCount2);
                                }
                            }
                            return regenmultiplier;
                        });
                        c.Emit(OpCodes.Mul);
                        c.Emit(OpCodes.Callvirt, typeof(CharacterBody).GetMethod("set_regen", BindingFlags.Instance | BindingFlags.NonPublic));

                        c.MarkLabel(dioend); // end label
                    }

                }
                catch (Exception ex) { base.Logger.LogError(ex); }
            };
        }
    }
    public class Content : IContentPackProvider
    {
        public ContentPack contentPack = new ContentPack();

        public string identifier
        {
            get { return "Content"; }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            EquipmentDef[] equipmentDefs = new EquipmentDef[] { Crystal_Burden.HerGamble };
            ArtifactDef[] artifactDefs = new ArtifactDef[] { Crystal_Burden.HerCurse };
            ItemDef[] itemDefs = new ItemDef[] { Crystal_Burden.HerBurden, Crystal_Burden.HerRecluse, Crystal_Burden.HerFury, Crystal_Burden.HerTorpor, Crystal_Burden.HerRancor, Crystal_Burden.HerPanic, Crystal_Burden.HBItemPicker };
            BuffDef[] buffDefs = new BuffDef[] { Crystal_Burden.HerGambleBuff, Crystal_Burden.HerGambleDeBuff };
            contentPack.identifier = identifier;
            contentPack.equipmentDefs.Add(equipmentDefs);
            contentPack.artifactDefs.Add(artifactDefs);
            contentPack.itemDefs.Add(itemDefs);
            contentPack.buffDefs.Add(buffDefs);
            args.ReportProgress(1f);
            yield break;
        }
        
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}