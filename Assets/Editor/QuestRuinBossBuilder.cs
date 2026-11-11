using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class QuestRuinBossBuilder
{
    const string Pasta="Assets/ElementalQuest/GuardiaoRuinas";
    static Sprite pedra,cabeca,quadrado;
    static Material material;
    public static void ConstruirEValidar(){Construir();QuestRuinBossTest.Begin();}
    static void PrepararSprites()
    {
        var fonte=AssetDatabase.LoadAllAssetsAtPath("Assets/ElementalQuest/Recortes/Cenario.png").OfType<Sprite>().First(s=>s.name=="Cenario_27");
        pedra=SpriteNativo("PedraMusgoFull",fonte.texture,fonte.rect);
        var rect=fonte.rect;cabeca=SpriteNativo("CabecaFull",fonte.texture,new Rect(rect.x+rect.width*.25f,rect.y+rect.height*.35f,rect.width*.5f,rect.height*.6f));
        var textura=AssetDatabase.LoadAssetAtPath<Texture2D>(Pasta+"/TexturaAviso.asset");
        if(textura==null){textura=new Texture2D(2,2,TextureFormat.RGBA32,false);textura.name="Sinal de area";textura.filterMode=FilterMode.Point;textura.SetPixels(new[]{Color.white,Color.white,Color.white,Color.white});textura.Apply();AssetDatabase.CreateAsset(textura,Pasta+"/TexturaAviso.asset");}
        quadrado=SpriteNativo("MarcadorArena",textura,new Rect(0,0,2,2));
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/ElementalQuest/TerrenoContinuo/TerrenoSemLuz.mat");
    }
    static Sprite SpriteNativo(string nome,Texture2D textura,Rect rect)
    {
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Pasta+"/"+nome+".asset");if(sprite!=null)return sprite;
        sprite=Sprite.Create(textura,rect,new Vector2(.5f,.5f),rect.width,0,SpriteMeshType.FullRect);sprite.name=nome;AssetDatabase.CreateAsset(sprite,Pasta+"/"+nome+".asset");return sprite;
    }
    static Sprite CristalNucleo(){var s=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ElementalQuest/Arte/Cristal0.png");return SpriteNativo("CristalNucleo",s.texture,s.rect);}
    static void Direcao(QuestRuinBoss boss)
    {
        if(boss.sentido!=null)return;
        var seta=new GameObject("Direcao anunciada");seta.transform.SetParent(boss.transform,false);boss.sentido=seta.AddComponent<TextMesh>();boss.sentido.fontSize=64;boss.sentido.characterSize=.13f;boss.sentido.anchor=TextAnchor.MiddleCenter;boss.sentido.color=new Color(1,.75f,.2f);seta.GetComponent<MeshRenderer>().sortingOrder=11;seta.SetActive(false);
    }
    public static void AjustarSinaisEValidar()
    {
        PrepararSprites();var cena=EditorSceneManager.OpenScene("Assets/Scenes/ArenaFloresta.unity",OpenSceneMode.Single);var boss=UnityEngine.Object.FindAnyObjectByType<QuestRuinBoss>();
        var antigaCabeca=AssetDatabase.LoadAssetAtPath<Sprite>(Pasta+"/CabecaPedra.asset");
        foreach(var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>()) {
            if(r.sprite!=null&&r.sprite.name=="Cenario_27")r.sprite=pedra;
            if(r.sprite==antigaCabeca)r.sprite=cabeca;
        }
        boss.nucleo.sprite=CristalNucleo();boss.halo.sprite=boss.nucleo.sprite;
        boss.aviso.sprite=quadrado;boss.aviso.size=Vector2.one;boss.aviso.transform.localScale=Vector3.one;Direcao(boss);
        var onda=PrefabUtility.LoadPrefabContents(Pasta+"/OndaChoque.prefab");onda.GetComponent<SpriteRenderer>().sprite=quadrado;PrefabUtility.SaveAsPrefabAsset(onda,Pasta+"/OndaChoque.prefab");PrefabUtility.UnloadPrefabContents(onda);
        foreach(var guid in AssetDatabase.FindAssets("t:AnimationClip",new[]{Pasta})) {
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid));
            Curva(clip,"BracoE","localPosition.x",0,-1.05f,clip.length,-1.05f);Curva(clip,"BracoD","localPosition.x",0,1.05f,clip.length,1.05f);EditorUtility.SetDirty(clip);
        }
        EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);AssetDatabase.SaveAssets();QuestRuinBossTest.Begin();
    }
    public static void Construir()
    {
        if(Application.isPlaying||Directory.Exists(Pasta))throw new InvalidOperationException("Assets do boss ja existem ou Play ativo; preservar.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Salve alteracoes abertas.");
        Directory.CreateDirectory(Pasta);AssetDatabase.Refresh();
        PrepararSprites();
        var cena=EditorSceneManager.OpenScene("Assets/Scenes/ArenaFloresta.unity",OpenSceneMode.Single);
        foreach(var raiz in cena.GetRootGameObjects()) {
            bool manter=raiz.GetComponent<PatoController>()!=null||raiz.GetComponent<QuestSession>()!=null||raiz.GetComponent<Camera>()!=null||raiz.GetComponent<QuestBackground>()!=null||raiz.GetComponents<Component>().Any(c=>c!=null&&c.GetType().Name=="Light2D");
            if(!manter)UnityEngine.Object.DestroyImmediate(raiz);
        }
        var pato=UnityEngine.Object.FindAnyObjectByType<PatoController>();pato.transform.position=new Vector3(-5.5f,-2.35f,0);pato.GetComponent<QuestPlayer>().pontoSeguro=pato.transform.position;
        var camera=Camera.main;var seguir=camera.GetComponent<QuestCamera>();if(seguir!=null)UnityEngine.Object.DestroyImmediate(seguir);
        camera.transform.position=new Vector3(0,.5f,-10);camera.orthographicSize=5.5f;
        Piso();Parede(-8.3f);Parede(8.3f);
        foreach(float x in new[]{-7.2f,7.2f}) {
            var coluna=Desenho("Coluna de pedra e musgo",null,pedra,new Vector2(x,-1.3f),new Vector2(1.3f,3.6f),2);coluna.color=new Color(.55f,.65f,.57f,.8f);
        }
        var arena=new GameObject("Batalha - camera fixa e resultado").AddComponent<QuestBossArena>();arena.cameraJogo=camera;
        var sessao=UnityEngine.Object.FindAnyObjectByType<QuestSession>();sessao.bossArena=arena;sessao.nomeFase="Ruinas - Guardiao";sessao.totalItens=0;sessao.avisarColetaCompleta=false;
        var raizBoss=new GameObject("Guardiao das Ruinas - golem provisorio");raizBoss.transform.position=new Vector3(4.2f,-3,0);
        var rb=raizBoss.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;
        var caixa=raizBoss.AddComponent<BoxCollider2D>();caixa.isTrigger=true;caixa.offset=new Vector2(0,1.4f);caixa.size=new Vector2(1.9f,2.8f);
        var boss=raizBoss.AddComponent<QuestRuinBoss>();boss.jogador=pato.transform;boss.arena=arena;arena.boss=boss;
        var visual=new GameObject("Visual");visual.transform.SetParent(raizBoss.transform,false);
        var corpo=Desenho("Corpo",visual.transform,pedra,new Vector2(0,1.35f),new Vector2(1.8f,1.9f),6);
        Desenho("Cabeca",visual.transform,cabeca,new Vector2(0,2.65f),new Vector2(1.15f,.9f),7);
        Desenho("BracoE",visual.transform,pedra,new Vector2(-1.05f,1.45f),new Vector2(.65f,1.6f),5);
        Desenho("BracoD",visual.transform,pedra,new Vector2(1.05f,1.45f),new Vector2(.65f,1.6f),5);
        Desenho("PeE",visual.transform,cabeca,new Vector2(-.5f,.2f),new Vector2(.8f,.55f),6);
        Desenho("PeD",visual.transform,cabeca,new Vector2(.5f,.2f),new Vector2(.8f,.55f),6);
        var cristal=CristalNucleo();
        boss.halo=Desenho("Halo",corpo.transform,cristal,Vector2.zero,new Vector2(.8f,1),8);
        boss.nucleo=Desenho("Nucleo",corpo.transform,cristal,Vector2.zero,new Vector2(.5f,.7f),9);
        boss.animator=visual.AddComponent<Animator>();boss.animator.runtimeAnimatorController=Animador();boss.animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        boss.aviso=Desenho("Aviso de area",null,quadrado,new Vector2(0,-2.93f),Vector2.one,10);boss.aviso.enabled=false;Direcao(boss);
        var onda=new GameObject("Onda de choque");var ondaVisual=onda.AddComponent<SpriteRenderer>();ondaVisual.sprite=quadrado;ondaVisual.sharedMaterial=material;ondaVisual.sortingOrder=9;ondaVisual.color=new Color(.35f,1,.45f);
        onda.transform.localScale=new Vector3(.6f,.4f,1);onda.AddComponent<QuestBossWave>();boss.prefabOnda=PrefabUtility.SaveAsPrefabAsset(onda,Pasta+"/OndaChoque.prefab");UnityEngine.Object.DestroyImmediate(onda);
        var portal=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ElementalQuest/TutorialFinal/PortalTutorial.prefab"));
        portal.name="Saida - apos derrota do Guardiao";portal.transform.position=new Vector3(6.6f,-2,0);UnityEngine.Object.DestroyImmediate(portal.GetComponent<QuestPortal>());portal.AddComponent<QuestBossExit>().arena=arena;arena.saida=portal;
        EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);AssetDatabase.SaveAssets();
        Debug.Log("EQ_BOSS_BUILD_OK: arena fixa, golem provisorio, Animator substituivel, tres ataques, quatro aberturas e saida.");
    }
    static SpriteRenderer Desenho(string nome,Transform pai,Sprite sprite,Vector2 pos,Vector2 tamanho,int ordem)
    {
        var obj=new GameObject(nome);if(pai!=null)obj.transform.SetParent(pai,false);obj.transform.localPosition=new Vector3(pos.x,pos.y,0);
        var r=obj.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=ordem;
        // O tamanho fica no renderer para que as curvas do Animator usem coordenadas locais naturais.
        r.drawMode=SpriteDrawMode.Sliced;r.size=tamanho;return r;
    }
    static RuntimeAnimatorController Animador()
    {
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Pasta+"/GuardiaoRuinas.controller");
        var maquina=controller.layers[0].stateMachine;
        foreach(string nome in new[]{"Idle","Preparacao0","Preparacao1","Preparacao2","Execucao0","Execucao1","Execucao2","Recuperacao","Vulneravel","RecebendoDano","Derrotado"}) {
            float tempo=nome.StartsWith("Preparacao")?1.2f:nome=="Execucao0"?.35f:nome=="Execucao1"?2.2f:nome=="Execucao2"?.7f:nome=="Vulneravel"?3.2f:nome=="RecebendoDano"?.35f:nome=="Recuperacao"?.55f:nome=="Derrotado"?1.2f:1.5f;
            var clip=new AnimationClip{name=nome};
            Curva(clip,"BracoE","localPosition.y",0,1.45f,tempo,1.45f);
            Curva(clip,"BracoD","localPosition.y",0,1.45f,tempo,1.45f);
            Curva(clip,"BracoE","localPosition.x",0,-1.05f,tempo,-1.05f);Curva(clip,"BracoD","localPosition.x",0,1.05f,tempo,1.05f);
            Curva(clip,"Corpo","localPosition.x",0,0,tempo,0);Curva(clip,"Corpo","localPosition.y",0,1.35f,tempo,1.35f);
            Curva(clip,"Cabeca","localPosition.y",0,2.65f,tempo,2.65f);
            if(nome=="Preparacao0")Curva(clip,"BracoD","localPosition.y",0,1.45f,tempo,2.9f);
            if(nome=="Preparacao1"){Curva(clip,"BracoD","localPosition.y",0,1.45f,tempo,2.8f);Curva(clip,"BracoE","localPosition.y",0,1.45f,tempo,2.8f);}
            if(nome=="Preparacao2")Curva(clip,"Corpo","localPosition.x",0,0,tempo,-.35f);
            if(nome=="Execucao0")Curva(clip,"BracoD","localPosition.y",0,2.9f,tempo,.35f);
            if(nome=="Execucao1"){Curva(clip,"BracoE","localPosition.y",0,2.8f,.15f,.35f);Curva(clip,"BracoD","localPosition.y",0,2.8f,.15f,.35f);}
            if(nome=="Vulneravel"){Curva(clip,"Corpo","localPosition.y",0,1.15f,tempo,1.15f);Curva(clip,"Cabeca","localPosition.y",0,2.4f,tempo,2.4f);}
            if(nome=="RecebendoDano")clip.SetCurve("Corpo",typeof(Transform),"localPosition.x",new AnimationCurve(new Keyframe(0,0),new Keyframe(.1f,.2f),new Keyframe(.2f,-.15f),new Keyframe(tempo,0)));
            if(nome=="Derrotado"){Curva(clip,"Corpo","localPosition.y",0,1.35f,tempo,.45f);Curva(clip,"Cabeca","localPosition.y",0,2.65f,tempo,.8f);Curva(clip,"BracoE","localPosition.y",0,1.45f,tempo,.2f);Curva(clip,"BracoD","localPosition.y",0,1.45f,tempo,.2f);}
            AssetDatabase.CreateAsset(clip,Pasta+"/"+nome+".anim");var estado=maquina.AddState(nome);estado.motion=clip;if(nome=="Idle")maquina.defaultState=estado;
        }
        return controller;
    }
    static void Curva(AnimationClip clip,string alvo,string propriedade,float t0,float v0,float t1,float v1){clip.SetCurve(alvo,typeof(Transform),propriedade,AnimationCurve.EaseInOut(t0,v0,t1,v1));}
    static void Piso()
    {
        var grid=new GameObject("Piso continuo da arena");grid.layer=3;grid.transform.position=new Vector3(-9,-4,0);grid.AddComponent<Grid>();
        var obj=new GameObject("Tilemap de floresta");obj.transform.SetParent(grid.transform,false);var mapa=obj.AddComponent<Tilemap>();mapa.tileAnchor=new Vector3(.5f,.5625f,0);obj.AddComponent<TilemapRenderer>().sharedMaterial=material;
        for(int x=0;x<18;x++)for(int y=0;y<4;y++) {int col=x==0?0:x==17?2:1;int linha=y==0?0:y==3?2:1;mapa.SetTile(new Vector3Int(x,-y,0),AssetDatabase.LoadAssetAtPath<Tile>("Assets/ElementalQuest/TerrenoContinuo/Tile_"+(linha*3+col).ToString("00")+".asset"));}
        var caixa=grid.AddComponent<BoxCollider2D>();caixa.offset=new Vector2(9,-1);caixa.size=new Vector2(18,4);
    }
    static void Parede(float x){var obj=new GameObject("Limite fisico da arena");obj.transform.position=new Vector3(x,1,0);obj.layer=3;obj.AddComponent<BoxCollider2D>().size=new Vector2(.6f,12);}
}
