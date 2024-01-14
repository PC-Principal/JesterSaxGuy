using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

/* Mod namespace */
namespace JesterSaxGuy
{
    /* required params for BepInEx Plugin */
    [BepInPlugin(modGUID, modName, modVersion)]
    
    /* Class with mod logic */
    public class Plugin : BaseUnityPlugin
    {
        /* Mod unique name */
        private const string modGUID = "PC_Principal.JesterSaxGuy";

        /* Mode name in store */
        private const string modName = "Jester sax guy sound";

        /* mod version */
        private const string modVersion = "1.0.0";
        
        /* Audio file variable */
        public static AudioClip Audio;
        
        /** bool variable with timer music */
        public static bool ConstTimer = true;

        /* Called when plugin initiated */
        private void Awake()
        {
            /* Const for sync JesterMusic */
            ConstTimer = Config.Bind("General", "ConstTimer", true, "Sets the Jester's popUpTimer field to line up with the song").Value;

            /* Music Location */
            string location = Info.Location.TrimEnd($"{modGUID}.dll".ToCharArray());
            
            /* Unity WebReauestVariable */
            UnityWebRequest audioLoader = UnityWebRequestMultimedia.GetAudioClip($"File://{location}JesterSaxGuy.mp3", AudioType.MPEG);

            /* Send request for new music */
            audioLoader.SendWebRequest();
            
            /* Waiting for loading music */
            while (!audioLoader.isDone) { }

            /* When request is success */
            if (audioLoader.result == UnityWebRequest.Result.Success)
            {
                /* Load audio with handler */
                Audio = DownloadHandlerAudioClip.GetContent(audioLoader);

                /* Initiate Harmony plugin info */
                new Harmony(PluginInfo.PLUGIN_GUID).PatchAll(typeof(JesterPatch));
                
                /* Logging that script is loaded */
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            }
            else /* If not request successful */
            {
                /* Log that audio file is not loaded */
                Logger.LogError($"Could not load audio file");
            }
        }
    }

    /* Patch JesterAI */
    [HarmonyPatch(typeof(JesterAI))]
    
    /* Jester Patch class */
    internal class JesterPatch
    {
        /* Postfix Audio Patch */
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void AudioPatch(JesterAI __instance)
        {
            /* SetUp Instance with new music */
            __instance.popGoesTheWeaselTheme = Plugin.Audio;
        }

        /* Postfix Updating Time PopUp Jester Head */
        [HarmonyPatch("SetJesterInitialValues")]
        [HarmonyPostfix]
        public static void ForceTime(JesterAI __instance)
        {
            /* If const timer exists */
            if (Plugin.ConstTimer)
            {
                /* Change instance popUpTimer */
                __instance.popUpTimer = 41.5f;
            }
        }
    }
}