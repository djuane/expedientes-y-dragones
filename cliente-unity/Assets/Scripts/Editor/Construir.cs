#nullable enable
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Expedientia.Cliente.Editor
{
    /// <summary>
    /// Prepara la escena y la configuración de Android, y compila el APK.
    /// Desde el editor: menú "Expedientes". Desde la línea de comandos:
    ///   Unity.exe -batchmode -quit -projectPath cliente-unity -executeMethod Expedientia.Cliente.Editor.Construir.Android -logFile -
    /// </summary>
    public static class Construir
    {
        private const string Escena = "Assets/Escenas/Principal.unity";

        [MenuItem("Expedientes/Preparar proyecto")]
        public static void Preparar()
        {
            if (!File.Exists(Escena))
            {
                Directory.CreateDirectory("Assets/Escenas");
                var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var cam = new GameObject("Camara", typeof(Camera), typeof(AudioListener));
                cam.tag = "MainCamera";
                var c = cam.GetComponent<Camera>();
                c.clearFlags = CameraClearFlags.SolidColor;
                c.backgroundColor = new Color32(0x0E, 0x0C, 0x1A, 0xFF);
                c.orthographic = true;
                EditorSceneManager.SaveScene(escena, Escena);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Escena, true) };

            PlayerSettings.companyName = "Leonel Díaz";
            PlayerSettings.productName = "Expedientes y Dragones";
            PlayerSettings.bundleVersion = Version();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "pe.expedientesydragones.juego");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            // Prototipo en la red local: el servidor de pruebas responde por http.
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            AssetDatabase.SaveAssets();
            Debug.Log("Proyecto preparado.");
        }

        [MenuItem("Expedientes/Compilar APK de Android")]
        public static void Android()
        {
            Preparar();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            Directory.CreateDirectory("Builds");
            var salida = $"Builds/ExpedientesYDragones-{Version()}.apk";
            var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Escena },
                locationPathName = salida,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            });
            Debug.Log($"Compilación: {r.summary.result}, {r.summary.totalErrors} errores, {r.summary.totalSize / 1048576f:0.0} MB → {salida}");
            if (Application.isBatchMode) EditorApplication.Exit(r.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>
        /// Compilación de Windows con la marca CAPTURAS (no afecta al APK). Ejecutar luego:
        ///   Builds/Capturas/ExpedientesYDragones.exe -capturas &lt;carpeta&gt;
        /// </summary>
        [MenuItem("Expedientes/Compilar capturas (Windows)")]
        public static void Capturas()
        {
            Preparar();
            Directory.CreateDirectory("Builds/Capturas");
            var salida = "Builds/Capturas/ExpedientesYDragones.exe";
            var r = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Escena },
                locationPathName = salida,
                target = BuildTarget.StandaloneWindows64,
                extraScriptingDefines = new[] { "CAPTURAS" },
                options = BuildOptions.None,
            });
            Debug.Log($"Capturas: {r.summary.result}, {r.summary.totalErrors} errores → {salida}");
            if (Application.isBatchMode) EditorApplication.Exit(r.summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static string Version()
        {
            try
            {
                var p = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo("git", "describe --tags --always")
                    { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true },
                };
                p.Start();
                var v = p.StandardOutput.ReadToEnd().Trim().TrimStart('v');
                p.WaitForExit(3000);
                return string.IsNullOrEmpty(v) ? "0.6.0" : v;
            }
            catch (Exception) { return "0.6.0"; }
        }
    }
}
