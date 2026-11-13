using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class QuestTestBuild
{
    const string Root="C:/Users/morei/Documents/Jogos Digitais/Builds";
    const string Status="C:/Users/morei/Documents/Jogos Digitais/BuildTeste/status.txt";
    const string Request="C:/Users/morei/Documents/Jogos Digitais/BuildTeste/request.txt";
    [InitializeOnLoadMethod]
    static void ListenForRequest()
    {
        if(Application.isBatchMode||Application.dataPath.Replace('\\','/')!="C:/Users/morei/Jogos Digitais/Elemental_Quest/Assets")return;
        EditorApplication.update-=CheckRequest;EditorApplication.update+=CheckRequest;
    }
    static void CheckRequest()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlaying||!File.Exists(Request))return;
        File.Delete(Request);EditorApplication.delayCall+=Run;
    }
    [MenuItem("Tools/Elemental Quest/Gerar versao de teste Windows")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Saia do Play antes de gerar a versao de teste.");
        string folder=Root+"/ElementalQuest-Teste-2026-11-13";
        for(int suffix=2;Directory.Exists(folder);suffix++)folder=Root+"/ElementalQuest-Teste-2026-11-13-"+suffix;
        Directory.CreateDirectory(folder);
        File.WriteAllText(Status,"BUILDING\n"+folder);
        try {
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            if(scenes.Length==0||scenes[0]!="Assets/Scenes/Inicio.unity")throw new InvalidOperationException("A lista de cenas deve iniciar pelo menu.");
            var options=new BuildPlayerOptions{
                scenes=scenes,locationPathName=folder+"/ElementalQuest.exe",
                target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
            };
            var report=BuildPipeline.BuildPlayer(options);
            var notes=new StringBuilder();
            notes.AppendLine("Elemental Quest - versao de teste Windows x64");
            notes.AppendLine("Resultado: "+report.summary.result);
            notes.AppendLine("Unity: "+Application.unityVersion);
            notes.AppendLine("Cenas: "+scenes.Length);
            notes.AppendLine("Tamanho: "+report.summary.totalSize+" bytes");
            foreach(var step in report.steps)foreach(var message in step.messages)
                if(message.type==LogType.Error||message.type==LogType.Exception)notes.AppendLine(message.content);
            File.WriteAllText(folder+"/BUILD-INFO.txt",notes.ToString());
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException(notes.ToString());
            File.WriteAllText(folder+"/LEIA-ME.txt","Versao de teste em desenvolvimento.\nAbra ElementalQuest.exe. Mantenha o executavel junto das demais pastas e arquivos desta pasta.\nControles: A/D ou setas; Shift para correr; Espaco para pular; 1-4 elementos; J magia; Ar + Espaco no ar: salto duplo; Ar + segurar J na descida: planar; Esc pausa.\n");
            File.WriteAllText(Status,"SUCCESS\n"+folder+"\n"+report.summary.totalSize);
            Debug.Log("EQ_TEST_BUILD_SUCCESS: "+folder);
        }catch(Exception ex){File.WriteAllText(Status,"FAILED\n"+folder+"\n"+ex);Debug.LogException(ex);}
    }
}
