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
    class HerPanicItem : Crystal_Burden
    {
        private static String tier = "";
        public static void Init()
        {
            ItemTier Hbtier = ItemTier.Lunar;
            if (!ToggleDebuffs.Value)
                Hbtier = ItemTier.Tier3;
            else
                tier = "Lunar";
            HerPanic = ScriptableObject.CreateInstance<ItemDef>();
            AddTokens();
            HerPanic.name = "HERPANIC";
            HerPanic.nameToken = "HERPANIC_NAME";
            HerPanic.pickupToken = "HERPANIC_PICKUP";
            HerPanic.descriptionToken = "HERPANIC_DESC";
            HerPanic.loreToken = "HERPANIC_LORE";
            HerPanic.deprecatedTier = Hbtier;
            if (Nsfw?.Value ?? false)
                HerPanic.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>(Artist.Value + "violetItemIcon");
            else if (!Nsfw?.Value ?? true)
                HerPanic.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("Brdn_Crystal_Panic" + tier + "ItemIcon");
            if (Nsfw?.Value ?? false)
                HerPanic.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "violether_burden");
            else if (!Nsfw?.Value ?? true)
                HerPanic.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Panic");
            HerPanic.canRemove = true;
            HerPanic.hidden = false;
            if (!VariantDropCount.Value)
                HerPanic.tags = [ItemTag.WorldUnique, ItemTag.ObliterationRelated, ItemTag.CanBeTemporary];
            else
                HerPanic.tags = [ItemTag.ObliterationRelated, ItemTag.CanBeTemporary];

            var rules = new ItemDisplayRuleDict();
            ItemDisplaysExpanded.HerItemDisplay(rules, "violether_burden", "Panic");
            AddLocation(rules);
            CustomItem CustomItem = new CustomItem(HerPanic, rules);
            ItemAPI.Add(CustomItem);
        }
        private static void AddTokens()
        {
            if (Nsfw?.Value ?? false)
                LanguageAPI.Add("HERPANIC_NAME", "Her Panic");
            else if (!Nsfw?.Value ?? true)
                LanguageAPI.Add("HERPANIC_NAME", "Crystal Panic");
            if (ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERPANIC_PICKUP", "Increase move speed and decrease damage.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
                LanguageAPI.Add("HERPANIC_DESC", $"Increase move speed by {Hbbv}% and decrease damage by {Hbdbv}%.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
            }
            if (!ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERPANIC_PICKUP", "Increase move speed.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
                LanguageAPI.Add("HERPANIC_DESC", $"Increase move speed by {Hbbv}%.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
            }
            LanguageAPI.Add("HERPANIC_LORE", "<style=cMono>//--AUTO-TRANSCRIPTION FROM [file unavailable] --//</style>\n\n...then I have something you may find more pleasurable.\n\nHere. Take it in your hand, feel its [energy] upon your palm. Observe its [irregular] texture.\n\nNow bring it within you. Do not worry.\n\nIt is active. Feel its [panic] within you, how your body strains to [focus] it. Let it give you [initiative].\n\nDo not worry. If this does not please you...");

        }
        public static void AddLocation(ItemDisplayRuleDict rules)
        {
            if (!ItemVisibility.Value && (Nsfw?.Value ?? false))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "violether_burden");
                followerPrefab.AddComponent<FakePanicPrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Panic");
                followerPrefab.AddComponent<FakePanicPrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Panic");
                Material what = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[0];
                Material what2 = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[1];
                what2.SetFloat("_Magnitude", 0.075f);
                var materials = followerPrefab.GetComponent<MeshRenderer>().materials;
                materials[2].shader = what.shader;
                materials[2].CopyPropertiesFromMaterial(what);
                materials[3].shader = what2.shader;
                materials[3].CopyPropertiesFromMaterial(what2);
                followerPrefab.GetComponent<MeshRenderer>().materials = materials;
                followerPrefab.AddComponent<FakePanicPrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                rules.Add("mdlCommandoDualies", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.4f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHuntress", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.4f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlToolbot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0f, 3.5f, 1f),
                    localAngles = new Vector3(270f, 0f, 0f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlEngi", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.4f, 0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMage", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.5f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMerc", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.5f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlTreebot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "FootBackR",
                    localPos = new Vector3(0f, 1f, 0f),
                    localAngles = new Vector3(0f, 180f, 180f),
                    localScale = generalScale * 2
                },
                });
                rules.Add("mdlLoader", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.4f, 0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlCroco", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0f, 4f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlCaptain", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.5f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlBandit2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.5f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHeretic", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "KneeR",
                    localPos = new Vector3(-0.85f, -0.0225f, 0f),
                    localAngles = new Vector3(-60f, -50f, -40f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlRailGunner", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.025f, 0.5f, 0f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlVoidSurvivor", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0f, 0.5f, 0f),
                    localAngles = new Vector3(-40f, -170f, 155f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMEL-T2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "KneeR",
                    localPos = new Vector3(-0.015f, 0.04f, -0.02f),
                    localAngles = new Vector3(-90f, 180f, 0f),
                    localScale = generalScale * 0.25f
                },
                });
                rules.Add("mdlPaladin", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0.05f, 0.5f, 0.05f),
                    localAngles = new Vector3(180f, -0.05f, 0f),
                    localScale = generalScale * 2f
                },
                });
                rules.Add("mdlDeputy", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(0f, 0.35f, 0.025f),
                    localAngles = new Vector3(330f, 350f, 180f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlDriver(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfR",
                    localPos = new Vector3(-0.015f, 0.4f, 0.02f),
                    localAngles = new Vector3(330f, 350f, 180f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlHouse(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "RightCalf",
                    localPos = new Vector3(0.025f, 0.35f, -0.05f),
                    localAngles = new Vector3(335f, 150f, 180f),
                    localScale = generalScale
                },
                });
            }
        }
    }
}
