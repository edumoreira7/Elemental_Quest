using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestTutorialFinalBuilder
{
    const string Pasta = "Assets/ElementalQuest/TutorialFinal";
    public static void ConstruirEValidar() { Construir(); QuestFinalStageTest.Begin(); }
    [MenuItem("Elemental Quest/Tutorial/Encurtar e montar desafio final")]
    public static void Construir()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Saia do Play.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Salve as cenas abertas.");
        Directory.CreateDirectory(Pasta);
        AssetDatabase.Refresh();
        var fonte = EditorSceneManager.OpenScene("Assets/Scenes/Fase1.unity", OpenSceneMode.Single);
        var forte = UnityEngine.Object.FindObjectsByType<QuestEnemy>().First(x => x.tipo == QuestEnemy.Tipo.Guardiao);
        SalvarPrefab(forte.gameObject, "GuardiaoTutorial");
        SalvarPrefab(UnityEngine.Object.FindAnyObjectByType<QuestPortal>().gameObject, "PortalTutorial");
        Scene cena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        if (UnityEngine.Object.FindAnyObjectByType<QuestTutorialFinal>() != null) throw new InvalidOperationException("Etapa final ja existe.");
        var treino = UnityEngine.Object.FindAnyObjectByType<QuestElementTraining>();
        foreach (Transform filho in treino.transform.Cast<Transform>().ToArray())
            if (filho.position.x >= 143) UnityEngine.Object.DestroyImmediate(filho.gameObject);
        treino.estacoes = treino.estacoes.Take(6).ToArray();
        var combate = UnityEngine.Object.FindAnyObjectByType<QuestTutorialCombat>();
        foreach (Transform filho in combate.transform) filho.position += Vector3.left * 80;
        combate.inimigo.limiteEsquerdo -= 80;
        combate.inimigo.limiteDireito -= 80;
        combate.inicio = 143; combate.fimObservacao = 152; combate.saida = 168; combate.fimDaArea = 172;
        combate.marco.transform.position = new Vector3(170, -1.3f, 0);
        var tiros = UnityEngine.Object.FindAnyObjectByType<QuestTutorialRanged>();
        foreach (Transform filho in tiros.transform.Cast<Transform>().ToArray())
        {
            if (filho.gameObject == tiros.marco.gameObject) continue;
            if (filho.position.x >= 291) UnityEngine.Object.DestroyImmediate(filho.gameObject);
            else filho.position += Vector3.left * 98;
        }
        tiros.segundo = null;
        tiros.primeiro.limiteEsquerdo -= 98; tiros.primeiro.limiteDireito -= 98;
        tiros.marco.transform.position = new Vector3(191, -1.3f, 0);
        tiros.deslocamentoX = -98; tiros.fimDaArea = 193; tiros.saidaLocal = 288;
        var tutorial = UnityEngine.Object.FindAnyObjectByType<QuestTutorialArea>();
        tutorial.faixas = tutorial.faixas.Where(f => f.inicio < 143 && f.fim <= 143).Concat(new[] {
            new QuestTutorialArea.Faixa { inicio = 143, fim = 172, mensagem = "Observe e contra-ataque" },
            new QuestTutorialArea.Faixa { inicio = 172, fim = 193, mensagem = "Desvie e use o abrigo" },
            new QuestTutorialArea.Faixa { inicio = 193, fim = 232, mensagem = "Desafio final" }
        }).ToArray();
        var mapa = UnityEngine.Object.FindAnyObjectByType<Tilemap>();
        mapa.ClearAllTiles();
        for (int x = -12; x < 232; x++)
            for (int y = -7; y <= -4; y++)
            {
                int coluna = x == -12 ? 0 : x == 231 ? 2 : 1;
                int linha = y == -4 ? 0 : y == -7 ? 2 : 1;
                mapa.SetTile(new Vector3Int(x,y,0), AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_" + (linha * 3 + coluna).ToString("00") + ".asset"));
            }
        var piso = GameObject.Find("Piso seguro sem abismos");
        piso.transform.position = new Vector3(110,-5,0);
        piso.GetComponent<BoxCollider2D>().size = new Vector2(244,4);
        var limite = GameObject.Find("Limite seguro - entrada do desafio final");
        limite.name = "Limite seguro - fim do tutorial";
        limite.transform.position = new Vector3(230,1,0);
        UnityEngine.Object.FindAnyObjectByType<QuestCamera>().maximoX = 223;
        var grupo = new GameObject("Integracao final - sem boss").transform;
        var final = grupo.gameObject.AddComponent<QuestTutorialFinal>();
        final.tutorial = tutorial;
        GameObject posto = new GameObject("Checkpoint antes da arena");
        posto.transform.SetParent(grupo);
        posto.transform.position = new Vector3(196,-2.2f,0);
        posto.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal0.png");
        var caixa = posto.AddComponent<BoxCollider2D>(); caixa.isTrigger = true; caixa.size = new Vector2(1,1.6f);
        final.checkpoint = posto.AddComponent<QuestCheckpoint>();
        final.checkpoint.destino = new Vector3(196,-2.35f,0);
        var inimigo = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Pasta + "/GuardiaoTutorial.prefab"));
        inimigo.transform.SetParent(grupo); inimigo.transform.position = new Vector3(212,-3,0);
        final.inimigo = inimigo.GetComponent<QuestEnemy>();
        final.inimigo.limiteEsquerdo = 209; final.inimigo.limiteDireito = 215;
        inimigo.SetActive(false);
        var portal = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Pasta + "/PortalTutorial.prefab"));
        portal.name = "Objetivo - concluir tutorial";
        portal.transform.SetParent(grupo); portal.transform.position = new Vector3(225,-2,0);
        final.portal = portal.GetComponent<QuestPortal>();
        var sessao = UnityEngine.Object.FindAnyObjectByType<QuestSession>();
        sessao.tutorialFinal = final;
        Degrau(grupo, 201, 1.5f, .6f);
        Degrau(grupo, 218, 2, .6f);
        foreach (float x in new[] { 194f, 200f, 206f, 221f })
        {
            var seta = new GameObject("Rota final"); seta.transform.SetParent(grupo); seta.transform.position = new Vector3(x,-1.5f,0);
            var texto = seta.AddComponent<TextMesh>(); texto.text = ">"; texto.fontSize = 48; texto.characterSize = .1f; texto.color = Color.yellow; texto.anchor = TextAnchor.MiddleCenter;
            seta.GetComponent<MeshRenderer>().sortingOrder = 14;
        }
        EditorSceneManager.MarkSceneDirty(cena); EditorSceneManager.SaveScene(cena); AssetDatabase.SaveAssets();
        if (!EditorBuildSettings.scenes.Any(s => s.path == "Assets/Scenes/Tutorial.unity"))
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene("Assets/Scenes/Tutorial.unity", true) }).ToArray();
        Debug.Log("EQ_FINAL_BUILD_OK: percurso -12..232; 32% menor apesar da arena; seis introducoes preservadas.");
    }
    static void SalvarPrefab(GameObject fonte, string nome)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(Pasta + "/" + nome + ".prefab") != null) return;
        var copia = UnityEngine.Object.Instantiate(fonte); copia.name = nome; copia.transform.position = Vector3.zero;
        PrefabUtility.SaveAsPrefabAsset(copia, Pasta + "/" + nome + ".prefab"); UnityEngine.Object.DestroyImmediate(copia);
    }
    static void Degrau(Transform grupo, float inicio, float largura, float altura)
    {
        var obj = new GameObject("Apoio de pulo e recuo"); obj.transform.SetParent(grupo); obj.layer = 3;
        obj.transform.position = new Vector3(inicio + largura / 2, -3 + altura / 2, 0);
        obj.AddComponent<BoxCollider2D>().size = new Vector2(largura,altura);
        var visual = obj.AddComponent<SpriteRenderer>();
        visual.sprite = AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_01.asset").sprite;
        visual.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/TerrenoContinuo/TerrenoSemLuz.mat");
        visual.drawMode = SpriteDrawMode.Sliced; visual.size = new Vector2(largura,altura);
    }
}
