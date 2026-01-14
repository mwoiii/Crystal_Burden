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
    class HerTorporItem : Crystal_Burden
    {
        private static String tier = "";
        public static void Init()
        {
            ItemTier Hbtier = ItemTier.Lunar;
            if (!ToggleDebuffs.Value)
                Hbtier = ItemTier.Tier3;
            else
                tier = "Lunar";
            HerTorpor = ScriptableObject.CreateInstance<ItemDef>();
            AddTokens();
            HerTorpor.name = "HERTORPOR";
            HerTorpor.nameToken = "HERTORPOR_NAME";
            HerTorpor.pickupToken = "HERTORPOR_PICKUP";
            HerTorpor.descriptionToken = "HERTORPOR_DESC";
            HerTorpor.loreToken = "HERTORPOR_LORE";
            HerTorpor.deprecatedTier = Hbtier;
            if (Nsfw?.Value ?? false)
                HerTorpor.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>(Artist.Value + "royalblueItemIcon");
            else if (!Nsfw?.Value ?? true)
                HerTorpor.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("Brdn_Crystal_Torpor" + tier + "ItemIcon");
            if (Nsfw?.Value ?? false)
                HerTorpor.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "royalblueher_burden");
            else if (!Nsfw?.Value ?? true)
                HerTorpor.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Torpor");
            HerTorpor.canRemove = true;
            HerTorpor.hidden = false;
            if (!VariantDropCount.Value)
                HerTorpor.tags = [ItemTag.WorldUnique, variantTag, ItemTag.CanBeTemporary];
            else
                HerTorpor.tags = [variantTag, ItemTag.CanBeTemporary];

            var rules = new ItemDisplayRuleDict();
            ItemDisplaysExpanded.HerItemDisplay(rules, "royalblueher_burden", "Torpor");
            AddLocation(rules);
            CustomItem CustomItem = new CustomItem(HerTorpor, rules);
            ItemAPI.Add(CustomItem);
        }
        private static void AddTokens()
        {
            if (Nsfw?.Value ?? false)
                LanguageAPI.Add("HERTORPOR_NAME", "Her Torpor");
            else if (!Nsfw?.Value ?? true)
                LanguageAPI.Add("HERTORPOR_NAME", "Crystal Torpor");
            if (ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERTORPOR_PICKUP", "Increase regen and decrease attack speed.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
                LanguageAPI.Add("HERTORPOR_DESC", $"Increase regen by {Hbbv}% and decrease attack speed by {Hbdbv}%.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
            }
            if (!ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERTORPOR_PICKUP", "Increase regen.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
                LanguageAPI.Add("HERTORPOR_DESC", $"Increase regen by {Hbbv}%.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
            }
            LanguageAPI.Add("HERTORPOR_LORE", "<style=cMono>//--AUTO-TRANSCRIPTION FROM [file unavailable] --//</style>\n\n...then I have something you may find more pleasurable.\n\nHere. Take it in your hand, feel its [cold] upon your palm. Observe its [smooth] texture.\n\nNow bring it within you. Do not worry.\n\nIt is active. Feel its [torpor] within you, how your body strains to [revive] it. Let it give you [peace].\n\nDo not worry. If this does not please you...");

        }
        public static void AddLocation(ItemDisplayRuleDict rules)
        {
            if (!ItemVisibility.Value && (Nsfw?.Value ?? false))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "royalblueher_burden");
                followerPrefab.AddComponent<FakeTorporPrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "royalblueher_burden");
                followerPrefab.AddComponent<FakeTorporPrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Torpor");
                Material what = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[0];
                Material what2 = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[1];
                what2.SetFloat("_Magnitude", 0.075f);
                var materials = followerPrefab.GetComponent<MeshRenderer>().materials;
                materials[2].shader = what.shader;
                materials[2].CopyPropertiesFromMaterial(what);
                materials[3].shader = what2.shader;
                materials[3].CopyPropertiesFromMaterial(what2);
                followerPrefab.GetComponent<MeshRenderer>().materials = materials;
                followerPrefab.AddComponent<FakeTorporPrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                rules.Add("mdlCommandoDualies", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.05f, 0.1f, -0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHuntress", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.025f, 0.15f, -0.075f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlToolbot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.05f, 2.75f, -0.75f),
                    localAngles = new Vector3(0f, 130f, 180f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlEngi", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.1f, -0.0875f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMage", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.1f, -0.0375f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMerc", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.005f, 0.125f, -0.075f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlTreebot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "FootFrontL",
                    localPos = new Vector3(0f, 0.2f, -0.2f),
                    localAngles = new Vector3(0f, 180f, 180f),
                    localScale = generalScale * 2
                },
                });
                rules.Add("mdlLoader", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.1f, -0.0875f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlCroco", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.225f, 1f, -0.75f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlCaptain", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.125f, -0.1f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlBandit2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.15f, -0.075f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHeretic", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "KneeL",
                    localPos = new Vector3(0.1f, -0.2f, 0f),
                    localAngles = new Vector3(80f, 20f, 115f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlRailGunner", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.13f, 0.085f, 0.04f),
                    localAngles = new Vector3(15f, 75f, 180f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlVoidSurvivor", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0.075f, 0.085f, 0.015f),
                    localAngles = new Vector3(15f, 75f, 180f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMEL-T2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "KneeL",
                    localPos = new Vector3(-0.0015f, 0.0525f, 0.015f),
                    localAngles = new Vector3(5f, -52.5f, 175.5f),
                    localScale = generalScale * 0.25f
                },
                });
                rules.Add("mdlPaladin", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.015f, 0f, -0.135f),
                    localAngles = new Vector3(35f, 170f, 180f),
                    localScale = generalScale * 2f
                },
                });
                rules.Add("mdlDeputy", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(-0.01f, 0.1f, 0.1f),
                    localAngles = new Vector3(20f, 185f, 180f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlDriver(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CalfL",
                    localPos = new Vector3(0f, 0.1f, 0.085f),
                    localAngles = new Vector3(20f, 185f, 180f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlHouse(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LeftCalf",
                    localPos = new Vector3(0f, 0.06f, -0.125f),
                    localAngles = new Vector3(20f, 185f, 180f),
                    localScale = generalScale
                },
                });
            }
        }
    }
}
