using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class QuestRegionalPresentationReview
{
    const string Output = "C:/Users/morei/Documents/Jogos Digitais/Ajustes_Fases_2_4/Review";
    static readonly string[] Scenes = { "CumeDosVentos", "ArenaCume", "VulcaoCinzento", "ArenaVulcao", "CasteloDoEclipse", "ArenaEclipse" };
    static readonly float[] X = { 5, 28, 60, 104 };
    static double next;
    static int scene, phase, frame;
    static float previousOffset;
    static Sprite lastSprite;
    static Vector3 lastPosition;
    [InitializeOnLoadMethod]
    static void Resume()
    {
        if (!Application.isBatchMode || Array.IndexOf(Environment.GetCommandLineArgs(), "QuestRegionalPresentationReview.Run") < 0) return;
        EditorApplication.playModeStateChanged -= Changed; EditorApplication.playModeStateChanged += Changed;
        if (EditorApplication.isPlaying) Begin();
    }
    public static void Run()
    {
        Directory.CreateDirectory(Output);
        SessionState.SetInt("EQ_REGIONAL_SCENE", 0);
        File.WriteAllText(Output + "/report.txt", "Regional presentation Play review\n");
        EditorSceneManager.OpenScene("Assets/Scenes/" + Scenes[0] + ".unity");
        EditorApplication.playModeStateChanged -= Changed; EditorApplication.playModeStateChanged += Changed;
        EditorApplication.isPlaying = true;
    }
    static void Changed(PlayModeStateChange state) { if (state == PlayModeStateChange.EnteredPlayMode) Begin(); }
    static void Begin()
    {
        Screen.SetResolution(1280, 720, false);
        scene = SessionState.GetInt("EQ_REGIONAL_SCENE", 0); phase = 0; frame = 0;
        next = EditorApplication.timeSinceStartup + 2;
        EditorApplication.update -= Tick; EditorApplication.update += Tick;
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < next) return;
        try
        {
            var p = Object.FindAnyObjectByType<QuestLayeredParallax>();
            if (p == null || p.Panels == null || p.Panels.Length != 3) throw new Exception("Missing layers " + Scenes[scene]);
            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>())
                if ((sr.name == "Background regional composto" || sr.name == "Eclipse - panorama") && sr.enabled)
                    throw new Exception("Composite still visible");
            var region = Object.FindAnyObjectByType<QuestRegionScenery>();
            var castle = Object.FindAnyObjectByType<QuestCastleScenery>();
            int floors = region != null ? region.DecoratedFloors : castle.DecoratedFloors;
            if (floors != (scene % 2 == 0 ? 7 : 1)) throw new Exception("Floor count changed: " + floors);
            if (scene % 2 == 0) Stage(p); else Arena(p);
        }
        catch (Exception e)
        {
            File.AppendAllText(Output + "/report.txt", "FAIL " + Scenes[scene] + ": " + e + "\n");
            Debug.LogException(e); EditorApplication.Exit(1);
        }
    }
    static void Stage(QuestLayeredParallax p)
    {
        var cam = Camera.main;
        var follow = cam.GetComponent<QuestCamera>(); if (follow != null) follow.enabled = false;
        if (phase == 0)
        {
            cam.transform.position = new Vector3(X[frame], .5f, -10);
            phase = 1; next = EditorApplication.timeSinceStartup + .15; return;
        }
        float far = p.Panels[0][2].position.x - cam.transform.position.x;
        float near = p.Panels[2][2].position.x - cam.transform.position.x;
        if (frame > 0 && Mathf.Abs(far - near) < .05f) throw new Exception("Layers not moving independently");
        foreach (var row in p.Panels)
            for (int i = 1; i < row.Length; i++)
                if (Mathf.Abs(row[i].GetComponent<SpriteRenderer>().bounds.min.x - row[i - 1].GetComponent<SpriteRenderer>().bounds.max.x) > .001f)
                    throw new Exception("Gap between panels");
        Capture(Scenes[scene] + "-x" + X[frame]);
        File.AppendAllText(Output + "/report.txt", Scenes[scene] + " x=" + X[frame] + ": 3 layers, separate offsets, no panel gaps, 7 floors.\n");
        previousOffset = near; frame++; phase = 0;
        if (frame == X.Length) Advance(); else next = EditorApplication.timeSinceStartup + .1;
    }
    static void Arena(QuestLayeredParallax p)
    {
        var d = Object.FindAnyObjectByType<QuestBossDefeatPresentation>();
        if (d == null || d.Body == null) throw new Exception("Defeat renderer missing");
        if (phase == 0)
        {
            Capture(Scenes[scene] + "-before");
            Component arena = (Component)Object.FindAnyObjectByType<QuestTempestArena>() ??
                (Component)Object.FindAnyObjectByType<QuestMagmaArena>() ?? Object.FindAnyObjectByType<QuestEclipseArena>();
            var t = arena.GetType();
            var set = t.GetMethod("Set", BindingFlags.Instance | BindingFlags.NonPublic);
            var vulnerable = Enum.Parse(t.GetNestedType("State"), "Vulnerable");
            for (int i = 0; i < 6; i++)
            {
                set.Invoke(arena, new[] { vulnerable, (object)10f });
                var go = new GameObject("Review projectile", typeof(BoxCollider2D), typeof(ProjetilPoder));
                go.transform.position = new Vector3(500, 500, 0);
                t.GetMethod("Receive").Invoke(arena, new object[] { go.GetComponent<Collider2D>() });
            }
            if ((int)t.GetProperty("Health").GetValue(arena) != 0) throw new Exception("Damage path did not defeat boss");
            phase = 1; frame = 0; next = EditorApplication.timeSinceStartup + .05; return;
        }
        if (!d.Playing || d.Body.sharedMaterial.mainTexture != d.Body.sprite.texture) throw new Exception("Defeat/material mismatch");
        var expected = d.frames[Mathf.Min(d.frames.Length - 1, Mathf.FloorToInt(d.Elapsed * 8))];
        if (d.Body.sprite != expected) throw new Exception("Wrong defeated frame");
        Capture(Scenes[scene] + "-defeat-" + frame.ToString("00"));
        if (frame == 0) lastPosition = d.Body.transform.position;
        if (Vector3.Distance(lastPosition, d.Body.transform.position) > .001f) throw new Exception("Boss anchor moved during defeat");
        if (frame == 8) lastSprite = d.Body.sprite;
        if (frame >= 8 && d.Body.sprite != d.frames[7]) throw new Exception("Last frame not reached");
        if (frame == 9 && d.Body.sprite != lastSprite) throw new Exception("Final pose not held");
        frame++;
        if (frame >= 10)
        {
            File.AppendAllText(Output + "/report.txt", Scenes[scene] + ": lethal damage through Receive; defeated animation sampled, fixed anchor, material texture correct, last pose held.\n");
            Advance();
        }
        else next = EditorApplication.timeSinceStartup + (frame < 8 ? .125 : .6);
    }
    static void Advance()
    {
        scene++;
        if (scene == Scenes.Length)
        {
            File.AppendAllText(Output + "/report.txt", "PASS all six scenes. No complete campaign traversal or player-controlled combat test.\n");
            Debug.Log("EQ_REGIONAL_PRESENTATION_REVIEW_OK"); EditorApplication.Exit(0); return;
        }
        SessionState.SetInt("EQ_REGIONAL_SCENE", scene); phase = 0; frame = 0;
        SceneManager.LoadScene(Scenes[scene]); next = EditorApplication.timeSinceStartup + 2;
    }
    static void Capture(string name)
    {
        var cam = Camera.main; var rt = new RenderTexture(1280, 720, 24);
        var old = cam.targetTexture; var oldActive = RenderTexture.active;
        cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
        File.WriteAllBytes(Output + "/" + name + ".png", tex.EncodeToPNG());
        cam.targetTexture = old; RenderTexture.active = oldActive;
        Object.DestroyImmediate(tex); rt.Release(); Object.DestroyImmediate(rt);
    }
}
