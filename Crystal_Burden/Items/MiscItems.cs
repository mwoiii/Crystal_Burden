using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Crystal_Burden
{
    class MiscItems : Crystal_Burden
    {
        public static void Init()
        {
            HBItemPicker = ScriptableObject.CreateInstance<ItemDef>();
            HBItemPicker.name = "HBITEMPICKER";
            HBItemPicker.AutoPopulateTokens();
            HBItemPicker.deprecatedTier = ItemTier.NoTier;
            HBItemPicker.hidden = true;
            HBItemPicker.tags = [ItemTag.CanBeTemporary];
            var rules = new ItemDisplayRuleDict();
            CustomItem CustomItem = new CustomItem(HBItemPicker, rules);
            ItemAPI.Add(CustomItem);
        }
    }
}
