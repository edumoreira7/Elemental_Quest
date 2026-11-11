using UnityEngine;
public class QuestArenaArrival : MonoBehaviour
{
    void OnGUI()
    {
        float largura=Mathf.Min(440,Screen.width-24);
        var area=new Rect((Screen.width-largura)/2,30,largura,108);
        GUI.Box(area,"");
        var estilo=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=17,wordWrap=true};
        GUI.Label(new Rect(area.x+8,area.y+8,largura-16,52),"Entrada da arena\nCombate do boss sera implementado depois.",estilo);
        if(GUI.Button(new Rect(area.x+24,area.y+70,largura-48,28),"Voltar a preparacao")) QuestForestStage.VoltarAPreparacao();
    }
}
