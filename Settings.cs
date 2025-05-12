using BepInEx.Configuration;

namespace ArachnophobiaMode
{
    public static class Settings
    {
        public static ConfigEntry<bool> AUTO_CULL { get; private set; }
        public static ConfigEntry<bool> AUTO_LOOT_FROM_CULL { get; private set; }
        public static ConfigEntry<bool> CULL_QUEEN { get; private set; }
        public static ConfigEntry<double> CULL_WAIT_TIME { get; private set; }
        public static ConfigEntry<float> CULL_RANGE { get; private set; }
        public static ConfigEntry<int> SILKWORM_GIVE_AMOUNT { get; private set; }

        internal static void Initialize(ConfigFile config)
        {
            AUTO_CULL = config.Bind<bool>("Server", "enableCulling", true, "Enable culling of spiders");
            AUTO_LOOT_FROM_CULL = config.Bind<bool>("Server", "enableExtraCullReward", false, "Enables the extra cull reward of silkworms");
            CULL_QUEEN = config.Bind<bool>("Server", "enableQueenCull", true, "Enable culling of Ungora The Spider Queen VBlood boss (this will turn off her aggro and she will die in one hit))");
            CULL_WAIT_TIME = config.Bind<double>("Server", "cullWaitTime", 2, "Time in seconds to wait before culling spiders again");
            CULL_RANGE = config.Bind<float>("Server", "cullRange", 50f, "Range to check for spiders to cull (5=1tile)");
            SILKWORM_GIVE_AMOUNT = config.Bind<int>("Server", "silkwormGiveAmount", 1, "Amount of silkworms to for each spider");
        }


    }
}
