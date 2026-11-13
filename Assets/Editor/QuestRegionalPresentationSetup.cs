using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class QuestRegionalPresentationSetup
{
    const string Root = "Assets/ElementalQuest/RegionalPresentation/";
    public static void Run()
    {
        if (Application.dataPath.Contains("ValidationProject"))
        {
            PlayerSettings.companyName = "ElementalQuestValidation";
            PlayerSettings.productName = "RegionalPresentationReview";
        }
        string[] regions = { "Cume", "Volcano", "Eclipse" };
        string[] bosses = { "Tempest", "Magma", "Eclipse" };
        string[] refs = { "TempestBoss/Idle/Idle_01.png", "MagmaBoss/idle/Idle_01.png", "EclipseBoss/idle/Idle_01.png" };
        string[] stages = { "CumeDosVentos", "VulcaoCinzento", "CasteloDoEclipse" };
        string[] arenas = { "ArenaCume", "ArenaVulcao", "ArenaEclipse" };
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        for (int r = 0; r < 3; r++)
        {
            var sprites = new Sprite[3];
            string[] names = { "00_Far", "01_Middle", "02_Near" };
            for (int l = 0; l < 3; l++)
            {
                string p = Root + "Parallax/" + regions[r] + "/" + names[l] + ".png";
                Import(p, 100, Vector2.one * .5f);
                sprites[l] = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            }
            var reference = (TextureImporter)AssetImporter.GetAtPath("Assets/ElementalQuest/" + refs[r]);
            var frames = new ObjectReferenceKeyframe[9];
            for (int f = 0; f < 8; f++)
            {
                string p = Root + "Bosses/" + bosses[r] + "/Derrotado_" + f.ToString("00") + ".png";
                Import(p, reference.spritePixelsPerUnit, reference.spritePivot);
                frames[f] = new ObjectReferenceKeyframe { time = f / 8f, value = AssetDatabase.LoadAssetAtPath<Sprite>(p) };
            }
            frames[8] = new ObjectReferenceKeyframe { time = 1.25f, value = frames[7].value };
            string clipPath = Root + "Bosses/" + bosses[r] + "/Derrotado.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
            clip.name = "Derrotado"; clip.frameRate = 8; clip.wrapMode = WrapMode.ClampForever;
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false; settings.stopTime = 1.25f;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            foreach (string name in new[] { stages[r], arenas[r] })
            {
                var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
                var root = Object.FindAnyObjectByType<QuestCampaignScene>().gameObject;
                var parallax = root.GetComponent<QuestLayeredParallax>();
                if (parallax == null) parallax = root.AddComponent<QuestLayeredParallax>();
                parallax.layers = sprites;
                parallax.factors = new[] { .008f, .035f, .085f };
                if (name == arenas[r])
                {
                    var defeated = root.GetComponent<QuestBossDefeatPresentation>();
                    if (defeated == null) defeated = root.AddComponent<QuestBossDefeatPresentation>();
                    defeated.defeat = clip;
                    defeated.frames = new Sprite[8];
                    for (int f = 0; f < 8; f++) defeated.frames[f] = (Sprite)frames[f].value;
                }
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("EQ_REGIONAL_PRESENTATION_SETUP_OK");
        EditorApplication.Exit(0);
    }
    static void Import(string path, float ppu, Vector2 pivot)
    {
        var i = (TextureImporter)AssetImporter.GetAtPath(path);
        i.textureType = TextureImporterType.Sprite; i.spriteImportMode = SpriteImportMode.Single;
        i.spritePixelsPerUnit = ppu; var ts = new TextureImporterSettings(); i.ReadTextureSettings(ts); ts.spriteAlignment = (int)SpriteAlignment.Custom; ts.spritePivot = pivot; ts.spriteMeshType = SpriteMeshType.FullRect; i.SetTextureSettings(ts);
        i.filterMode = FilterMode.Point; i.mipmapEnabled = false; i.npotScale = TextureImporterNPOTScale.None;
        i.textureCompression = TextureImporterCompression.Uncompressed; i.maxTextureSize = 4096;
        i.alphaIsTransparency = true;
        i.SaveAndReimport();
    }
}

