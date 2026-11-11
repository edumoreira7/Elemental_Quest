using System;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class QuestTutorialControls : MonoBehaviour
{
    [Serializable]
    public class Cartao
    {
        public float inicio, fim;
        public string tecla, acao;
        [NonSerialized] public bool mostrado;
    }
    public QuestTutorialArea tutorial;
    public Cartao[] cartoes;
    public bool Visivel { get; private set; }
    public int Exibidos { get; private set; }
    private Cartao atual;
    private float ate;
    void LateUpdate()
    {
        if (QuestSession.Atual != null && QuestSession.Atual.Pausado) { Visivel = false; return; }
        float x = tutorial.jogador.position.x;
        Visivel = atual != null && Time.time < ate && x >= atual.inicio && x < atual.fim;
        if (!Visivel)
            foreach (var cartao in cartoes)
                if (!cartao.mostrado && x >= cartao.inicio && x < cartao.fim)
                {
                    atual = cartao; cartao.mostrado = true; ate = Time.time + 3;
                    Exibidos++; Visivel = true; break;
                }
        if (Visivel) tutorial.DefinirDica("");
    }
    void OnGUI()
    {
        if (!Visivel) return;
        float largura = Mathf.Min(360, Screen.width - 24);
        var area = new Rect((Screen.width - largura) / 2, Screen.height - 68, largura, 42);
        Color anterior = GUI.color;
        GUI.color = new Color(.025f,.06f,.12f,.96f); GUI.DrawTexture(area,Texture2D.whiteTexture);
        GUI.color = new Color(1,.84f,.35f);
        var tecla = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
        GUI.Box(new Rect(area.x+8,area.y+7,110,28),atual.tecla,tecla);
        GUI.color = Color.white;
        var acao = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 16 };
        GUI.Label(new Rect(area.x+128,area.y+7,largura-134,28),atual.acao,acao);
        GUI.color = anterior;
    }
}
