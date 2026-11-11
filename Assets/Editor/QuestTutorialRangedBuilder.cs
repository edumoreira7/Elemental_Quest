using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestTutorialRangedBuilder
{
    private const string Pasta = "Assets/ElementalQuest/TutorialEtapa4";
    private const string Prefab = Pasta + "/AtiradorTutorial.prefab";
    private static Transform grupo;
    private static Material material;

    public static void ConstruirEValidar()
    {
        Construir();
        QuestRangedStageTest.Begin();
    }

    [MenuItem("Elemental Quest/Tutorial/Adicionar encontro com atirador")]
    public static void Construir()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Saia do Play antes de editar.");
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            if (SceneManager.GetSceneAt(indice).isDirty) throw new InvalidOperationException("Salve as alteracoes abertas antes de continuar.");
        }
        Directory.CreateDirectory(Pasta);
        AssetDatabase.Refresh();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefab) == null)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Fase1.unity", OpenSceneMode.Single);
            QuestEnemy existente = UnityEngine.Object.FindObjectsByType<QuestEnemy>().First(alvo => alvo.tipo == QuestEnemy.Tipo.Atirador);
            GameObject copia = UnityEngine.Object.Instantiate(existente.gameObject);
            copia.name = "AtiradorTutorial";
            copia.transform.position = Vector3.zero;
            PrefabUtility.SaveAsPrefabAsset(copia, Prefab);
            UnityEngine.Object.DestroyImmediate(copia);
        }
        Scene cena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        if (GameObject.Find("Combate a distancia - etapa 4") != null) throw new InvalidOperationException("Area ja existe; nada foi reconstruido.");
        grupo = new GameObject("Combate a distancia - etapa 4").transform;
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/TerrenoContinuo/TerrenoSemLuz.mat");
        Tilemap mapa = UnityEngine.Object.FindAnyObjectByType<Tilemap>();
        var tiles = AssetDatabase.FindAssets("t:Tile", new[] { "Assets/ElementalQuest/TerrenoContinuo" }).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Tile>).Where(tile => Mascara(tile) >= 0).ToDictionary(Mascara);
        for (int coluna = -12; coluna < 344; coluna++)
        {
            for (int linha = -7; linha <= -4; linha++)
            {
                bool norte = linha < -4, leste = coluna < 343, sul = linha > -7, oeste = coluna > -12;
                int mascara = (norte ? 1 : 0) | (leste ? 2 : 0) | (sul ? 4 : 0) | (oeste ? 8 : 0)
                    | (norte && leste ? 16 : 0) | (sul && leste ? 32 : 0) | (sul && oeste ? 64 : 0) | (norte && oeste ? 128 : 0);
                Tile tile = tiles[mascara];
                mapa.SetTile(new Vector3Int(coluna, linha, 0), tile);
            }
        }
        GameObject piso = GameObject.Find("Piso seguro sem abismos");
        piso.transform.position = new Vector3(166, -5, 0);
        piso.GetComponent<BoxCollider2D>().size = new Vector2(356, 4);
        GameObject limite = GameObject.Find("Limite seguro - fim do primeiro combate");
        limite.name = "Limite seguro - entrada do desafio final";
        limite.transform.position = new Vector3(342, 1, 0);
        UnityEngine.Object.FindAnyObjectByType<QuestCamera>().maximoX = 335;
        Cobertura("Abrigo do primeiro encontro", 277, 2.5f, 1);
        Cobertura("Expansao - degrau de salto", 293, 2, .6f);
        Cobertura("Expansao - abrigo elevado", 297, 2.5f, 1);
        QuestTutorialRanged encontro = grupo.gameObject.AddComponent<QuestTutorialRanged>();
        encontro.tutorial = UnityEngine.Object.FindAnyObjectByType<QuestTutorialArea>();
        encontro.jogador = encontro.tutorial.jogador;
        encontro.primeiro = Atirador("Atirador - introducao", 283, -2.98f);
        encontro.segundo = Atirador("Atirador - expansao sequencial", 305, -2.98f);
        GameObject marco = new GameObject("Entrada do desafio final");
        marco.transform.SetParent(grupo, false);
        marco.transform.position = new Vector3(324, -1.3f, 0);
        encontro.marco = marco.AddComponent<SpriteRenderer>();
        encontro.marco.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/TutorialEtapa2/Tutorial_Interface_12-Elemento-Selecionado.asset");
        encontro.marco.sortingOrder = 12;
        marco.transform.localScale = Vector3.one * (.7f / encontro.marco.sprite.bounds.size.y);
        foreach (float posicaoX in new[] { 273f, 287f, 292f, 300.5f, 315f, 329f }) Seta(posicaoX);
        QuestTutorialArea.Faixa[] novas = {
            new QuestTutorialArea.Faixa { inicio = 270, fim = 291, mensagem = "Observe o atirador" },
            new QuestTutorialArea.Faixa { inicio = 291, fim = 319, mensagem = "Pulo e abrigo: avance" },
            new QuestTutorialArea.Faixa { inicio = 319, fim = 342, mensagem = "Proximo: desafio final" }
        };
        encontro.tutorial.faixas = encontro.tutorial.faixas.Concat(novas).ToArray();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        AssetDatabase.SaveAssets();
        Debug.Log("EQ_RANGED_BUILD_OK: dois encontros sequenciais, projeteis existentes e entrada do desafio final.");
    }

    private static int Mascara(Tile tile)
    {
        string[] nomes = { "Tile_00", "Tile_01", "Tile_02", "Tile_03", "Tile_04", "Tile_05", "Tile_06", "Tile_07", "Tile_08" };
        int[] mascaras = { 38, 110, 76, 55, 255, 205, 19, 155, 137 };
        int indice = Array.IndexOf(nomes, Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(tile)));
        return indice >= 0 ? mascaras[indice] : -1;
    }

    private static QuestEnemy Atirador(string nome, float posicaoX, float altura)
    {
        GameObject objeto = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab));
        objeto.name = nome;
        objeto.transform.SetParent(grupo, true);
        objeto.transform.position = new Vector3(posicaoX, altura, 0);
        QuestEnemy inimigo = objeto.GetComponent<QuestEnemy>();
        inimigo.limiteEsquerdo = posicaoX - .5f;
        inimigo.limiteDireito = posicaoX + .5f;
        objeto.SetActive(false);
        return inimigo;
    }

    private static void Cobertura(string nome, float inicio, float largura, float altura)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = new Vector2(inicio + largura * .5f, -3 + altura * .5f);
        objeto.layer = 3;
        objeto.AddComponent<BoxCollider2D>().size = new Vector2(largura, altura);
        int pecas = Mathf.CeilToInt(largura);
        for (int indice = 0; indice < pecas; indice++)
        {
            int tileIndice = indice == 0 ? 0 : indice == pecas - 1 ? 2 : 1;
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_" + tileIndice.ToString("00") + ".asset");
            GameObject desenho = new GameObject("Terreno existente");
            desenho.transform.SetParent(objeto.transform, false);
            desenho.transform.localPosition = new Vector3(-largura * .5f + (indice + .5f) * largura / pecas, .0625f * altura, 0);
            SpriteRenderer renderer = desenho.AddComponent<SpriteRenderer>();
            renderer.sprite = tile.sprite;
            renderer.sharedMaterial = material;
            desenho.transform.localScale = new Vector3(largura / pecas, altura, 1);
        }
    }

    private static void Seta(float posicaoX)
    {
        GameObject objeto = new GameObject("Rota do atirador");
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = new Vector3(posicaoX, -1.6f, 0);
        TextMesh texto = objeto.AddComponent<TextMesh>();
        texto.text = ">";
        texto.fontSize = 48;
        texto.characterSize = .1f;
        texto.anchor = TextAnchor.MiddleCenter;
        texto.color = new Color(1, .8f, .2f);
        objeto.GetComponent<MeshRenderer>().sortingOrder = 14;
    }
}
