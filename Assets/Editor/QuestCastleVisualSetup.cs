using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class QuestCastleVisualSetup
{
    public static void Run()
    {
        string folder = "Assets/ElementalQuest/CastleScenery/";
        foreach (string file in new[] { "Background", "Surface", "Fill", "Ledge" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(folder + file + ".png");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }
        foreach (string name in new[] { "CasteloDoEclipse", "ArenaEclipse" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity");
            var root = Object.FindAnyObjectByType<QuestCampaignScene>().gameObject;
            var art = root.GetComponent<QuestCastleScenery>();
            if (art == null) art = root.AddComponent<QuestCastleScenery>();
            art.backdrop = AssetDatabase.LoadAssetAtPath<Sprite>(folder + "Background.png");
            art.surface = AssetDatabase.LoadAssetAtPath<Sprite>(folder + "Surface.png");
            art.masonry = AssetDatabase.LoadAssetAtPath<Sprite>(folder + "Fill.png");
            art.ledge = AssetDatabase.LoadAssetAtPath<Sprite>(folder + "Ledge.png");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        Debug.Log("CASTLE_VISUAL_SETUP_OK");
        EditorApplication.Exit(0);
    }
}
