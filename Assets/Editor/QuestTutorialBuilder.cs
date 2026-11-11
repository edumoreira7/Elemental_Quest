using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestTutorialBuilder
{
    private const string Cena = "Assets/Scenes/Tutorial.unity";
    private const string Pasta = "Assets/ElementalQuest/TutorialEtapa1";
    private static Material material;
    private static Sprite grama;
    private static Sprite terra;

    public static void CriarEValidar()
    {
        Criar();
        QuestTutorialStageTest.Begin();
    }

    [MenuItem("Elemental Quest/Tutorial/Criar primeira area segura")]
    public static void Criar()
    {
        if (Application.isPlaying || File.Exists(Cena))
            throw new InvalidOperationException("A cena Tutorial ja existe ou o Play esta ativo. Nada foi reconstruido.");
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            if (SceneManager.GetSceneAt(indice).isDirty)
                throw new InvalidOperationException("Salve as alteracoes abertas antes de criar o tutorial.");
        }
        Directory.CreateDirectory(Pasta);
        AssetDatabase.Refresh();
        Sprite[] cenario = AssetDatabase.LoadAllAssetsAtPath("Assets/ElementalQuest/Recortes/Cenario.png").OfType<Sprite>().ToArray();
        grama = cenario.First(sprite => int.Parse(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1)) == 1);
        terra = cenario.First(sprite => int.Parse(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1)) == 9);
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/Arte/SpritesSemLuz.mat");
        Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SceneManager.SetActiveScene(cena);
        GameObject cameraObjeto = new GameObject("Main Camera");
        cameraObjeto.tag = "MainCamera";
        Camera camera = cameraObjeto.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.backgroundColor = new Color(.18f, .5f, .8f);
        camera.transform.position = new Vector3(2, .3f, -10);
        cameraObjeto.AddComponent<AudioListener>();
        GameObject jogador = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/Prefabs/PatoMago.prefab"));
        jogador.name = "Pato Mago - controles existentes";
        jogador.transform.position = new Vector3(0, -2.3f, 0);
        jogador.transform.localScale = Vector3.one * .72f;
        QuestPlayer vida = jogador.GetComponent<QuestPlayer>();
        if (vida == null)
            vida = jogador.AddComponent<QuestPlayer>();
        vida.pontoSeguro = jogador.transform.position;
        QuestCamera seguir = cameraObjeto.AddComponent<QuestCamera>();
        seguir.alvo = jogador.transform;
        seguir.minimoX = 0;
        seguir.maximoX = 47;
        QuestBackgroundSetup.Criar(camera, jogador.transform, 1);
        Chao();
        Solido("Limite seguro esquerdo", new Vector2(-7, 1), new Vector2(1, 12));
        Solido("Reserva para a proxima area", new Vector2(54, 1), new Vector2(1, 12));
        Plataforma("Obstaculo baixo - aprender pulo", 23, 1.2f, -2.4f, .6f);
        Plataforma("Combinacao 1", 28.5f, 3, -2.4f);
        Plataforma("Combinacao 2", 33, 3, -1.8f);
        Plataforma("Combinacao 3", 37.5f, 3, -1.2f);
        Vector2[] trilha = {
            new Vector2(2, -2.15f), new Vector2(6, -2.15f),
            new Vector2(11, -2.15f), new Vector2(15, -2.15f), new Vector2(19, -2.15f),
            new Vector2(23.6f, -1.55f), new Vector2(30, -1.55f),
            new Vector2(34.5f, -.95f), new Vector2(39, -.35f), new Vector2(46, -2.15f)
        };
        foreach (Vector2 posicao in trilha)
            Cristal(posicao);
        foreach (float posicaoX in new[] { 9f, 21f, 27f, 43f, 49f })
            Seta(posicaoX);
        QuestSession sessao = new GameObject("Sessao do tutorial").AddComponent<QuestSession>();
        sessao.fase = 1;
        sessao.nomeFase = "Tutorial - Primeiros passos";
        sessao.totalItens = trilha.Length;
        QuestTutorialArea tutorial = new GameObject("Orientacao contextual - uma mensagem por vez").AddComponent<QuestTutorialArea>();
        tutorial.jogador = jogador.transform;
        tutorial.faixas = new[] {
            Faixa(-9, 8.5f, "A / D ou setas: andar"),
            Faixa(9, 20.5f, "Shift esq.: correr"),
            Faixa(21, 26.5f, "Espaço: pular"),
            Faixa(27, 42, "Direção + Espaço: subir"),
            Faixa(43, 54, "Siga a trilha")
        };
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena, Cena);
        AssetDatabase.SaveAssets();
        Debug.Log("EQ_TUTORIAL_BUILD_OK: primeira area segura salva em " + Cena);
    }

    private static QuestTutorialArea.Faixa Faixa(float inicio, float fim, string mensagem)
    {
        return new QuestTutorialArea.Faixa { inicio = inicio, fim = fim, mensagem = mensagem };
    }

    private static void Chao()
    {
        GameObject grid = new GameObject("Chao continuo seguro", typeof(Grid));
        GameObject desenho = new GameObject("Tilemap - terreno existente", typeof(Tilemap), typeof(TilemapRenderer));
        desenho.transform.SetParent(grid.transform, false);
        Tilemap mapa = desenho.GetComponent<Tilemap>();
        desenho.GetComponent<TilemapRenderer>().sharedMaterial = material;
        Tile topo = TileExistente("Grama", grama);
        Tile miolo = TileExistente("Terra", terra);
        for (int coluna = -12; coluna < 60; coluna++)
        {
            for (int linha = -7; linha <= -4; linha++)
                mapa.SetTile(new Vector3Int(coluna, linha, 0), linha == -4 ? topo : miolo);
        }
        GameObject piso = Solido("Piso seguro sem abismos", new Vector2(24, -5), new Vector2(72, 4));
        piso.transform.SetParent(grid.transform, true);
    }

    private static Tile TileExistente(string nome, Sprite sprite)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.None;
        tile.transform = Matrix4x4.Scale(new Vector3(1 / sprite.bounds.size.x, 1 / sprite.bounds.size.y, 1));
        AssetDatabase.CreateAsset(tile, Pasta + "/" + nome + ".asset");
        return tile;
    }

    private static GameObject Solido(string nome, Vector2 posicao, Vector2 tamanho)
    {
        GameObject objeto = new GameObject(nome);
        objeto.layer = 3;
        objeto.transform.position = posicao;
        objeto.AddComponent<BoxCollider2D>().size = tamanho;
        return objeto;
    }

    private static void Plataforma(string nome, float inicio, float largura, float topo, float espessura = .36f)
    {
        GameObject objeto = Solido(nome, new Vector2(inicio + largura * .5f, topo - espessura * .5f), new Vector2(largura, espessura));
        int pecas = Mathf.CeilToInt(largura);
        for (int indice = 0; indice < pecas; indice++)
        {
            GameObject desenho = new GameObject("Superficie de grama");
            desenho.transform.SetParent(objeto.transform, false);
            desenho.transform.localPosition = new Vector3(-largura * .5f + (indice + .5f) * largura / pecas, 0, 0);
            SpriteRenderer visual = desenho.AddComponent<SpriteRenderer>();
            visual.sprite = grama;
            visual.sharedMaterial = material;
            desenho.transform.localScale = new Vector3(largura / pecas / grama.bounds.size.x, espessura / grama.bounds.size.y, 1);
        }
    }

    private static void Cristal(Vector2 posicao)
    {
        GameObject objeto = new GameObject("Cristal - guia do percurso");
        objeto.transform.position = posicao;
        Sprite[] quadros = Enumerable.Range(0, 6).Select(indice => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal" + indice + ".png")).ToArray();
        SpriteRenderer visual = objeto.AddComponent<SpriteRenderer>();
        visual.sprite = quadros[0];
        visual.sharedMaterial = material;
        visual.sortingOrder = 5;
        objeto.AddComponent<QuestAnimation>().quadros = quadros;
        objeto.AddComponent<CircleCollider2D>().radius = .28f;
        objeto.GetComponent<CircleCollider2D>().isTrigger = true;
        objeto.AddComponent<QuestCrystal>();
    }

    private static void Seta(float posicaoX)
    {
        GameObject objeto = new GameObject("Seta - seguir a direita");
        objeto.transform.position = new Vector3(posicaoX, -1.7f, 0);
        TextMesh texto = objeto.AddComponent<TextMesh>();
        texto.text = ">";
        texto.fontSize = 48;
        texto.characterSize = .1f;
        texto.anchor = TextAnchor.MiddleCenter;
        texto.color = new Color(1, .8f, .2f);
        objeto.GetComponent<MeshRenderer>().sortingOrder = 12;
    }
}
