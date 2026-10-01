using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LokisChair
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "snaevar.lokischair";
        public const string Name = "Loki's Chair";
        public const string Version = "0.2.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            new Harmony(Guid).PatchAll();
        }
    }
}
