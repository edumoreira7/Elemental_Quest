using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class QuestRuinBossUpgradeBuilder
{
    const string Art="Assets/ElementalQuest/GuardiaoRuinasVisual/Generated";
    const string Pasta="Assets/ElementalQuest/GuardiaoRuinasIntegrado";
    public static void IntegrarEValidar()
    {
        if(Application.isPlaying||Directory.Exists(Pasta))throw new InvalidOperationException("Integracao ja existe ou Play ativo; preservar alteracoes.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Salve as cenas abertas.");
        Directory.CreateDirectory(Pasta);AssetDatabase.Refresh();
        var cena=EditorSceneManager.OpenScene("Assets/Scenes/ArenaFloresta.unity",OpenSceneMode.Single);
        var boss=UnityEngine.Object.FindAnyObjectByType<QuestRuinBoss>();
        var controlador=AnimatorController.CreateAnimatorControllerAtPath(Pasta+"/GuardiaoCombate.controller");
        var maquina=controlador.layers[0].stateMachine;
        Adicionar(maquina,"Idle","Idle",0,6,1,true);
        Adicionar(maquina,"Preparacao0","Attack_Slam",0,4,.95f,false);
        Adicionar(maquina,"Execucao0","Attack_Slam",4,4,.32f,false);
        Adicionar(maquina,"Preparacao1","Attack_Shockwave",0,4,.95f,false);
        Adicionar(maquina,"Execucao1","Attack_Shockwave",4,4,1.85f,false,new[]{0f,.12f,.3f,.55f});
        Adicionar(maquina,"Preparacao2","Attack_Charge",0,4,.95f,false);
        Adicionar(maquina,"Execucao2","Attack_Charge",4,4,.52f,false);
        Adicionar(maquina,"Recuperacao","Vulnerable",0,6,1,true);
        Adicionar(maquina,"Vulneravel","Vulnerable",0,6,1,true);
        Adicionar(maquina,"RecebendoDano","Hurt",0,4,1f/3,false);
        Adicionar(maquina,"Derrotado","Death",0,10,1.25f,false);
        UnityEngine.Object.DestroyImmediate(boss.animator.gameObject);
        boss.nucleo=null;boss.halo=null;
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/GuardiaoRuinas_Visual.prefab"));
        visual.name="Golem - sprites finais integrados";visual.transform.SetParent(boss.transform,false);visual.transform.localPosition=Vector3.zero;visual.transform.localScale=new Vector3(-1.3f,1.3f,1);
        boss.animator=visual.GetComponent<Animator>();boss.animator.runtimeAnimatorController=controlador;boss.animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        boss.visualPrincipal=visual.GetComponent<SpriteRenderer>();boss.visualPrincipal.sortingOrder=6;boss.escalaVisual=1.3f;boss.vidaMaxima=6;
        var core=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Core_FX.prefab"));
        core.transform.SetParent(visual.transform,false);core.transform.localPosition=new Vector3(0,.95f,0);core.transform.localScale=Vector3.one*.32f;core.GetComponent<SpriteRenderer>().sortingOrder=8;
        boss.efeitoNucleo=core;boss.pontoNucleo=core.transform;core.SetActive(false);
        var onda=UnityEngine.Object.Instantiate(boss.prefabOnda);onda.name="Onda de choque - sprites";onda.transform.localScale=Vector3.one;onda.GetComponent<SpriteRenderer>().enabled=false;
        var fx=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Shockwave_FX.prefab"));fx.transform.SetParent(onda.transform,false);fx.transform.localPosition=Vector3.zero;fx.transform.localScale=Vector3.one*.5f;fx.GetComponent<SpriteRenderer>().sortingOrder=10;
        var visualOnda=AnimatorController.CreateAnimatorControllerAtPath(Pasta+"/OndaVisual.controller");
        Adicionar(visualOnda.layers[0].stateMachine,"OndaAtiva","Shockwave_FX",1,4,.5f,true);visualOnda.layers[0].stateMachine.defaultState=visualOnda.layers[0].stateMachine.states[0].state;
        fx.GetComponent<Animator>().runtimeAnimatorController=visualOnda;fx.GetComponent<Animator>().cullingMode=AnimatorCullingMode.AlwaysAnimate;
        onda.GetComponent<QuestBossWave>().velocidade=4.2f;onda.GetComponent<QuestBossWave>().alcance=6;
        boss.prefabOnda=PrefabUtility.SaveAsPrefabAsset(onda,Pasta+"/OndaChoqueAnimada.prefab");UnityEngine.Object.DestroyImmediate(onda);
        EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);AssetDatabase.SaveAssets();
        Debug.Log("EQ_BOSS_UPGRADE_BUILD_OK: sprites integrados, seis vidas, ritmo acelerado, absorcao de Terra.");QuestRuinBossUpgradeTest.Begin();
    }
    static void Adicionar(AnimatorStateMachine maquina,string nome,string fonte,int primeiro,int quantidade,float duracao,bool loop,float[] tempos=null)
    {
        var origem=AssetDatabase.LoadAssetAtPath<AnimationClip>(Art+"/"+fonte+".anim");
        var binding=AnimationUtility.GetObjectReferenceCurveBindings(origem).Single(b=>b.type==typeof(SpriteRenderer));
        var sprites=AnimationUtility.GetObjectReferenceCurve(origem,binding).Select(k=>k.value).Distinct().Cast<Sprite>().ToArray();
        if(sprites.Length<primeiro+quantidade)throw new InvalidOperationException("Frames insuficientes: "+fonte);
        var keys=new ObjectReferenceKeyframe[quantidade+1];
        for(int i=0;i<quantidade;i++)keys[i]=new ObjectReferenceKeyframe{time=tempos==null?i*duracao/quantidade:tempos[i],value=sprites[primeiro+i]};
        keys[quantidade]=new ObjectReferenceKeyframe{time=duracao,value=sprites[primeiro+quantidade-1]};
        var clip=new AnimationClip{name=nome,frameRate=12};AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
        var configuracao=AnimationUtility.GetAnimationClipSettings(clip);configuracao.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,configuracao);
        AssetDatabase.CreateAsset(clip,Pasta+"/"+nome+".anim");var estado=maquina.AddState(nome);estado.motion=clip;estado.writeDefaultValues=false;
        if(nome=="Idle")maquina.defaultState=estado;
    }
}
