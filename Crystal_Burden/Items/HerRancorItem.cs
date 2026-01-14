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
    class HerRancorItem : Crystal_Burden
    {
        private static String tier = "";
        public static void Init()
        {
            ItemTier Hbtier = ItemTier.Lunar;
            if (!ToggleDebuffs.Value)
                Hbtier = ItemTier.Tier3;
            else
                tier = "Lunar";
            HerRancor = ScriptableObject.CreateInstance<ItemDef>();
            AddTokens();
            HerRancor.name = "HERRANCOR";
            HerRancor.nameToken = "HERRANCOR_NAME";
            HerRancor.pickupToken = "HERRANCOR_PICKUP";
            HerRancor.descriptionToken = "HERRANCOR_DESC";
            HerRancor.loreToken = "HERRANCOR_LORE";
            HerRancor.deprecatedTier = Hbtier;
            if (Nsfw?.Value ?? false)
                HerRancor.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>(Artist.Value + "orangeItemIcon");
            else if (!Nsfw?.Value ?? true)
                HerRancor.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("Brdn_Crystal_Rancor" + tier + "ItemIcon");
            if (Nsfw?.Value ?? false)
                HerRancor.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "orangeher_burden");
            else if (!Nsfw?.Value ?? true)
                HerRancor.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Rancor");
            HerRancor.canRemove = true;
            HerRancor.hidden = false;
            if (!VariantDropCount.Value)
                HerRancor.tags = [ItemTag.WorldUnique, ItemTag.ObliterationRelated, ItemTag.CanBeTemporary];
            else
                HerRancor.tags = [ItemTag.ObliterationRelated, ItemTag.CanBeTemporary];

            var rules = new ItemDisplayRuleDict();
            ItemDisplaysExpanded.HerItemDisplay(rules, "orangeher_burden", "Rancor");
            AddLocation(rules);
            CustomItem CustomItem = new CustomItem(HerRancor, rules);
            ItemAPI.Add(CustomItem);
        }
        private static void AddTokens()
        {
            if (Nsfw?.Value ?? false)
                LanguageAPI.Add("HERRANCOR_NAME", "Her Rancor");
            else if (!Nsfw?.Value ?? true)
                LanguageAPI.Add("HERRANCOR_NAME", "Crystal Rancor");
            if (ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERRANCOR_PICKUP", "Increase damage and decrease armor.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
                LanguageAPI.Add("HERRANCOR_DESC", $"Increase damage by {Hbbv}% and decrease armor by {Hbdbv}%.\nAll item drops are now variants of: <color=#307FFF>" + NameToken + "</color>");
            }
            if (!ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERRANCOR_PICKUP", "Increase damage.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
                LanguageAPI.Add("HERRANCOR_DESC", $"Increase damage by {Hbbv}%.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
            }
            LanguageAPI.Add("HERRANCOR_LORE", "<style=cMono>//--AUTO-TRANSCRIPTION FROM [file unavailable] --//</style>\n\n...then I have something you may find more pleasurable.\n\nHere. Take it in your hand, feel its [girth] upon your palm. Observe its [pointed] texture.\n\nNow bring it within you. Do not worry.\n\nIt is active. Feel its [rancor] within you, how your body strains to [contain] it. Let it give you [strength].\n\nDo not worry. If this does not please you...");

        }
        public static void AddLocation(ItemDisplayRuleDict rules)
        {
            if (!ItemVisibility.Value && (Nsfw?.Value ?? false))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "orangeher_burden");
                followerPrefab.AddComponent<FakeRancorPrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Rancor");
                followerPrefab.AddComponent<FakeRancorPrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Rancor");
                Material what = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[0];
                Material what2 = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[1];
                what2.SetFloat("_Magnitude", 0.075f);
                var materials = followerPrefab.GetComponent<MeshRenderer>().materials;
                materials[2].shader = what.shader;
                materials[2].CopyPropertiesFromMaterial(what);
                materials[3].shader = what2.shader;
                materials[3].CopyPropertiesFromMaterial(what2);
                followerPrefab.GetComponent<MeshRenderer>().materials = materials;
                followerPrefab.AddComponent<FakeRancorPrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                rules.Add("mdlCommandoDualies", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.05f, 0.3f, -0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHuntress", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.05f, 0.2f, 0.025f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlToolbot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.35f, 3f, 1f),
                    localAngles = new Vector3(0f, 0f, 140f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlEngi", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.02f, 0.25f, 0f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMage", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0f, 0.3f, -0.05f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMerc", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.02f, 0.2f, 0.01f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlTreebot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(0.1f, 0.3f, 0.25f),
                    localAngles = new Vector3(0f, 0f, 155f),
                    localScale = generalScale * 2
                },
                });
                rules.Add("mdlLoader", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "MechLowerArmL",
                    localPos = new Vector3(0.05f, 0.4f, -0.05f),
                    localAngles = new Vector3(180f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlCroco", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.75f, 1.5f, 0.5f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlCaptain", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.05f, 0.45f, -0.015f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlBandit2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.025f, 0.2f, 0f),
                    localAngles = new Vector3(-20f, -160f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHeretic", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "ElbowL",
                    localPos = new Vector3(0.15f, -0.05f, -0.02f),
                    localAngles = new Vector3(10f, -175f, 45f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlRailGunner", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.025f, 0.2f, -0.015f),
                    localAngles = new Vector3(-15f, -150f, 135f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlVoidSurvivor", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "ForeArmL",
                    localPos = new Vector3(0.03f, 0.39f, -0.0035f),
                    localAngles = new Vector3(-15f, 80f, 145f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMEL-T2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "ElbowL",
                    localPos = new Vector3(0.0155f, 0.085f, -0.005f),
                    localAngles = new Vector3(0f, 95f, 130f),
                    localScale = generalScale * 0.25f
                },
                });
                rules.Add("mdlPaladin", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.1f, 0.25f, -0.05f),
                    localAngles = new Vector3(350f, 90f, 135f),
                    localScale = generalScale * 2f
                },
                });
                rules.Add("mdlDeputy", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "ForeArmL",
                    localPos = new Vector3(0.03f, 0.25f, 0.03f),
                    localAngles = new Vector3(350f, 15f, 135f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlDriver(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(0.055f, 0.22f, 0.04f),
                    localAngles = new Vector3(0f, 60f, 150f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlHouse(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "LeftForearm",
                    localPos = new Vector3(-0.05f, 0.25f, -0.025f),
                    localAngles = new Vector3(0f, 240f, 150f),
                    localScale = generalScale
                },
                });
            }
        }
    }
}
