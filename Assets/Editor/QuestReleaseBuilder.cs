using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestReleaseBuilder
{
    static Font fonte;static Transform canvas;
    static Color creme=new Color(.95f,.94f,.82f),verde=new Color(.64f,.85f,.36f),escuro=new Color(.035f,.085f,.09f);
    static string[] Cenas => EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
    public static void CriarMenuEValidar()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Saia do Play.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Salve alteracoes abertas.");
        fonte=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var cena=EditorSceneManager.OpenScene("Assets/Scenes/FasePrincipal.unity",OpenSceneMode.Single);EditorSceneManager.SaveScene(cena,"Assets/Scenes/Inicio.unity");
        foreach(var raiz in cena.GetRootGameObjects())if(raiz.GetComponent<Camera>()==null&&raiz.GetComponent<QuestBackground>()==null)UnityEngine.Object.DestroyImmediate(raiz);
        var camera=Camera.main;if(camera.GetComponent<QuestCamera>()!=null)UnityEngine.Object.DestroyImmediate(camera.GetComponent<QuestCamera>());camera.transform.position=new Vector3(0,.5f,-10);
        foreach(var fundo in UnityEngine.Object.FindObjectsByType<QuestBackground>())fundo.alvo=null;
        Decoracao();
        var obj=new GameObject("Menu principal",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=obj.transform;obj.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var escala=obj.GetComponent<CanvasScaler>();escala.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;escala.referenceResolution=new Vector2(1280,720);escala.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var menu=obj.AddComponent<QuestMainMenu>();
        var painel=Caixa("Painel floresta",canvas,0,0,665,720,new Color(.025f,.065f,.075f,.96f));painel.anchorMin=new Vector2(0,0);painel.anchorMax=new Vector2(0,1);painel.offsetMin=new Vector2(0,0);painel.offsetMax=new Vector2(665,0);
        Texto("UMA AVENTURA ELEMENTAL",canvas,80,88,510,30,14,verde);
        var titulo=Texto("ELEMENTAL\nQUEST",canvas,74,138,560,150,66,creme);titulo.fontStyle=FontStyle.Bold;titulo.lineSpacing=.9f;
        Texto("Entre a floresta e as ruínas.",canvas,80,307,500,30,22,new Color(.7f,.78f,.72f));
        string[] icones={"01-Icone-Terra","02-Icone-Ar","03-Icone-Fogo","04-Icone-Gelo"};
        for(int i=0;i<4;i++){var r=Caixa("Elemento",canvas,80+i*55,355,34,34,Color.white);var image=r.GetComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/TutorialEtapa2/Tutorial_Interface_"+icones[i]+".asset");image.preserveAspect=true;}
        menu.jogar=Botao("JOGAR AVENTURA",canvas,80,423,400,58,verde,escuro);UnityEventTools.AddPersistentListener(menu.jogar.onClick,menu.Jogar);
        var tutorial=Botao("TUTORIAL",canvas,80,493,400,48,new Color(.17f,.28f,.25f),creme);UnityEventTools.AddPersistentListener(tutorial.onClick,menu.Tutorial);
        var controles=Botao("CONTROLES",canvas,80,554,192,40,new Color(.1f,.18f,.17f),creme);UnityEventTools.AddPersistentListener(controles.onClick,menu.AbrirControles);
        var sair=Botao("SAIR",canvas,288,554,192,40,new Color(.1f,.18f,.17f),creme);UnityEventTools.AddPersistentListener(sair.onClick,menu.Sair);
        Texto("ELEMENTAL QUEST  ·  PROJETO ACADÊMICO",canvas,80,658,550,22,12,new Color(.48f,.6f,.53f));
        var selo=Caixa("Legenda cenario",canvas,0,0,370,40,new Color(.025f,.065f,.075f,.85f));selo.anchorMin=selo.anchorMax=new Vector2(1,1);selo.anchoredPosition=new Vector2(-40,-90);selo.pivot=new Vector2(1,1);
        Texto("CAMINHO DAS RUÍNAS",selo,20,8,330,24,17,creme).alignment=TextAnchor.MiddleCenter;
        var frase=Caixa("Convite",canvas,0,0,370,64,new Color(.025f,.065f,.075f,.85f));frase.anchorMin=frase.anchorMax=new Vector2(1,0);frase.pivot=new Vector2(1,0);frase.anchoredPosition=new Vector2(-40,18);
        Texto("Quatro elementos.\nUm guardião à sua espera.",frase,22,8,330,52,20,creme);
        var overlay=Caixa("Controles - modal",canvas,0,0,1280,720,new Color(0,.025f,.03f,.88f));overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;overlay.offsetMin=overlay.offsetMax=Vector2.zero;
        menu.controles=overlay.gameObject;
        var card=Caixa("Cartao de controles",overlay,0,0,680,530,escuro);card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.pivot=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;
        Texto("COMO JOGAR",card,40,30,600,48,32,verde);
        string[] teclas={"A / D ou setas","Shift esquerdo","Espaço","1 · 2 · 3 · 4","J","Ar: Espaço no ar","Ar: segurar J","Esc"};
        string[] acoes={"Mover","Correr","Pular","Terra · Ar · Fogo · Gelo","Conjurar magia","Segundo salto","Planar durante a queda","Pausar / continuar"};
        for(int i=0;i<teclas.Length;i++){Texto(teclas[i],card,40,103+i*38,218,32,20,verde);Texto(acoes[i],card,270,103+i*38,370,32,21,creme);}
        menu.fecharControles=Botao("VOLTAR",card,40,442,600,52,verde,escuro);UnityEventTools.AddPersistentListener(menu.fecharControles.onClick,menu.FecharControles);overlay.gameObject.SetActive(false);
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);AssetDatabase.SaveAssets();
        EditorBuildSettings.scenes=Cenas.Select(c=>new EditorBuildSettingsScene(c,true)).ToArray();
        string settings=File.ReadAllText("ProjectSettings/EditorBuildSettings.asset");int a=settings.IndexOf("  m_Scenes:"),b=settings.IndexOf("  m_configObjects:");
        if(a<0||b<a)throw new InvalidOperationException("Configuracao de cenas inesperada.");
        string lista="  m_Scenes:\n"+string.Join("",Cenas.Select(c=>"  - enabled: 1\n    path: "+c+"\n    guid: "+AssetDatabase.AssetPathToGUID(c)+"\n"));File.WriteAllText("ProjectSettings/EditorBuildSettings.asset",settings.Substring(0,a)+lista+settings.Substring(b));
        PlanejarLimpeza();Debug.Log("EQ_RELEASE_MENU_OK");QuestReleaseFlowTest.Begin();
    }
    static void Decoracao()
    {
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/TerrenoContinuo/TerrenoSemLuz.mat");
        var grid=new GameObject("Cenario do menu",typeof(Grid));grid.transform.position=new Vector3(-12,-4,0);
        var obj=new GameObject("Chao decorativo",typeof(Tilemap),typeof(TilemapRenderer));obj.transform.SetParent(grid.transform,false);var mapa=obj.GetComponent<Tilemap>();mapa.tileAnchor=new Vector3(.5f,.5625f,0);obj.GetComponent<TilemapRenderer>().sharedMaterial=mat;
        for(int x=0;x<24;x++)for(int y=0;y<4;y++)mapa.SetTile(new Vector3Int(x,-y,0),AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_"+(y==0?1:4).ToString("00")+".asset"));
        var boss=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/GuardiaoRuinasVisual/Generated/GuardiaoRuinas_Visual.prefab"));boss.name="Ilustracao do Guardiao";boss.transform.position=new Vector3(5,-3,0);boss.transform.localScale=new Vector3(-1.3f,1.3f,1);boss.GetComponent<SpriteRenderer>().sortingOrder=6;
        var modelo=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/Prefabs/PatoMago.prefab");var pato=new GameObject("Ilustracao do Pato");pato.transform.position=new Vector3(2.4f,-2.4f,0);pato.transform.localScale=Vector3.one*.72f;var r=pato.AddComponent<SpriteRenderer>();r.sprite=modelo.GetComponent<SpriteRenderer>().sprite;r.sharedMaterial=modelo.GetComponent<SpriteRenderer>().sharedMaterial;r.sortingOrder=7;pato.AddComponent<Animator>().runtimeAnimatorController=modelo.GetComponent<Animator>().runtimeAnimatorController;
    }
    static RectTransform Caixa(string nome,Transform pai,float x,float y,float w,float h,Color cor){var o=new GameObject(nome,typeof(RectTransform),typeof(Image));o.transform.SetParent(pai,false);var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);o.GetComponent<Image>().color=cor;return r;}
    static Text Texto(string valor,Transform pai,float x,float y,float w,float h,int tamanho,Color cor){var o=new GameObject("Texto",typeof(RectTransform),typeof(Text));o.transform.SetParent(pai,false);var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);var t=o.GetComponent<Text>();t.font=fonte;t.text=valor;t.fontSize=tamanho;t.color=cor;t.raycastTarget=false;t.alignment=TextAnchor.UpperLeft;return t;}
    static Button Botao(string valor,Transform pai,float x,float y,float w,float h,Color fundo,Color letra){var r=Caixa(valor,pai,x,y,w,h,fundo);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();var cores=b.colors;cores.normalColor=Color.white;cores.highlightedColor=new Color(1.15f,1.15f,1.15f);cores.selectedColor=cores.highlightedColor;cores.pressedColor=new Color(.75f,.85f,.75f);b.colors=cores;var t=Texto(valor,r,8,0,w-16,h,20,letra);t.alignment=TextAnchor.MiddleCenter;t.fontStyle=FontStyle.Bold;return b;}
    public static void PlanejarLimpeza()
    {
        var manter=new HashSet<string>(AssetDatabase.GetDependencies(Cenas,true));
        foreach(var caminho in AssetDatabase.GetAllAssetPaths()) {
            if(caminho.StartsWith("Assets/Resources/")||(caminho.StartsWith("Assets/Settings/")&&(caminho.EndsWith(".asset")||caminho.EndsWith(".inputactions"))))manter.Add(caminho);
            if(caminho.StartsWith("Assets/Scripts/")&&caminho.EndsWith(".cs")&&!Path.GetFileNameWithoutExtension(caminho).EndsWith("Test")&&!caminho.EndsWith("QuestArenaArrival.cs"))manter.Add(caminho);
        }
        foreach(var teste in manter.Where(p=>p.StartsWith("Assets/Scripts/")&&p.EndsWith("Test.cs")).ToArray())manter.Remove(teste);
        foreach(var caminho in new[]{"Assets/Editor/QuestReleaseBuilder.cs","Assets/Tests/QuestReleaseFlowTest.cs","Assets/Editor/QuestMagicImporter.cs"})manter.Add(caminho);
        foreach(var caminho in manter.ToArray()){manter.Add(caminho+".meta");string dir=Path.GetDirectoryName(caminho).Replace('\\','/');while(dir.StartsWith("Assets/")){manter.Add(dir+".meta");dir=Path.GetDirectoryName(dir).Replace('\\','/');}}
        var remover=Directory.GetFiles("Assets","*",SearchOption.AllDirectories).Select(p=>p.Replace('\\','/')).Where(p=>!manter.Contains(p)).OrderBy(p=>p).ToArray();
        File.WriteAllLines("release-remove.txt",remover);File.WriteAllLines("release-keep.txt",manter.OrderBy(p=>p));Debug.Log("EQ_RELEASE_PLAN_OK: "+remover.Length+" arquivos de desenvolvimento ou sem dependencia de jogo para arquivar.");
    }
    public static void GerarPreviewWindows()
    {
        const string pasta="C:/Users/morei/Documents/Codex/EQ-final-preview-e55d26b-20261111";
        if(File.Exists(pasta+"/ElementalQuest.exe"))throw new InvalidOperationException("Preview existente; preservar.");
        Directory.CreateDirectory(pasta);
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.productName="Elemental Quest";
        var opcoes=new BuildPlayerOptions{scenes=Cenas,locationPathName=pasta+"/ElementalQuest.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None};
        var relatorio=BuildPipeline.BuildPlayer(opcoes);
        if(relatorio.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build nao concluido: "+relatorio.summary.result);
        Debug.Log("EQ_RELEASE_BUILD_OK: "+pasta+"; "+relatorio.summary.totalSize+" bytes.");EditorApplication.Exit(0);
    }
}
