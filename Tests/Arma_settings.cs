using _ARK_;
using System;
using System.IO;
using UnityEngine;

partial class ARMA
{
    [Serializable]
    public class Version : JSon
    {
        public static readonly string version_file = typeof(Version).TypeToFileName() + json;

#if UNITY_EDITOR
        public static readonly string
            dir_editor = Path.Combine(Application.dataPath, "Resources"),
            file_editor = Path.Combine(dir_editor, version_file);
#endif

        public byte VERSION;
    }

    [Serializable]
    public class Settings : JSon
    {
        public static readonly string file_name = typeof(Settings).TypeToFileName() + json;
        public static string FileDir => NUCLEOR.home_path.ForceDir().FullName;
        public static string FilePath => Path.Combine(FileDir, file_name);
    }

    public static Version version;
    public static Settings settings;

    //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(IncrementVersion))]
    public static void IncrementVersion()
    {
        ++version.VERSION;
        Debug.Log($"{nameof(IncrementVersion)}: {version.VERSION}");
        version.Save(Version.file_editor, true);
    }

    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(DecrementVersion))]
    static void DecrementVersion()
    {
        --version.VERSION;
        version.Save(Version.file_editor, true);
        Debug.Log($"{nameof(DecrementVersion)}: {version.VERSION}");
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        version ??= new();

        TextAsset text = Resources.Load<TextAsset>(Version.version_file[..^".txt".Length]);
        version = JsonUtility.FromJson<Version>(text.text);
        version.OnRead();

        Debug.Log($"[ARMA_VERSION]: {version.VERSION}");

        LoadSettings_logged();

#if UNITY_EDITOR
        return;
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(LoadSettings))]
#endif
    static void LoadSettings_logged() => LoadSettings(true);
    public static void LoadSettingsNoLog() => LoadSettings(false);
    public static void LoadSettings(in bool log)
    {
        settings ??= new();
        JSon.Read(ref settings, Settings.FilePath, true, log);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(SaveSettings))]
#endif
    static void SaveSettings_logged() => SaveSettings(true);
    public static void SaveSettingsNoLog() => SaveSettings(false);
    public static void SaveSettings(in bool log)
    {
        settings ??= new();
        settings.Save(Settings.FilePath, log);
    }
}