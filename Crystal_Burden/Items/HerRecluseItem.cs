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
    class HerRecluseItem : Crystal_Burden
    {
        private static String tier = "";
        public static void Init()
        {
            ItemTier Hbtier = ItemTier.Lunar;
            if (!ToggleDebuffs.Value)
                Hbtier = ItemTier.Tier3;
            else
                tier = "Lunar";
            HerRecluse = ScriptableObject.CreateInstance<ItemDef>();
            AddTokens();
            HerRecluse.name = "HERRECLUSE";
            HerRecluse.nameToken = "HERRECLUSE_NAME";
            HerRecluse.pickupToken = "HERRECLUSE_PICKUP";
            HerRecluse.descriptionToken = "HERRECLUSE_DESC";
            HerRecluse.loreToken = "HERRECLUSE_LORE";
            HerRecluse.deprecatedTier = Hbtier;
            if (Nsfw?.Value ?? false)
                HerRecluse.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>(Artist.Value + "lightishblueItemIcon");
            else if (!Nsfw?.Value ?? true)
                HerRecluse.pickupIconSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("Brdn_Crystal_Recluse" + tier + "ItemIcon");
            if (Nsfw?.Value ?? false)
                HerRecluse.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "lightishblueher_burden");
            else if (!Nsfw?.Value ?? true)
                HerRecluse.pickupModelPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Recluse");
            HerRecluse.canRemove = true;
            HerRecluse.hidden = false;
            if (!VariantDropCount.Value)
                HerRecluse.tags = [ItemTag.WorldUnique, variantTag, ItemTag.CanBeTemporary];
            else
                HerRecluse.tags = [variantTag, ItemTag.CanBeTemporary];

            var rules = new ItemDisplayRuleDict();
            ItemDisplaysExpanded.HerItemDisplay(rules, "lightishblueher_burden", "Recluse");
            AddLocation(rules);
            CustomItem CustomItem = new CustomItem(HerRecluse, rules);
            ItemAPI.Add(CustomItem);
        }
        private static void AddTokens()
        {
            if (Nsfw?.Value ?? false)
                LanguageAPI.Add("HERRECLUSE_NAME", "Her Recluse");
            else if (!Nsfw?.Value ?? true)
                LanguageAPI.Add("HERRECLUSE_NAME", "Crystal Recluse");
            if (ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERRECLUSE_PICKUP", "Increase armor and decrease regen.\nAll item drops are now: <color=#307FFF>" + NameToken + "</color>");
                LanguageAPI.Add("HERRECLUSE_DESC", $"Increase armor by {Hbbv}% and decrease regen by {Hbdbv}%.\nAll item drops are now: <color=#307FFF>" + NameToken + "</color>");
            }
            if (!ToggleDebuffs.Value)
            {
                LanguageAPI.Add("HERRECLUSE_PICKUP", "Increase armor.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
                LanguageAPI.Add("HERRECLUSE_DESC", $"Increase armor by {Hbbv}%.\nMonsters now have a chance to drop variants of: <color=#e7553b>" + NameToken + "</color>");
            }
            LanguageAPI.Add("HERRECLUSE_LORE", "<style=cMono>//--AUTO-TRANSCRIPTION FROM [file unavailable] --//</style>\n\n...then I have something you may find more pleasurable.\n\nHere. Take it in your hand, feel its [softness] upon your palm. Observe its [blocky] texture.\n\nNow bring it within you. Do not worry.\n\nIt is active. Feel its [recluse] within you, how your body strains to [sustain] it. Let it give you [safety].\n\nDo not worry. If this does not please you...");

        }

        public static void AddLocation(ItemDisplayRuleDict rules)
        {
            if (!ItemVisibility.Value && (Nsfw?.Value ?? false))
            {
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>(Artist.Value + "lightishblueher_burden");
                followerPrefab.AddComponent<FakeReclusePrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Recluse");
                followerPrefab.AddComponent<FakeReclusePrefabSizeScript>();
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
                GameObject followerPrefab = Crystal_Burden.bundle.LoadAsset<GameObject>("Brdn_Crystal_Recluse");
                Material what = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[0];
                Material what2 = LegacyResourcesAPI.Load<GameObject>("prefabs/networkedobjects/LockedMage").transform.Find("ModelBase/IceMesh").GetComponent<MeshRenderer>().materials[1];
                what2.SetFloat("_Magnitude", 0.075f);
                var materials = followerPrefab.GetComponent<MeshRenderer>().materials;
                materials[2].shader = what.shader;
                materials[2].CopyPropertiesFromMaterial(what);
                materials[3].shader = what2.shader;
                materials[3].CopyPropertiesFromMaterial(what2);
                followerPrefab.GetComponent<MeshRenderer>().materials = materials;
                followerPrefab.AddComponent<FakeReclusePrefabSizeScript>();
                Vector3 generalScale = new Vector3(.0125f, .0125f, .0125f);
                rules.Add("mdlCommandoDualies", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.25f, 0.35f, -0.05f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHuntress", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.2f, 0.3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlToolbot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(2.5f, 2.5f, -1f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlEngi", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.2f, 0.2f, 0.1f),
                    localAngles = new Vector3(-45f, -90f, -90f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMage", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.15f, 0.3f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMerc", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.225f, 0.3f, -0.025f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlTreebot", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "FlowerBase",
                    localPos = new Vector3(0.8f, 0.75f, -0.4f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 2
                },
                });
                rules.Add("mdlLoader", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.25f, 0.3f, -0.05f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlCroco", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-3f, 2f, 2f),
                    localAngles = new Vector3(-30f, -160f, 10f),
                    localScale = generalScale * 10
                },
                });
                rules.Add("mdlCaptain", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.25f, 0.45f, -0.05f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlBandit2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.25f, 0.35f, -0.05f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlHeretic", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.6f, -0.6f, -0.6f),
                    localAngles = new Vector3(-90f, 90f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlRailGunner", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.24f, 0.12f, -0.035f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlVoidSurvivor", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.22f, 0.225f, -0.25f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
                rules.Add("mdlMEL-T2", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Body",
                    localPos = new Vector3(0.055f, 0.06f, -0.035f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 0.25f
                },
                });
                rules.Add("mdlPaladin", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.3f, 0.45f, -0.175f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 2f
                },
                });
                rules.Add("mdlDeputy", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.175f, 0.225f, 0.02f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlDriver(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.3f, 0.385f, -0.05f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale * 0.8f
                },
                });
                rules.Add("mdlHouse(Clone)", new ItemDisplayRule[] { new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = followerPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.25f, 0.15f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = generalScale
                },
                });
            }
        }
    }
}
