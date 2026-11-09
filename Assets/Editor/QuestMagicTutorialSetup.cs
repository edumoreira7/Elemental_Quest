using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestMagicTutorialSetup
{
    public static void PrepararTudo()
    {
        QuestRecoverySetup.Configurar();
        Configurar();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Elemental Quest/Magia/Adicionar trecho de teste na Fase1 %&m")]
    public static void Configurar()
    {
        if (Application.isPlaying)
            return;
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            if (SceneManager.GetSceneAt(indice).isDirty)
            {
                Debug.LogWarning("Salve as alteracoes de cena antes de montar o trecho de teste.");
                return;
            }
        }
        Scene anterior = SceneManager.GetActiveScene();
        Scene cena = SceneManager.GetSceneByPath("Assets/Scenes/Fase1.unity");
        bool aberta = cena.isLoaded;
        if (!aberta)
            cena = EditorSceneManager.OpenScene("Assets/Scenes/Fase1.unity", OpenSceneMode.Additive);
        SceneManager.SetActiveScene(cena);
        foreach (GameObject raiz in cena.GetRootGameObjects())
        {
            if (raiz.name == "Teste dos elementos")
            {
                SceneManager.SetActiveScene(anterior);
                if (!aberta)
                    EditorSceneManager.CloseScene(cena, true);
                return;
            }
        }
        GameObject grupo = new GameObject("Teste dos elementos");
        Criar(grupo, "Raizes inflamaveis", QuestElementTarget.Tipo.Raizes, new Vector3(29, -2.05f, 0), new Vector2(1f, 1.9f), "Fogo/04-Barreira-Raizes-Intacta", 1);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        SceneManager.SetActiveScene(anterior);
        if (!aberta)
            EditorSceneManager.CloseScene(cena, true);
        Debug.Log("EQ_MAGIC_SETUP_OK: trecho de teste adicionado sem reconstruir terreno. Nao representa o tutorial final.");
    }

    private static void Criar(GameObject grupo, string nome, QuestElementTarget.Tipo tipo, Vector3 posicao, Vector2 tamanho, string caminho, int quantidade)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo.transform);
        objeto.transform.position = posicao;
        BoxCollider2D colisor = objeto.AddComponent<BoxCollider2D>();
        colisor.size = tamanho;
        colisor.isTrigger = tipo != QuestElementTarget.Tipo.Raizes;
        objeto.layer = tipo == QuestElementTarget.Tipo.Raizes ? 3 : 0;
        objeto.AddComponent<QuestElementTarget>().tipo = tipo;
        Sprite[] quadros = QuestMagicArt.Quadros(caminho, quantidade);
        if (quadros.Length > 0)
        {
            GameObject desenho = new GameObject("Visual provisorio");
            desenho.transform.SetParent(objeto.transform, false);
            SpriteRenderer renderer = desenho.AddComponent<SpriteRenderer>();
            string pasta = "Assets/ElementalQuest/Atualizados/Magia";
            System.IO.Directory.CreateDirectory(pasta);
            AssetDatabase.Refresh();
            string arquivo = pasta + "/" + tipo + ".asset";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(arquivo);
            if (sprite == null)
            {
                sprite = Object.Instantiate(quadros[0]);
                AssetDatabase.CreateAsset(sprite, arquivo);
            }
            renderer.sprite = sprite;
            renderer.sortingOrder = 12;
            if (tipo == QuestElementTarget.Tipo.Lava)
                renderer.color = new Color(1f, .3f, .1f);
            desenho.transform.localScale = new Vector3(tamanho.x / quadros[0].bounds.size.x, tamanho.y / quadros[0].bounds.size.y, 1);
        }
    }
}
