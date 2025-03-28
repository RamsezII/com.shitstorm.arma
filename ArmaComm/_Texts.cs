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

        public string lobby_name = "lobby_default";
        public string lobby_pass;

        public int LobbyHash => string.IsNullOrWhiteSpace(lobby_pass) ? 0 : lobby_pass.GetHashCode(StringComparison.Ordinal);
    }

    public static Version version;
    public static Settings settings;

    //----------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(IncrementVersion))]
    public static void IncrementVersion()
    {
        version ??= new();
        ++version.VERSION;
        Debug.Log($"{nameof(IncrementVersion)}: {version.VERSION}");
        version.Save(Version.file_editor, true);
    }

    [UnityEditor.MenuItem("Assets/" + nameof(_WRTC_) + "/" + nameof(DecrementVersion))]
    static void DecrementVersion()
    {
        version ??= new();
        --version.VERSION;
        version.Save(Version.file_editor, true);
        Debug.Log($"{nameof(DecrementVersion)}: {version.VERSION}");
    }
#endif

    //----------------------------------------------------------------------------------------------------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        TextAsset text = Resources.Load<TextAsset>(Version.version_file[..^".txt".Length]);
        version = JsonUtility.FromJson<Version>(text.text);
        version.OnRead();

        Debug.Log($"[ARMA_VERSION]: {version.VERSION}");

        LoadSettings(true);
    }

    //----------------------------------------------------------------------------------------------------------

    public static void LoadSettings(in bool log)
    {
        settings ??= new();
        JSon.Read(ref settings, Settings.FilePath, true, log);

        if (string.IsNullOrWhiteSpace(settings.lobby_name))
        {
            Debug.LogWarning($"[ARMA] {nameof(Settings.lobby_name)} is null or empty, setting to default");
            settings.lobby_name = "lobby_default";
            SaveSettings(log);
        }
    }

    public static void SaveSettings(in bool log)
    {
        settings ??= new();
        settings.Save(Settings.FilePath, log);
    }
}