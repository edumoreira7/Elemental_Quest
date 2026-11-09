using UnityEditor;
using UnityEngine;

public class QuestMagicImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/ElementalMagic/"))
            return;
        TextureImporter textura = (TextureImporter)assetImporter;
        textura.textureType = TextureImporterType.Default;
        textura.filterMode = FilterMode.Point;
        textura.textureCompression = TextureImporterCompression.Uncompressed;
        textura.alphaIsTransparency = true;
        textura.mipmapEnabled = false;
        textura.isReadable = true;
        textura.maxTextureSize = 4096;
        textura.npotScale = TextureImporterNPOTScale.None;
    }
}
