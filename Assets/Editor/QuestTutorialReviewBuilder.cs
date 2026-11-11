using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestTutorialReviewBuilder
{
    public static void RevisarEValidar()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Saia do Play.");
        for (int i=0;i<SceneManager.sceneCount;i++)
            if(SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Salve as alteracoes abertas.");
        var cena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity",OpenSceneMode.Single);
        var area = UnityEngine.Object.FindAnyObjectByType<QuestTutorialArea>();
        var controles = area.GetComponent<QuestTutorialControls>();
        if (controles == null) controles = area.gameObject.AddComponent<QuestTutorialControls>();
        controles.tutorial = area;
        controles.cartoes = new[] {
            Card(0,8,"A / D","Mover"), Card(9,20,"Shift esq.","Correr"), Card(21,27,"Espaço","Pular"),
            Card(49,56,"1 2 3 4","Trocar elemento"), Card(56,74,"1 + J","Criar degrau"),
            Card(74,96,"4 + J","Congelar agua"), Card(96,106.5f,"2 + Espaço","No alto: outro salto"),
            Card(106.5f,123,"Segure J","Planar ao cair"), Card(123,143,"3 + J","Queimar raizes")
        };
        if (GameObject.Find("Vida antes da arena") == null)
        {
            var vida = new GameObject("Vida antes da arena");
            vida.transform.position = new Vector3(194,-2.2f,0);
            vida.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal0.png");
            var caixa = vida.AddComponent<BoxCollider2D>(); caixa.isTrigger=true; caixa.size=new Vector2(.8f,1.2f);
            vida.AddComponent<QuestLifePickup>();
        }
        foreach (var raiz in cena.GetRootGameObjects())
            foreach(var transform in raiz.GetComponentsInChildren<Transform>(true))
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)
                    throw new InvalidOperationException("Script ausente: " + transform.name);
        if (area.faixas.Any(f=>f.mensagem.Length>26)) throw new InvalidOperationException("Dica longa encontrada.");
        EditorSceneManager.MarkSceneDirty(cena); EditorSceneManager.SaveScene(cena); AssetDatabase.SaveAssets();
        Debug.Log("EQ_REVIEW_BUILD_OK: cartoes contextuais separados, sem scripts ausentes.");
        QuestTutorialReviewTest.Begin();
    }
    static QuestTutorialControls.Cartao Card(float inicio,float fim,string tecla,string acao)
    { return new QuestTutorialControls.Cartao {inicio=inicio,fim=fim,tecla=tecla,acao=acao}; }
}
