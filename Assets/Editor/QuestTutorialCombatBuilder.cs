using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestTutorialCombatBuilder
{
    private const string Terreno = "Assets/ElementalQuest/TerrenoContinuo";
    private const string Pasta = "Assets/ElementalQuest/TutorialEtapa3";
    private const string Prefab = Pasta + "/GuaxinimCorpoACorpo.prefab";
    [Serializable] private class Grade { public int cell; public int columns; public int rows; public Peca[] tiles; }
    [Serializable] private class Peca { public string type; public int mask; public int index; public int column; public int row; }

    public static void ConstruirEValidar()
    {
        Construir();
        QuestCombatStageTest.Begin();
    }

    [MenuItem("Elemental Quest/Tutorial/Adicionar primeiro combate e terreno novo")]
    public static void Construir()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Saia do Play antes de editar a cena.");
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            if (SceneManager.GetSceneAt(indice).isDirty) throw new InvalidOperationException("Salve as cenas abertas antes de continuar.");
        }
        Directory.CreateDirectory(Pasta);
        AssetDatabase.Refresh();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
        {
            Scene origem = EditorSceneManager.OpenScene("Assets/Scenes/Fase1.unity", OpenSceneMode.Single);
            QuestEnemy existente = UnityEngine.Object.FindObjectsByType<QuestEnemy>().First(inimigo => inimigo.tipo == QuestEnemy.Tipo.Guaxinim);
            GameObject copia = UnityEngine.Object.Instantiate(existente.gameObject);
            copia.name = "GuaxinimCorpoACorpo";
            copia.transform.position = Vector3.zero;
            PrefabUtility.SaveAsPrefabAsset(copia, Prefab);
            UnityEngine.Object.DestroyImmediate(copia);
        }
        Scene cena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        if (GameObject.Find("Primeiro combate - etapa 3") != null) throw new InvalidOperationException("Encontro ja existe; nada foi reconstruido.");
        Tilemap mapa = UnityEngine.Object.FindAnyObjectByType<Tilemap>();
        Sprite gramaAntiga = ((Tile)mapa.GetTile(new Vector3Int(0, -4, 0))).sprite;
        Grade grade = JsonUtility.FromJson<Grade>(File.ReadAllText(Terreno + "/grade-e-validacao.json"));
        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(Terreno + "/Terreno-Continuo-128.png");
        importador.textureType = TextureImporterType.Default;
        importador.filterMode = FilterMode.Point;
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.alphaIsTransparency = true;
        importador.isReadable = true;
        importador.mipmapEnabled = false;
        importador.npotScale = TextureImporterNPOTScale.None;
        importador.maxTextureSize = 2048;
        importador.SaveAndReimport();
        Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(Terreno + "/Terreno-Continuo-128.png");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Terreno + "/TerrenoSemLuz.mat");
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            material.mainTexture = atlas;
            AssetDatabase.CreateAsset(material, Terreno + "/TerrenoSemLuz.mat");
        }
        Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();
        foreach (Peca peca in grade.tiles)
        {
            string caminhoSprite = Terreno + "/Sprite_" + peca.index.ToString("00") + ".asset";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminhoSprite);
            if (sprite == null)
            {
                sprite = Sprite.Create(atlas, new Rect(peca.column * grade.cell, (grade.rows - 1 - peca.row) * grade.cell, grade.cell, grade.cell), new Vector2(.5f, .5f), grade.cell);
                AssetDatabase.CreateAsset(sprite, caminhoSprite);
            }
            string caminhoTile = Terreno + "/Tile_" + peca.index.ToString("00") + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(caminhoTile);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                AssetDatabase.CreateAsset(tile, caminhoTile);
            }
            if (peca.type == "autotile") tiles[peca.mask] = tile;
        }
        mapa.GetComponent<TilemapRenderer>().sharedMaterial = material;
        mapa.tileAnchor = new Vector3(.5f, .5625f, 0);
        for (int coluna = -12; coluna < 272; coluna++)
        {
            for (int linha = -7; linha <= -4; linha++)
            {
                bool norte = linha < -4;
                bool leste = coluna < 271;
                bool sul = linha > -7;
                bool oeste = coluna > -12;
                int mascara = (norte ? 1 : 0) | (leste ? 2 : 0) | (sul ? 4 : 0) | (oeste ? 8 : 0)
                    | (norte && leste ? 16 : 0) | (sul && leste ? 32 : 0) | (sul && oeste ? 64 : 0) | (norte && oeste ? 128 : 0);
                mapa.SetTile(new Vector3Int(coluna, linha, 0), tiles[mascara]);
            }
        }
        SpriteRenderer[] antigos = UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(renderer => renderer.sprite == gramaAntiga).ToArray();
        foreach (IGrouping<Transform, SpriteRenderer> conjunto in antigos.GroupBy(renderer => renderer.transform.parent))
        {
            SpriteRenderer[] pecas = conjunto.OrderBy(renderer => renderer.transform.localPosition.x).ToArray();
            for (int indice = 0; indice < pecas.Length; indice++)
            {
                SpriteRenderer renderer = pecas[indice];
                Sprite sprite = tiles[pecas.Length == 1 ? 0 : indice == 0 ? 2 : indice == pecas.Length - 1 ? 8 : 10].sprite;
                Vector3 escala = renderer.transform.localScale;
                escala.x *= renderer.sprite.bounds.size.x / sprite.bounds.size.x;
                escala.y *= renderer.sprite.bounds.size.y / sprite.bounds.size.y;
                renderer.sprite = sprite;
                renderer.sharedMaterial = material;
                renderer.transform.localScale = escala;
                renderer.transform.localPosition += Vector3.up * (.0625f * escala.y);
            }
        }
        GameObject piso = GameObject.Find("Piso seguro sem abismos");
        piso.transform.position = new Vector3(130, -5, 0);
        piso.GetComponent<BoxCollider2D>().size = new Vector2(284, 4);
        GameObject limite = GameObject.Find("Limite seguro - fim do treino");
        limite.name = "Limite seguro - fim do primeiro combate";
        limite.transform.position = new Vector3(270, 1, 0);
        UnityEngine.Object.FindAnyObjectByType<QuestCamera>().maximoX = 263;
        GameObject grupo = new GameObject("Primeiro combate - etapa 3");
        GameObject adversario = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab));
        adversario.name = "Guaxinim - primeiro encontro";
        adversario.transform.SetParent(grupo.transform, true);
        adversario.transform.position = new Vector3(237, -2.98f, 0);
        QuestEnemy inimigo = adversario.GetComponent<QuestEnemy>();
        inimigo.limiteEsquerdo = 235;
        inimigo.limiteDireito = 239.5f;
        QuestTutorialCombat encontro = grupo.AddComponent<QuestTutorialCombat>();
        encontro.tutorial = UnityEngine.Object.FindAnyObjectByType<QuestTutorialArea>();
        encontro.jogador = encontro.tutorial.jogador;
        encontro.inimigo = inimigo;
        GameObject marco = new GameObject("Passagem depois do encontro");
        marco.transform.SetParent(grupo.transform, false);
        marco.transform.position = new Vector3(254, -1.8f, 0);
        encontro.marco = marco.AddComponent<SpriteRenderer>();
        encontro.marco.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/TutorialEtapa2/Tutorial_Interface_12-Elemento-Selecionado.asset");
        encontro.marco.sortingOrder = 12;
        float tamanhoMarco = encontro.marco.sprite.bounds.size.y;
        marco.transform.localScale = Vector3.one * (.6f / tamanhoMarco);
        Texto("Aviso visual antes da patrulha", "!", new Vector3(234, -.9f, 0), new Color(1, .6f, .15f), grupo.transform);
        foreach (float posicaoX in new[] { 225f, 230f, 253f, 261f })
            Texto("Indicacao do caminho", ">", new Vector3(posicaoX, -1.7f, 0), new Color(1, .8f, .2f), grupo.transform);
        List<QuestTutorialArea.Faixa> faixas = encontro.tutorial.faixas.ToList();
        faixas.Add(new QuestTutorialArea.Faixa { inicio = 223, fim = 232, mensagem = "Observe a patrulha" });
        faixas.Add(new QuestTutorialArea.Faixa { inicio = 232, fim = 252, mensagem = "J: ataque; A/D: recue" });
        faixas.Add(new QuestTutorialArea.Faixa { inicio = 252, fim = 270, mensagem = "Derrote o guaxinim" });
        encontro.tutorial.faixas = faixas.ToArray();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        AssetDatabase.SaveAssets();
        Debug.Log("EQ_COMBAT_BUILD_OK: terreno atualizado, um Guaxinim reutilizado e encontro conectado sem alterar IA.");
    }

    private static void Texto(string nome, string mensagem, Vector3 posicao, Color cor, Transform grupo)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = posicao;
        TextMesh texto = objeto.AddComponent<TextMesh>();
        texto.text = mensagem;
        texto.fontSize = 48;
        texto.characterSize = .1f;
        texto.anchor = TextAnchor.MiddleCenter;
        texto.color = cor;
        objeto.GetComponent<MeshRenderer>().sortingOrder = 14;
    }
}
