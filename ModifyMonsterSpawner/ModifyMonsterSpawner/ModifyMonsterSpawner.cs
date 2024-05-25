using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;



internal class ModifySpawnerConfig
{

    private static ConfigEntry<int> _spawnModify;

    private static ConfigEntry<float> _spawnDuration;


    public static int SpawnModify
    {
        get
        {
            return _spawnModify.Value;
        }
        set
        {
            _spawnModify.Value = value;
        }
    }

    public static float SpawnDuration
    {
        get
        {
            return _spawnDuration.Value;
        }
        set
        {
            _spawnDuration.Value = value;
        }
    }

    internal static void Bind(ConfigFile config)
    {
        _spawnModify = config.Bind<int>("General", "Spawn Modify", 3, "Modifier of monster spawning quantity");
        _spawnDuration = config.Bind<float>("General", "Spawn Duration", 2.5f, "The gap between the first and second wave sleeper");
    }
}

namespace ModifyMonsterSpawner
{
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ModifyMonsterSpawner : BaseUnityPlugin
    {
        public static ModifyMonsterSpawner Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            ModifySpawnerConfig.Bind(((BaseUnityPlugin)this).Config);
            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

    }
    
}