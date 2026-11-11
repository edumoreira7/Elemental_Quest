using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestElementTrainingBuilder
{
    private const string Pasta = "Assets/ElementalQuest/TutorialEtapa2";
    private static Transform grupo;
    private static Sprite grama;
    private static Material material;

    public static void ConstruirEValidar()
    {
        Construir();
        QuestTrainingStageTest.Begin();
    }

    public static void AtualizarGuiasEValidar()
    {
        Scene cena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        QuestElementTraining treino = UnityEngine.Object.FindAnyObjectByType<QuestElementTraining>();
        grupo = treino.transform;
        GuiasTerra(treino.estacoes);
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        AssetDatabase.SaveAssets();
        QuestTrainingStageTest.Begin();
    }

    [MenuItem("Elemental Quest/Tutorial/Conectar treino elemental seguro")]
    public static void Construir()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Saia do Play antes de editar a cena.");
        for (int indice = 0; indice < SceneManager.sceneCount; indice++)
        {
            if (SceneManager.GetSceneAt(indice).isDirty)
                throw new InvalidOperationException("Salve as cenas abertas antes de continuar.");
        }
        Scene cena = EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity", OpenSceneMode.Single);
        if (GameObject.Find("Treino elemental - etapa 2") != null)
            throw new InvalidOperationException("Treino ja existe; nenhuma reconstrucao foi feita.");
        Directory.CreateDirectory(Pasta);
        AssetDatabase.Refresh();
        Tilemap mapa = UnityEngine.Object.FindAnyObjectByType<Tilemap>();
        TileBase topo = mapa.GetTile(new Vector3Int(0, -4, 0));
        TileBase terra = mapa.GetTile(new Vector3Int(0, -5, 0));
        grama = ((Tile)topo).sprite;
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/Arte/SpritesSemLuz.mat");
        for (int coluna = 60; coluna < 224; coluna++)
        {
            for (int linha = -7; linha <= -4; linha++)
                mapa.SetTile(new Vector3Int(coluna, linha, 0), linha == -4 ? topo : terra);
        }
        GameObject piso = GameObject.Find("Piso seguro sem abismos");
        piso.transform.position = new Vector3(106, -5, 0);
        piso.GetComponent<BoxCollider2D>().size = new Vector2(236, 4);
        GameObject limite = GameObject.Find("Reserva para a proxima area");
        limite.name = "Limite seguro - fim do treino";
        limite.transform.position = new Vector3(223, 1, 0);
        UnityEngine.Object.FindAnyObjectByType<QuestCamera>().maximoX = 216;
        UnityEngine.Object.FindAnyObjectByType<QuestSession>().avisarColetaCompleta = false;
        grupo = new GameObject("Treino elemental - etapa 2").transform;
        QuestElementTraining treino = grupo.gameObject.AddComponent<QuestElementTraining>();
        treino.tutorial = UnityEngine.Object.FindAnyObjectByType<QuestTutorialArea>();
        treino.jogador = treino.tutorial.jogador;
        List<QuestElementTraining.Estacao> estacoes = new List<QuestElementTraining.Estacao>();
        estacoes.Add(Estacao("Troca", QuestElementTraining.Objetivo.Selecao, 49, 56, "1-4: troque o elemento", new Vector2(53, -2.3f), 0));
        Plataforma("Terra - marco alto", 65, 3, -.6f);
        estacoes.Add(Estacao("Terra", QuestElementTraining.Objetivo.Terra, 56, 74, "1 + J: crie um degrau", new Vector2(66.5f, .05f), 0));
        Plataforma("Gelo - degrau de acesso", 77, 1.5f, -2.3f);
        Plataforma("Gelo - margem esquerda", 79, 3, -1.4f);
        Plataforma("Gelo - margem direita", 84.4f, 3, -1.4f);
        QuestElementTarget agua = Agua(82, 2.4f, -1.4f, "Gelo - poco de pratica seguro");
        estacoes.Add(Estacao("Gelo", QuestElementTraining.Objetivo.Gelo, 74, 96, "4 + J: congele a agua", new Vector2(86, -.75f), 3, agua));
        Plataforma("Ar - degrau de partida", 99, 3, -2.3f);
        Plataforma("Ar - salto duplo", 104, 3, .2f);
        estacoes.Add(Estacao("Ar salto", QuestElementTraining.Objetivo.ArDuplo, 96, 106.5f, "2 + Espaco no ar", new Vector2(105.3f, .85f), 1));
        Plataforma("Ar - pouso do planar", 110.5f, 3, -2.2f);
        estacoes.Add(Estacao("Ar planar", QuestElementTraining.Objetivo.ArPlanar, 106.5f, 123, "Segure J ao cair", new Vector2(112, -1.55f), 1));
        QuestElementTarget raizes = Raizes(129, 2.2f, "Fogo - raizes de treino");
        estacoes.Add(Estacao("Fogo", QuestElementTraining.Objetivo.Fogo, 123, 143, "3 + J: queime as raizes", new Vector2(132, -2.3f), 2, raizes));
        Plataforma("Pratica Terra - outro degrau", 150, 2, -1f);
        estacoes.Add(Estacao("Terra 2", QuestElementTraining.Objetivo.Terra, 143, 154, "Terra: alcance o marco", new Vector2(151, -.35f), 0));
        Plataforma("Pratica Gelo - margem", 154.5f, 2, -2.1f);
        Plataforma("Pratica Gelo - chegada", 158.3f, 2.5f, -2.1f);
        QuestElementTarget agua2 = Agua(156.5f, 1.8f, -2.1f, "Pratica Gelo - poco menor");
        estacoes.Add(Estacao("Gelo 2", QuestElementTraining.Objetivo.Gelo, 154, 164, "Gelo: crie a passagem", new Vector2(159.5f, -1.45f), 3, agua2));
        Plataforma("Pratica Ar - aproximacao segura", 164.5f, 1.5f, -2.5f);
        Plataforma("Pratica Ar - alcance alto", 167, 3, -.5f);
        estacoes.Add(Estacao("Ar 2", QuestElementTraining.Objetivo.ArDuplo, 164, 172, "Ar: salte ate o marco", new Vector2(168.5f, .15f), 1));
        QuestElementTarget raizes2 = Raizes(176, 2.2f, "Pratica Fogo - nova passagem");
        estacoes.Add(Estacao("Fogo 2", QuestElementTraining.Objetivo.Fogo, 172, 182, "Fogo: abra a passagem", new Vector2(179, -2.3f), 2, raizes2));
        Plataforma("Combinacao - acesso com Terra", 186, 3, -.6f);
        QuestElementTarget aguaFinal = Agua(189, 2.4f, -.6f, "Combinacao - passagem de Gelo");
        Plataforma("Combinacao - margem de Gelo", 191.4f, 3, -.6f);
        Plataforma("Combinacao - alcance com Ar", 197, 3, 1.6f);
        QuestElementTarget raizesFinal = Raizes(205, 2.2f, "Combinacao - saida com Fogo");
        estacoes.Add(Estacao("Combinacao", QuestElementTraining.Objetivo.Combinacao, 182, 222.7f, "Combine os elementos", new Vector2(208, -2.3f), 0, aguaFinal, raizesFinal));
        treino.estacoes = estacoes.ToArray();
        GuiasTerra(treino.estacoes);
        List<QuestTutorialArea.Faixa> faixas = treino.tutorial.faixas.ToList();
        QuestTutorialArea.Faixa ultima = faixas[faixas.Count - 1];
        ultima.fim = 49;
        faixas[faixas.Count - 1] = ultima;
        foreach (QuestElementTraining.Estacao estacao in estacoes)
        {
            faixas.Add(new QuestTutorialArea.Faixa { inicio = estacao.inicio, fim = estacao.fim, mensagem = estacao.dica });
            Seta(estacao.inicio + .7f, -1.6f);
        }
        treino.tutorial.faixas = faixas.ToArray();
        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        AssetDatabase.SaveAssets();
        Debug.Log("EQ_TRAINING_BUILD_OK: introducoes, expansoes e combinacao conectadas ao tutorial existente.");
    }

    private static void GuiasTerra(QuestElementTraining.Estacao[] estacoes)
    {
        estacoes[1].pontoTerra = 62.2f;
        estacoes[6].pontoTerra = 147.2f;
        estacoes[10].pontoTerra = 183.2f;
        foreach (QuestElementTraining.Estacao estacao in estacoes)
        {
            if (estacao.pontoTerra <= 0 || GameObject.Find("Ponto de Terra " + estacao.nome) != null)
                continue;
            Vector3 posicao = new Vector3(estacao.pontoTerra, -2.7f, 0);
            GameObject moldura = Desenho("Ponto de Terra " + estacao.nome, "Tutorial/Interface/12-Elemento-Selecionado", posicao, new Vector2(.8f, .5f));
            moldura.GetComponent<SpriteRenderer>().sortingOrder = 11;
            Desenho("Simbolo Terra " + estacao.nome, "Tutorial/Interface/01-Icone-Terra", posicao, new Vector2(.35f, .35f));
        }
    }

    private static QuestElementTraining.Estacao Estacao(string nome, QuestElementTraining.Objetivo tipo, float inicio, float fim, string dica, Vector2 chegada, int elemento, params QuestElementTarget[] alvos)
    {
        string[] icones = { "01-Icone-Terra", "02-Icone-Ar", "03-Icone-Fogo", "04-Icone-Gelo" };
        string icone = tipo == QuestElementTraining.Objetivo.Selecao || tipo == QuestElementTraining.Objetivo.Combinacao ? "12-Elemento-Selecionado" : icones[elemento];
        GameObject marco = Desenho("Marco " + nome, "Tutorial/Interface/" + icone, chegada + Vector2.up * .7f, new Vector2(.55f, .55f));
        GameObject objetivo = new GameObject("Chegada " + nome);
        objetivo.transform.SetParent(grupo, false);
        objetivo.transform.position = chegada;
        BoxCollider2D colisor = objetivo.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;
        colisor.size = new Vector2(1.3f, 1.2f);
        return new QuestElementTraining.Estacao { nome = nome, tipo = tipo, inicio = inicio, fim = fim, dica = dica, chegada = colisor, marco = marco.GetComponent<SpriteRenderer>(), alvos = alvos };
    }

    private static QuestElementTarget Agua(float inicio, float largura, float topo, string nome)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = new Vector2(inicio + largura * .5f, topo - .15f);
        BoxCollider2D colisor = objeto.AddComponent<BoxCollider2D>();
        colisor.isTrigger = true;
        colisor.size = new Vector2(largura, .3f);
        QuestElementTarget alvo = objeto.AddComponent<QuestElementTarget>();
        alvo.tipo = QuestElementTarget.Tipo.Agua;
        GameObject visual = Desenho("Poca de treino", "Tutorial/Treino/07-Poca-Congelamento", objeto.transform.position, new Vector2(largura, .5f));
        visual.transform.SetParent(objeto.transform, true);
        return alvo;
    }

    private static QuestElementTarget Raizes(float posicaoX, float altura, string nome)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = new Vector2(posicaoX, -3 + altura * .5f);
        objeto.layer = 3;
        objeto.AddComponent<BoxCollider2D>().size = new Vector2(1, altura);
        QuestElementTarget alvo = objeto.AddComponent<QuestElementTarget>();
        alvo.tipo = QuestElementTarget.Tipo.Raizes;
        GameObject visual = Desenho("Raizes", "Tutorial/Treino/03-Raizes-Bloqueio", objeto.transform.position, new Vector2(1.1f, altura));
        visual.transform.SetParent(objeto.transform, true);
        return alvo;
    }

    private static GameObject Desenho(string nome, string caminho, Vector3 posicao, Vector2 tamanho)
    {
        string arquivo = Pasta + "/" + caminho.Replace('/', '_') + ".asset";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(arquivo);
        if (sprite == null)
        {
            Sprite[] quadros = QuestMagicArt.Quadros(caminho, 1);
            if (quadros.Length == 0)
                throw new InvalidOperationException("Sprite ausente: " + caminho);
            sprite = UnityEngine.Object.Instantiate(quadros[0]);
            AssetDatabase.CreateAsset(sprite, arquivo);
        }
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = posicao;
        SpriteRenderer renderer = objeto.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 12;
        objeto.transform.localScale = new Vector3(tamanho.x / sprite.bounds.size.x, tamanho.y / sprite.bounds.size.y, 1);
        return objeto;
    }

    private static void Plataforma(string nome, float inicio, float largura, float topo)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = new Vector2(inicio + largura * .5f, topo - .2f);
        objeto.layer = 3;
        objeto.AddComponent<BoxCollider2D>().size = new Vector2(largura, .4f);
        int pecas = Mathf.CeilToInt(largura);
        for (int indice = 0; indice < pecas; indice++)
        {
            GameObject desenho = new GameObject("Grama existente");
            desenho.transform.SetParent(objeto.transform, false);
            desenho.transform.localPosition = new Vector3(-largura * .5f + (indice + .5f) * largura / pecas, 0, 0);
            SpriteRenderer renderer = desenho.AddComponent<SpriteRenderer>();
            renderer.sprite = grama;
            renderer.sharedMaterial = material;
            desenho.transform.localScale = new Vector3(largura / pecas / grama.bounds.size.x, .4f / grama.bounds.size.y, 1);
        }
    }

    private static void Seta(float posicaoX, float altura)
    {
        GameObject objeto = new GameObject("Seta do treino");
        objeto.transform.SetParent(grupo, false);
        objeto.transform.position = new Vector2(posicaoX, altura);
        TextMesh texto = objeto.AddComponent<TextMesh>();
        texto.text = ">";
        texto.fontSize = 48;
        texto.characterSize = .1f;
        texto.anchor = TextAnchor.MiddleCenter;
        texto.color = new Color(1, .8f, .2f);
        objeto.GetComponent<MeshRenderer>().sortingOrder = 15;
    }
}
