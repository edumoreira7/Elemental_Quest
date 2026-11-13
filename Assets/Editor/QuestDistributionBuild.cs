using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Run each platform in a separate batch invocation with its matching -buildTarget.
public static class QuestDistributionBuild
{
    public static void Windows() { Build(BuildTarget.StandaloneWindows64); }
    public static void Web() { Build(BuildTarget.WebGL); }
    static string Argument(string name)
    {
        var args = Environment.GetCommandLineArgs(); int i = Array.IndexOf(args, name);
        if (i < 0 || i + 1 >= args.Length) throw new ArgumentException("Missing " + name);
        return args[i + 1];
    }
    static void Build(BuildTarget target)
    {
        try
        {
            string output = Path.GetFullPath(Argument("-questOutput"));
            string revision = Argument("-questRevision");
            if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any()) throw new IOException("Preserve existing output: " + output);
            Directory.CreateDirectory(output);
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0 || scenes[0] != "Assets/Scenes/Inicio.unity") throw new InvalidOperationException("Build must start at Inicio.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Unknown, target)) throw new InvalidOperationException("Platform module missing: " + target);
            if (target == BuildTarget.StandaloneWindows64) PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            else
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
            }
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = scenes, target = target, options = BuildOptions.None,
                locationPathName = target == BuildTarget.WebGL ? output : Path.Combine(output, "ElementalQuest.exe")
            });
            File.WriteAllText(Path.Combine(output, "BUILD-INFO.txt"),
                "Elemental Quest\nRevision: " + revision + "\nUnity: " + Application.unityVersion +
                "\nPlatform: " + target + "\nResult: " + report.summary.result + "\nBytes: " + report.summary.totalSize +
                "\nScenes:\n" + string.Join("\n", scenes) + "\n");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Build failed: " + report.summary.result);
            Debug.Log("EQ_DISTRIBUTION_OK: " + target + " " + output); EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
