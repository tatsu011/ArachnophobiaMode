using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArachnophobiaMode
{
    class Commands
    {
        public static bool Enabled { get; private set; }

        private static ManualLogSource _log => Plugin.LogInstance;

        static Commands()
        {
            Enabled = IL2CPPChainloader.Instance.Plugins.TryGetValue("gg.deca.VampireCommandFramework", out var info);
            if (Enabled) _log.LogInfo($"VCF Version: {info.Metadata.Version}");
        }
    }
}
