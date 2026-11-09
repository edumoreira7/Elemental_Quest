using System.Collections.Generic;
using UnityEngine;

public class QuestMagicArt : MonoBehaviour
{
    private static readonly Dictionary<string, Sprite[]> cache = new Dictionary<string, Sprite[]>();
    private Sprite[] quadros;
    private SpriteRenderer visual;
    private float inicio;
    private bool repetir;
    private float fps;

    public static Sprite[] Quadros(string caminho, int quantidade = 6)
    {
        string chave = caminho + quantidade;
        if (cache.TryGetValue(chave, out Sprite[] existentes))
            return existentes;
        Texture2D textura = Resources.Load<Texture2D>("ElementalMagic/" + caminho);
        if (textura == null)
            return new Sprite[0];
        Sprite[] resultado = new Sprite[quantidade];
        int largura = textura.width / quantidade;
        Color32[] pixels = textura.GetPixels32();
        int esquerda = largura;
        int direita = -1;
        int baixo = textura.height;
        int cima = -1;
        for (int quadro = 0; quadro < quantidade; quadro++)
        {
            for (int posicaoY = 0; posicaoY < textura.height; posicaoY++)
            {
                for (int posicaoX = 0; posicaoX < largura; posicaoX++)
                {
                    if (pixels[posicaoY * textura.width + quadro * largura + posicaoX].a <= 16)
                        continue;
                    esquerda = Mathf.Min(esquerda, posicaoX);
                    direita = Mathf.Max(direita, posicaoX);
                    baixo = Mathf.Min(baixo, posicaoY);
                    cima = Mathf.Max(cima, posicaoY);
                }
            }
        }
        if (direita < esquerda)
            return new Sprite[0];
        for (int indice = 0; indice < quantidade; indice++)
            resultado[indice] = Sprite.Create(textura, new Rect(indice * largura + esquerda, baixo, direita - esquerda + 1, cima - baixo + 1), new Vector2(.5f, .5f), 100f);
        cache[chave] = resultado;
        return resultado;
    }

    public static GameObject Criar(string caminho, Vector3 posicao, Vector2 tamanho, int quantidade = 6, bool loop = false, float velocidade = 10f)
    {
        Sprite[] sprites = Quadros(caminho, quantidade);
        if (sprites.Length == 0)
            return null;
        GameObject objeto = new GameObject(caminho);
        objeto.transform.position = posicao;
        SpriteRenderer renderer = objeto.AddComponent<SpriteRenderer>();
        renderer.sprite = sprites[0];
        renderer.sortingOrder = 20;
        if (tamanho.x <= 0)
            tamanho.x = tamanho.y * sprites[0].bounds.size.x / sprites[0].bounds.size.y;
        objeto.transform.localScale = new Vector3(tamanho.x / sprites[0].bounds.size.x, tamanho.y / sprites[0].bounds.size.y, 1);
        QuestMagicArt animacao = objeto.AddComponent<QuestMagicArt>();
        animacao.quadros = sprites;
        animacao.visual = renderer;
        animacao.inicio = Time.time;
        animacao.repetir = loop;
        animacao.fps = velocidade;
        if (!loop)
            Destroy(objeto, quantidade / velocidade);
        return objeto;
    }

    void Update()
    {
        if (quadros == null || quadros.Length == 0)
            return;
        int indice = Mathf.FloorToInt((Time.time - inicio) * fps);
        visual.sprite = quadros[repetir ? indice % quadros.Length : Mathf.Min(indice, quadros.Length - 1)];
    }
}
