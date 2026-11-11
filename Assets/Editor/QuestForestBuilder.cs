using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestForestBuilder
{
    static Transform grupo;
    static Material material;
    static Tile[] tiles;
    const string Principal="Assets/Scenes/FasePrincipal.unity", Arena="Assets/Scenes/ArenaFloresta.unity";
    public static void ConstruirEValidar() {Construir(); QuestForestStageTest.Begin();}
    public static void AcabamentoEValidar()
    {
        var cena=EditorSceneManager.OpenScene(Principal,OpenSceneMode.Single);
        foreach(var grid in UnityEngine.Object.FindObjectsByType<Grid>()) if(Mathf.Abs(grid.transform.position.x-31)<.01f&&Mathf.Abs(grid.transform.position.y+1.8f)<.01f)grid.transform.position+=Vector3.left;
        foreach(var cristal in UnityEngine.Object.FindObjectsByType<QuestCrystal>()) ConfigurarCristal(cristal.gameObject);
        UnirBordas();
        EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);AssetDatabase.SaveAssets();QuestForestStageTest.Begin();
    }
    public static void Construir()
    {
        if(Application.isPlaying || File.Exists(Principal) || File.Exists(Arena)) throw new InvalidOperationException("Cenas novas ja existem ou Play ativo; preservar.");
        for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Salve alteracoes abertas.");
        tiles=Enumerable.Range(0,9).Select(i=>AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_"+i.ToString("00")+".asset")).ToArray();
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/TerrenoContinuo/TerrenoSemLuz.mat");
        Scene cena=Base(Principal,"Floresta - Caminho das Ruinas",155);
        grupo=new GameObject("Percurso - floresta e ruinas").transform;
        Chao("Clareira inicial",-8,22,-3); Chao("Primeira subida",14,10,-2); Chao("Degrau da subida",12,2,-2.5f,1);
        Chao("Vale de exploracao e combate",24,20,-3); Plataforma(26,3,-1.7f); Plataforma(30,3,-.8f);
        Chao("Descanso e encontro a distancia",44,16,-3); Plataforma(49,2,-2);
        Chao("Trecho Terra",60,14,-3); Plataforma(68,4,-.6f);
        Chao("Recuperacao da agua",74,10,-4.2f); Chao("Margem de entrada",74,2,-3,1); Chao("Margem de saida",78.4f,5.6f,-3,1); Agua(76,2.4f);
        Chao("Bosque de plataformas",84,14,-3); Plataforma(84,3,-2.3f); Plataforma(89,3,.2f); Plataforma(95,2,-2.2f);
        Chao("Raizes e descanso",98,10,-3); Raizes(100);
        Chao("Subida combinada",108,6,-2); Plataforma(106,2,-2.5f);
        Chao("Passagem de raizes",114,7,-3); Raizes(117);
        Chao("Recuperacao da segunda agua",121,3,-4.2f); Agua(121,2.4f);
        Chao("Margem combinada",123.4f,9.6f,-3); Plataforma(125,2,-2);
        Chao("Territorio do Guardiao",133,15,-3);
        Chao("Preparacao nas ruinas",148,26,-3);
        Parede(-7); Parede(170);
        var jogador=UnityEngine.Object.FindAnyObjectByType<PatoController>().transform;
        var fase=new GameObject("Ritmo dos encontros e retomada").AddComponent<QuestForestStage>(); fase.jogador=jogador;
        fase.encontros=new[] {
            Inimigo("Corpo a corpo - clareira", "TutorialEtapa3/GuaxinimCorpoACorpo",38,-3,36,40,33,44),
            Inimigo("Atirador - abrigo", "TutorialEtapa4/AtiradorTutorial",56,-3,55.5f,56.5f,49,60),
            Inimigo("Corpo a corpo - subida", "TutorialEtapa3/GuaxinimCorpoACorpo",111,-2,110,113,108,115),
            Inimigo("Atirador - combinacao", "TutorialEtapa4/AtiradorTutorial",132,-3,131.5f,132.5f,127,136),
            Inimigo("Guardiao das ruinas", "TutorialFinal/GuardiaoTutorial",141,-3,138,144,137,148)
        };
        Checkpoint(46); Checkpoint(105); Checkpoint(151); Vida(47.5f); Vida(106); Vida(153);
        foreach(float x in new[]{3f,7,11,17,21,25,28,32,42,45,61,65,69,71,75,79,82,85,90,96,103,107,110,119,124,128,135,146,150,156}) {
            float y=x>=14 && x<24 ? -1.15f : x==28 ? -.85f : x==32 ? .05f : x==69 || x==71 ? .25f : x==90 ? 1.05f : x>=108 && x<114 ? -1.15f : -2.15f;
            Cristal(x,y);
        }
        foreach(float x in new[]{10f,25,43,60,73,83,97,104,115,124,136,149,158}) Seta(x);
        Simbolo("01-Icone-Terra",65.2f,-2.6f); Simbolo("04-Icone-Gelo",75,-2); Simbolo("02-Icone-Ar",85.5f,-1.2f); Simbolo("03-Icone-Fogo",98.5f,-1.8f);
        var tutorial=new GameObject("Orientacao contextual da floresta").AddComponent<QuestTutorialArea>(); tutorial.jogador=jogador;
        tutorial.fimDaArea=162;
        tutorial.faixas=new[] {
            F(0,12,"Siga os cristais"),F(12,24,"Suba e explore"),F(24,34,"Ha cristais no alto"),F(34,44,"Observe e contra-ataque"),
            F(44,49,"Ative a bandeira"),F(49,60,"Use o abrigo"),F(60,74,"Terra: alcance o alto"),F(74,84,"Gelo: congele a agua"),
            F(84,98,"Ar: explore as copas"),F(98,104,"Fogo: abra as raizes"),F(104,108,"Recupere-se na bandeira"),
            F(108,137,"Combine pulo e magia"),F(137,148,"Recue e contra-ataque"),F(148,174,"Bandeira antes da arena")
        };
        var portal=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/TutorialFinal/PortalTutorial.prefab"));
        portal.name="Portal das ruinas - entrada da arena"; portal.transform.position=new Vector3(162,-2,0);
        UnityEngine.Object.DestroyImmediate(portal.GetComponent<QuestPortal>());
        portal.AddComponent<QuestForestGate>().fase=fase;
        var sessao=UnityEngine.Object.FindAnyObjectByType<QuestSession>(); sessao.totalItens=UnityEngine.Object.FindObjectsByType<QuestCrystal>().Length;
        EditorSceneManager.MarkSceneDirty(cena); EditorSceneManager.SaveScene(cena);
        var arena=Base(Arena,"Ruinas - Entrada da arena",8);
        grupo=new GameObject("Arena reservada - sem boss").transform; Chao("Piso da arena",-8,32,-3); Parede(-7); Parede(20);
        new GameObject("Aviso da arena reservada").AddComponent<QuestArenaArrival>();
        EditorSceneManager.MarkSceneDirty(arena); EditorSceneManager.SaveScene(arena); AssetDatabase.SaveAssets();
        foreach(string caminho in new[]{Principal,Arena}) {
            if(!EditorBuildSettings.scenes.Any(s=>s.path==caminho)) EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(caminho,true)}).ToArray();
        }
        string configuracao="ProjectSettings/EditorBuildSettings.asset";
        string texto=File.ReadAllText(configuracao);
        foreach(string caminho in new[]{Principal,Arena}) if(!texto.Contains("path: "+caminho))
            texto=texto.Replace("  m_configObjects:","  - enabled: 1\n    path: "+caminho+"\n    guid: "+AssetDatabase.AssetPathToGUID(caminho)+"\n  m_configObjects:");
        File.WriteAllText(configuracao,texto);
        Debug.Log("EQ_FOREST_BUILD_OK: floresta, cinco encontros espacados, desafios dos quatro elementos e arena separada sem boss.");
    }
    static Scene Base(string caminho,string nome,float limite)
    {
        var cena=EditorSceneManager.OpenScene("Assets/Scenes/Tutorial.unity",OpenSceneMode.Single);
        EditorSceneManager.SaveScene(cena,caminho);
        foreach(var raiz in cena.GetRootGameObjects()) {
            bool manter=raiz.GetComponent<PatoController>()!=null || raiz.GetComponent<QuestSession>()!=null || raiz.GetComponent<Camera>()!=null || raiz.GetComponent<QuestBackground>()!=null || raiz.GetComponents<Component>().Any(c=>c!=null && c.GetType().Name=="Light2D");
            if(!manter) UnityEngine.Object.DestroyImmediate(raiz);
        }
        var jogador=UnityEngine.Object.FindAnyObjectByType<PatoController>(); jogador.transform.position=new Vector3(0,-2.35f,0); jogador.GetComponent<QuestPlayer>().pontoSeguro=jogador.transform.position;
        var camera=UnityEngine.Object.FindAnyObjectByType<QuestCamera>(); camera.minimoX=0; camera.maximoX=limite; camera.transform.position=new Vector3(2,.3f,-10);
        var sessao=UnityEngine.Object.FindAnyObjectByType<QuestSession>(); sessao.fase=1; sessao.nomeFase=nome; sessao.totalItens=0; sessao.avisarColetaCompleta=false; sessao.tutorialFinal=null;
        return cena;
    }
    static void Chao(string nome,float x,float largura,float topo,int profundidade=4)
    {
        int n=Mathf.CeilToInt(largura);
        var grid=new GameObject(nome); grid.transform.SetParent(grupo); grid.transform.position=new Vector3(x,topo-1,0); grid.transform.localScale=new Vector3(largura/n,1,1); grid.layer=3; grid.AddComponent<Grid>();
        var obj=new GameObject("Tilemap existente"); obj.transform.SetParent(grid.transform,false);
        var mapa=obj.AddComponent<Tilemap>(); mapa.tileAnchor=new Vector3(.5f,.5625f,0);
        obj.AddComponent<TilemapRenderer>().sharedMaterial=material;
        for(int coluna=0;coluna<n;coluna++) for(int linha=0;linha<profundidade;linha++) {
            int a=coluna==0 ? 0 : coluna==n-1 ? 2 : 1; int b=linha==0 ? 0 : linha==profundidade-1 ? 2 : 1;
            mapa.SetTile(new Vector3Int(coluna,-linha,0),tiles[b*3+a]);
        }
        var caixa=grid.AddComponent<BoxCollider2D>(); caixa.offset=new Vector2(n/2f,1-profundidade/2f); caixa.size=new Vector2(n,profundidade);
    }
    static void UnirBordas()
    {
        if(tiles==null)tiles=Enumerable.Range(0,9).Select(i=>AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_"+i.ToString("00")+".asset")).ToArray();
        Physics2D.SyncTransforms();
        var mapas=UnityEngine.Object.FindObjectsByType<Tilemap>();
        foreach(var mapa in mapas) {
            var caixa=mapa.GetComponentInParent<BoxCollider2D>();if(caixa==null)continue;
            var bounds=caixa.bounds;
            bool esquerda=mapas.Any(m=>m!=mapa&&m.GetComponentInParent<BoxCollider2D>()!=null&&Mathf.Abs(m.GetComponentInParent<BoxCollider2D>().bounds.max.x-bounds.min.x)<.02f&&Mathf.Abs(m.GetComponentInParent<BoxCollider2D>().bounds.max.y-bounds.max.y)<.02f);
            bool direita=mapas.Any(m=>m!=mapa&&m.GetComponentInParent<BoxCollider2D>()!=null&&Mathf.Abs(m.GetComponentInParent<BoxCollider2D>().bounds.min.x-bounds.max.x)<.02f&&Mathf.Abs(m.GetComponentInParent<BoxCollider2D>().bounds.max.y-bounds.max.y)<.02f);
            var cells=mapa.cellBounds;
            for(int y=cells.yMin;y<cells.yMax;y++) {
                int linha=y==cells.yMax-1?0:y==cells.yMin?2:1;
                if(esquerda)mapa.SetTile(new Vector3Int(cells.xMin,y,0),tiles[linha*3+1]);
                if(direita)mapa.SetTile(new Vector3Int(cells.xMax-1,y,0),tiles[linha*3+1]);
            }
        }
    }
    static void Plataforma(float x,float largura,float topo) {Chao("Plataforma de exploracao",x,largura,topo,1);}
    static void Parede(float x) {var o=new GameObject("Limite do percurso"); o.transform.SetParent(grupo); o.transform.position=new Vector3(x,1,0);o.layer=3;o.AddComponent<BoxCollider2D>().size=new Vector2(1,12);}
    static QuestForestStage.Encontro Inimigo(string nome,string prefab,float x,float y,float esquerda,float direita,float inicio,float fim)
    {
        var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/"+prefab+".prefab"));
        obj.name=nome; obj.transform.SetParent(grupo); obj.transform.position=new Vector3(x,y+.02f,0);
        var inimigo=obj.GetComponent<QuestEnemy>(); inimigo.limiteEsquerdo=esquerda;inimigo.limiteDireito=direita; obj.SetActive(false);
        return new QuestForestStage.Encontro{inimigo=inimigo,inicio=inicio,fim=fim};
    }
    static GameObject Objeto(string nome,float x,float y,Vector2 tamanho)
    {
        var o=new GameObject(nome);o.transform.SetParent(grupo);o.transform.position=new Vector3(x,y,0);
        o.AddComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal0.png");
        var c=o.AddComponent<BoxCollider2D>();c.isTrigger=true;c.size=tamanho;return o;
    }
    static void Checkpoint(float x) {Objeto("Bandeira - descanso seguro",x,-2.2f,new Vector2(1,1.6f)).AddComponent<QuestCheckpoint>().destino=new Vector3(x,-2.35f,0);}
    static void Vida(float x) {Objeto("Vida na preparacao",x,-2.2f,new Vector2(.8f,1.2f)).AddComponent<QuestLifePickup>();}
    static void Cristal(float x,float y) {var o=Objeto("Cristal - trilha e exploracao",x,y,Vector2.one*.5f);o.AddComponent<QuestCrystal>();ConfigurarCristal(o);}
    static void ConfigurarCristal(GameObject o) {o.transform.localScale=Vector3.one;var r=o.GetComponent<SpriteRenderer>();r.sortingOrder=5;var animacao=o.GetComponent<QuestAnimation>();if(animacao==null)animacao=o.AddComponent<QuestAnimation>();animacao.quadros=Enumerable.Range(0,6).Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal"+i+".png")).ToArray();}
    static void Agua(float x,float largura) {var o=Objeto("Agua congelavel",x+largura/2,-3.15f,new Vector2(largura,.3f));o.AddComponent<QuestElementTarget>().tipo=QuestElementTarget.Tipo.Agua;Arte(o,"07-Poca-Congelamento",new Vector2(largura,.5f));}
    static void Raizes(float x) {var o=Objeto("Raizes - Fogo",x,-1.9f,new Vector2(1,2.2f));o.layer=3;o.GetComponent<BoxCollider2D>().isTrigger=false;o.AddComponent<QuestElementTarget>().tipo=QuestElementTarget.Tipo.Raizes;Arte(o,"03-Raizes-Bloqueio",new Vector2(1.1f,2.2f));}
    static void Arte(GameObject o,string arquivo,Vector2 tamanho) {var r=o.GetComponent<SpriteRenderer>();r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/TutorialEtapa2/Tutorial_Treino_"+arquivo+".asset");r.sharedMaterial=material;r.sortingOrder=4;o.transform.localScale=Vector3.one;var filho=new GameObject("Arte de interacao");filho.transform.SetParent(o.transform,false);var desenho=filho.AddComponent<SpriteRenderer>();desenho.sprite=r.sprite;desenho.sharedMaterial=material;desenho.sortingOrder=4;filho.transform.localScale=new Vector3(tamanho.x/r.sprite.bounds.size.x,tamanho.y/r.sprite.bounds.size.y,1);r.enabled=false;}
    static void Simbolo(string arquivo,float x,float y) {var o=new GameObject("Pista do elemento");o.transform.SetParent(grupo);o.transform.position=new Vector3(x,y,0);var r=o.AddComponent<SpriteRenderer>();r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/TutorialEtapa2/Tutorial_Interface_"+arquivo+".asset");r.sharedMaterial=material;r.sortingOrder=8;o.transform.localScale=Vector3.one*(.45f/r.sprite.bounds.size.y);}
    static void Seta(float x) {var o=new GameObject("Rota principal");o.transform.SetParent(grupo);o.transform.position=new Vector3(x,-1.2f,0);var t=o.AddComponent<TextMesh>();t.text=">";t.fontSize=48;t.characterSize=.1f;t.anchor=TextAnchor.MiddleCenter;t.color=Color.yellow;o.GetComponent<MeshRenderer>().sortingOrder=12;}
    static QuestTutorialArea.Faixa F(float a,float b,string texto) {return new QuestTutorialArea.Faixa{inicio=a,fim=b,mensagem=texto};}
}
