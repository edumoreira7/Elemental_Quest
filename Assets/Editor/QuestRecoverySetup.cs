using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestRecoverySetup
{
    [MenuItem("Elemental Quest/Adicionar checkpoints e vidas nas fases")]
    public static void Configurar()
    {
        if (Application.isPlaying)
            return;
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            if (SceneManager.GetSceneAt(indice).isDirty)
            {
                Debug.LogWarning("Salve as alteracoes da cena antes de adicionar checkpoints.");
                return;
            }
        }
        Scene ativa = SceneManager.GetActiveScene();
        foreach (string nome in new[] { "Fase1", "Fase2" })
        {
            string caminho = "Assets/Scenes/" + nome + ".unity";
            Scene cena = SceneManager.GetSceneByPath(caminho);
            bool aberta = cena.isLoaded;
            if (!aberta)
                cena = EditorSceneManager.OpenScene(caminho, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(cena);
            bool existe = false;
            foreach (GameObject raiz in cena.GetRootGameObjects())
                existe |= raiz.name == "Recuperacao da fase";
            if (!existe)
            {
                GameObject grupo = new GameObject("Recuperacao da fase");
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal0.png");
                if (sprite == null)
                {
                    Object.DestroyImmediate(grupo);
                    Debug.LogError("Sprite de cristal nao encontrado; nenhuma recuperacao adicionada.");
                }
                else
                {
                    float posicaoX = nome == "Fase1" ? 19f : 15f;
                    GameObject checkpoint = Objeto("Checkpoint", grupo.transform, sprite, new Vector3(posicaoX, -2.2f, 0), Color.cyan);
                    checkpoint.AddComponent<QuestCheckpoint>().destino = new Vector3(posicaoX, -2.3f, 0);
                    Objeto("Vida coletavel", grupo.transform, sprite, new Vector3(posicaoX + 2f, -2.2f, 0), Color.red).AddComponent<QuestLifePickup>();
                    EditorSceneManager.MarkSceneDirty(cena);
                    EditorSceneManager.SaveScene(cena);
                }
            }
            if (!aberta)
                EditorSceneManager.CloseScene(cena, true);
        }
        SceneManager.SetActiveScene(ativa);
        Debug.Log("EQ_RECOVERY_SETUP_OK: checkpoints e vidas nas duas fases; visuais provisorios de cristal ciano e vermelho.");
    }

    private static GameObject Objeto(string nome, Transform grupo, Sprite sprite, Vector3 posicao, Color cor)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo);
        objeto.transform.position = posicao;
        SpriteRenderer visual = objeto.AddComponent<SpriteRenderer>();
        visual.sprite = sprite;
        visual.color = cor;
        visual.sortingOrder = 15;
        BoxCollider2D colisor = objeto.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;
        colisor.size = new Vector2(.8f, 1.2f);
        return objeto;
    }
}
