using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

// Visual-only import: never opens or edits an existing scene, enemy prefab or combat script.
public static class GuardiaoRuinasVisualBuilder
{
    const string Root = "Assets/ElementalQuest/GuardiaoRuinasVisual";
    const string Generated = Root + "/Generated";
    [Serializable] class Manifest { public ClipSpec[] clips; }
    [Serializable] class ClipSpec { public string name; public int frames; public int fps; public bool loop; }
    [Serializable] class Report { public bool success; public int characterFrames; public int clips; public int effects; public int assertions; public string combatIntegration; public string runtimeCombatTest; }
    static int checks;
    static string Absolute(string path) => Path.Combine(Directory.GetParent(Application.dataPath).FullName, path);
    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }

    [MenuItem("Elemental Quest/Guardiao das Ruinas/Importar pacote visual e validar")]
    public static void BuildAndValidate()
    {
        checks = 0;
        Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Saia do Play antes de importar.");
        Require(!Directory.Exists(Absolute(Generated)), "Generated ja existe; nenhum asset existente sera sobrescrito.");
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Absolute(Root + "/manifesto.json")));
        Require(manifest.clips != null && manifest.clips.Length == 8, "Esperadas oito animacoes do boss.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        foreach (string file in Directory.GetFiles(Absolute(Root + "/Frames"), "*.png", SearchOption.AllDirectories))
            Configure(file, new Vector2(.5f, .0625f));
        foreach (string file in Directory.GetFiles(Absolute(Root + "/Effects"), "*.png", SearchOption.AllDirectories))
            Configure(file, file.Contains("Core_FX") ? new Vector2(.5f,.5f) : new Vector2(.5f,.125f));
        AssetDatabase.CreateFolder(Root, "Generated");
        var clips = new Dictionary<string,AnimationClip>();
        var frames = new Dictionary<string,Sprite[]>();
        int characterFrames = 0;
        foreach (var spec in manifest.clips)
        {
            var sprites = ReadSprites(Root + "/Frames/" + spec.name);
            Require(sprites.Length == spec.frames, spec.name + ": numero de quadros diferente do manifesto.");
            var clip = MakeClip(spec.name, sprites, spec.fps, spec.loop);
            clips.Add(spec.name, clip); frames.Add(spec.name, sprites);
            characterFrames += sprites.Length;
        }
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Generated + "/GuardiaoRuinas.controller");
        var machine = controller.layers[0].stateMachine;
        var states = new Dictionary<string,AnimatorState>();
        foreach (var spec in manifest.clips)
        {
            var state = machine.AddState(spec.name);
            state.motion = clips[spec.name]; state.writeDefaultValues = false;
            states.Add(spec.name, state);
            controller.AddParameter(spec.name, AnimatorControllerParameterType.Trigger);
            var transition = machine.AddAnyStateTransition(state);
            transition.hasExitTime = false; transition.duration = 0; transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0, spec.name);
        }
        machine.defaultState = states["Idle"];
        foreach (var spec in manifest.clips.Where(s => !s.loop && s.name != "Death"))
        {
            var transition = states[spec.name].AddTransition(states["Idle"]);
            transition.hasExitTime = true; transition.exitTime = 1; transition.duration = 0;
        }
        // Death deliberately has no automatic return transition: its final sprite stays visible.
        var material = new Material(Shader.Find("Sprites/Default"));
        AssetDatabase.CreateAsset(material, Generated + "/GuardiaoSemLuz.mat");
        var visual = new GameObject("Guardiao das Ruinas - Visual");
        try
        {
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sharedMaterial = material; renderer.sprite = frames["Idle"][0];
            var animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
            foreach (var spec in manifest.clips)
            {
                var clip = clips[spec.name];
                Require(AnimationUtility.GetCurveBindings(clip).Length == 0, "Nao deve haver curvas de transform: " + spec.name);
                Require(AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 1, "Esperada apenas curva de sprite.");
                Require(AnimationUtility.GetAnimationClipSettings(clip).loopTime == spec.loop, "Loop incorreto.");
                for (int i = 0; i < spec.frames; i++)
                {
                    clip.SampleAnimation(visual, (i+.1f)/spec.fps);
                    Require(renderer.sprite == frames[spec.name][i], "Ordem dos frames incorreta: " + spec.name + "/" + i);
                    Require(visual.transform.localPosition == Vector3.zero && visual.transform.localScale == Vector3.one, "Transform alterado pela animacao.");
                    Require(renderer.sprite.pixelsPerUnit == 100, "PPU incorreto.");
                    Require(renderer.sprite.rect.width == 512 && renderer.sprite.rect.height == 512, "Canvas inconsistente.");
                    Require(Mathf.Abs(renderer.sprite.pivot.y - 32) < .01f, "Pivo dos pes inconsistente.");
                }
            }
            Require(states["Death"].transitions.Length == 0, "Death deve permanecer na pose final.");
            Require(machine.anyStateTransitions.Length == 8, "Transicoes AnyState incompletas.");
            renderer.sprite = frames["Idle"][0];
            PrefabUtility.SaveAsPrefabAsset(visual, Generated + "/GuardiaoRuinas_Visual.prefab");
        }
        finally { UnityEngine.Object.DestroyImmediate(visual); }
        MakeEffect("Shockwave_FX", false, material);
        MakeEffect("Core_FX", true, material);
        AssetDatabase.SaveAssets();
        var report = new Report { success=true, characterFrames=characterFrames, clips=8, effects=2, assertions=checks,
            combatIntegration="PENDENTE: controlador novo do boss nao localizado; nenhum dano, hitbox ou evento de combate conectado.",
            runtimeCombatTest="NAO EXECUTADO. Validacao de importacao, curvas e amostragem de clips no Editor; nao equivale a teste da batalha." };
        File.WriteAllText(Absolute(Root + "/Validacao-Unity.json"), JsonUtility.ToJson(report, true));
        Debug.Log("GUARDIAO_VISUAL_OK " + JsonUtility.ToJson(report));
    }

    static void Configure(string file, Vector2 pivot)
    {
        string project = Directory.GetParent(Application.dataPath).FullName;
        string path = file.Substring(project.Length+1).Replace('\\','/');
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Require(importer != null, "Importador ausente: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 1024;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot; settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings); importer.SaveAndReimport();
        Require(importer.filterMode == FilterMode.Point, "Filtro incorreto: " + path);
    }
    static Sprite[] ReadSprites(string folder)
    {
        return Directory.GetFiles(Absolute(folder), "*.png").OrderBy(p=>p, StringComparer.Ordinal)
            .Select(p=>AssetDatabase.LoadAssetAtPath<Sprite>(folder+"/"+Path.GetFileName(p))).ToArray();
    }
    static AnimationClip MakeClip(string name, Sprite[] sprites, int fps, bool loop)
    {
        Require(sprites.Length>0 && sprites.All(s=>s!=null), "Sprites ausentes: "+name);
        var clip = new AnimationClip { name=name, frameRate=fps };
        var keys = sprites.Select((sprite,i)=>new ObjectReferenceKeyframe { time=(float)i/fps, value=sprite }).ToList();
        keys.Add(new ObjectReferenceKeyframe { time=(float)sprites.Length/fps-.0001f, value=sprites.Last() });
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys.ToArray());
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=loop;
        AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip, Generated+"/"+name+".anim");
        return clip;
    }
    static void MakeEffect(string name, bool loop, Material material)
    {
        var sprites = ReadSprites(Root+"/Effects/"+name);
        Require(sprites.Length==6,"Efeito deve ter seis quadros.");
        var clip = MakeClip(name,sprites,10,loop);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Generated+"/"+name+".controller");
        var state=controller.layers[0].stateMachine.AddState(name);state.motion=clip;
        controller.layers[0].stateMachine.defaultState=state;
        var obj = new GameObject(name);
        try
        {
            var renderer=obj.AddComponent<SpriteRenderer>();renderer.sprite=sprites[0];renderer.sharedMaterial=material;
            obj.AddComponent<Animator>().runtimeAnimatorController=controller;
            PrefabUtility.SaveAsPrefabAsset(obj,Generated+"/"+name+".prefab");
        }
        finally { UnityEngine.Object.DestroyImmediate(obj); }
    }
}
