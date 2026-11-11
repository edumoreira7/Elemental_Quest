using System;
using UnityEngine;

public class QuestTutorialArea : MonoBehaviour
{
    [Serializable]
    public struct Faixa
    {
        public float inicio;
        public float fim;
        public string mensagem;
    }

    public Transform jogador;
    public Faixa[] faixas;
    public float fimDaArea = 43f;
    public Font fonte;
    public bool Concluiu { get; private set; }
    public string MensagemAtual { get; private set; }
    private GUIStyle estilo;

    public void DefinirDica(string mensagem)
    {
        MensagemAtual = mensagem;
    }

    public string MensagemEm(float posicaoX)
    {
        foreach (Faixa faixa in faixas)
        {
            if (posicaoX >= faixa.inicio && posicaoX < faixa.fim)
                return faixa.mensagem;
        }
        return string.Empty;
    }

    void Update()
    {
        if (jogador == null)
            return;
        MensagemAtual = MensagemEm(jogador.position.x);
        Concluiu |= jogador.position.x >= fimDaArea;
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(MensagemAtual) || (QuestSession.Atual != null && QuestSession.Atual.Pausado))
            return;
        if (estilo == null)
            estilo = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = false, padding = new RectOffset(12, 12, 0, 0) };
        estilo.font = fonte;
        estilo.fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * .022f), 14, 18);
        float largura = Mathf.Min(Screen.width - 32, estilo.CalcSize(new GUIContent(MensagemAtual)).x + 12);
        Rect painel = new Rect((Screen.width - largura) * .5f, Screen.height - 62, largura, 36);
        Color anterior = GUI.color;
        GUI.color = new Color(.025f, .06f, .12f, .92f);
        GUI.DrawTexture(painel, Texture2D.whiteTexture);
        GUI.color = new Color(.9f, .75f, .3f, .65f);
        GUI.DrawTexture(new Rect(painel.x + 12, painel.yMax - 2, painel.width - 24, 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(painel, MensagemAtual, estilo);
        GUI.color = anterior;
    }
}
