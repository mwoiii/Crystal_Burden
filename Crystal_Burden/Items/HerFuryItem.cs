using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Crystal_Burden
{
    class HerFuryItem : Crystal_Burden
    {
        private static String tier = "";
        public static void Init()
        {
            ItemTier Hbtier = ItemTier.Lunar;
            if (!ToggleDebuffs.Value)
                Hbtier = ItemTier.Tier3;
            else
                tier = "Lunar";
            HerFury = ScriptableObject.CreateInstance<ItemDef>();
            AddTokens();
            HerFury.name = "HERFURY";
            HerFury.nameToken = "HERFURY_NAME";
            HerFury.pickupToken = "HERFURY_PICKUP";
            HerFury.descriptionToken = "HERFURY_DESC";
            HerFury.loreToken = "HERFURY_LORE";
            HerFury.deprecatedTier = Hbtier;
            if (Nsfw?.Value ?? false)
                HerFury.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>(Artist.Value + "reallyredItemIcon");
            else if (!Nsfw?.Value ?? true)
                HerFury.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("Brdn_Crystal_Fury" + tier + "ItemIcon");
            if (Nsfw?.Value ?? false)
                HerFury.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "reallyredher_burden");
            else if (!Nsfw?.Value ?? true)
                HerFury.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Fury");
            HerFury.canRemove = true;
            HerFury.hidden = false;
            if (!VariantDropCount.Value)
                HerFury.tags = [ItemTag.WorldUnique, ItemTag.ObliterationRelated, ItemTag.CanBeTemporary];
            else
                HerFury.tags = [ItemTag.ObliterationRelated, ItemTag.CanBeTemporary];

            var rules = new ItemDisplayRuleDict();
            ItemDisplaysExpanded.HerItemDisplay(rules, "reallyredher_burden", "Fury");
            AddLocation(rules);
            CustomItem CustomItem = new CustomItem(HerFury, rules);
            ItemAPI.Add(CustomItem);
        }
        private static void AddTokens()
        {
            if (Nsfw?.Value ?? false)
                LanguageAPI.Add("HERFURY_NAME", "Her Fury");
            else if (!Nsfw?.Value ?? true)
                LanguageAPI.Add("HERFURY_NAME", "Crystal Fury");
            if (ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERFURY_PICKUP", "Increase attack speed and decrease HP.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
                LanguageAPI.Add("HERFURY_DESC", $"Increase attack speed by {Hbbv}% and decrease HP by {Hbdbv}%.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
            }
            if (!ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERFURY_PICKUP", "Increase attack speed.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
                LanguageAPI.Add("HERFURY_DESC", $"Increase attack speed by {Hbbv}%.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
            }
            LanguageAPI.Add("HERFURY_LORE", "<style=cMono>//--AUTO-TRANSCRIPTION FROM [file unavailable] --//</style>\n\n...then I have something you may find more pleasurable.\n\nHere. Take it in your hand, feel its [heat] upon your palm. Observe its [rough] texture.\n\nNow bring it within you. Do not worry.\n\nIt is active. Feel its [fury] within you, how your body strains to [sustain] it. Let it give you [passion].\n\nDo not worry. If this does not please you...");

        }
        public static void AddLocation(ItemDisplayRuleDict rules)
        {
            if (!ItemVisibility.Value && (Nsfw?.Value ?? false))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "reallyredher_burden");
                followerPrefab.AddComponent<FakeFuryPrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                _ = new ItemDisplayRule[]
                {
                new ItemDisplayRule
                {
                     ruleType = ItemDisplayRuleType.ParentedPrefab,
                     followerPrefab = followerPrefab,
                     childName = "Pelvis",
                     localPos = new Vector3(0f, 0.1f, 0.1f),
                     localAngles = new Vector3(180f, -0.05f, 0f),
                     localScale = generalScale
                }
                };
            }
            if (!ItemVisibility.Value && (!Nsfw?.Value ?? true))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Fury");
                followerPrefab.AddComponent<FakeFuryPrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                _ = new ItemDisplayRule[]
                {
                new ItemDisplayRule
                {
                     ruleType = ItemDisplayRuleType.ParentedPrefab,
                     followerPrefab = followerPrefab,
                     childName = "Pelvis",
                     localPos = new Vector3(0f, 0.1f, 0.1f),
                     localAngles = new Vector3(180f, -0.05f, 0f),
                     localScale = generalScale
                }
                };
            }
            if (ItemVisibility.Value && (!Nsfw?.Value ?? true))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Fury");
                Material what = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[0];
                Material what2 = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[1];
                what2.SetFloat("_Magnitude", 0.075f);
                var materials = followerPrefab.GetComponent<MeshRenderer>().materials;
                materials[2].shader = what.shader;
                materials[2].CopyPropertiesFromMaterial(what);
                materials[3].shader = what2.shader;
                materials[3].CopyPropertiesFromMaterial(what2);
                followerPrefab.GetComponent<MeshRenderer>().materials = materials;
                followerPrefab.AddComponent<FakeFuryPrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                rules.Add("mdlCommandoDualies", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.2f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHuntress", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.15f, -0.1f, 0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlToolbot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Hip",
                    localPos = new Vector3(0.9f, 0.75f, 1.15f),
                    localAngles = new Vector3(0f, 90f, 180f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlEngi", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.25f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMage", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.15f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMerc", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.2f, 0.05f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlTreebot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.65f, 0.25f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 2
                },
                });
                rules.Add("mdlLoader", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.15f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlCroco", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(1.75f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlCaptain", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.2f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlBandit2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.2f, -0.1f, -0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHeretic", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.45f, 0f, -0.35f),
                    localAngles = new Vector3(-90f, 90f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlRailGunner", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.14f, 0.42f, 0.015f),
                    localAngles = new Vector3(-40f, -130f, 130f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlVoidSurvivor", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.09f, 0.025f, 0.12f),
                    localAngles = new Vector3(0f, 70f, 175f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMEL-T2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Waist",
                    localPos = new Vector3(0.02f, 0.032f, 0.043f),
                    localAngles = new Vector3(0f, 90f, 180f),
                    localScale = generalScale * 0.25f
                },
                });
                rules.Add("mdlPaladin", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.325f, 0.15f, 0.04f),
                    localAngles = new Vector3(0f, 20f, 0f),
                    localScale = generalScale * 2f
                },
                });
                rules.Add("mdlDeputy", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.125f, -0.05f, -0.05f),
                    localAngles = new Vector3(0f, -160f, 180f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlDriver(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.2f, -0.025f, 0.05f),
                    localAngles = new Vector3(0f, 20f, 180f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlHouse(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.175f, 0f, 0f),
                    localAngles = new Vector3(0f, 180f, 190f),
                    localScale = generalScale
                },
                });
            }
        }
    }
}
