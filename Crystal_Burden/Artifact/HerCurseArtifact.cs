using R2API;
using RoR2;
using UnityEngine;

namespace Crystal_Burden
{
    class HerCurseArtifact : Crystal_Burden
    {
        public static void Init()
        {
            HerCurse = ScriptableObject.CreateInstance<ArtifactDef>();
            LanguageAPI.Add("HERCURSE_DESC", "All item drops will be turned into Burden Variants.");
            HerCurse.descriptionToken = "HERCURSE_DESC";
            if (Nsfw?.Value ?? false)
            {
                LanguageAPI.Add("HERCURSE_NAME", "Artifact of Her Curse");
                HerCurse.nameToken = "HERCURSE_NAME";
                HerCurse.smallIconSelectedSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("HerCurseArtifactBurdenEnabled");
                HerCurse.smallIconDeselectedSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("HerCurseArtifactBurdenDisabled");
            }
            else if (!Nsfw?.Value ?? true)
            {
                LanguageAPI.Add("HERCURSE_NAME", "Artifact of Crystal Curse");
                HerCurse.nameToken = "HERCURSE_NAME";
                HerCurse.smallIconSelectedSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("HerCurseArtifactCrystalEnabled");
                HerCurse.smallIconDeselectedSprite = Crystal_Burden.bundle.LoadAsset<Sprite>("HerCurseArtifactCrystalDisabled");

            }
            ContentAddition.AddArtifactDef(HerCurse);
        }
    }
}
