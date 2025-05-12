using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace ArachnophobiaMode.Patches
{
    [HarmonyPatch]
    class InitializePlayer_Patch
    {
        internal static List<int> playerEntityIndices = new();

        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
        [HarmonyPostfix]
        public static void OnUserConnected_Patch(ServerBootstrapSystem __instance, NetConnectionId connectionID)
        {
            try
            {
                int userIndex = Utils.Server.GetExistingSystemManaged<ServerBootstrapSystem>()._NetEndPointToApprovedUserIndex[connectionID];
                var serverClient = Utils.Server.GetExistingSystemManaged<ServerBootstrapSystem>()._ApprovedUsersLookup[userIndex];
                Entity userEntity = serverClient.UserEntity;
                User user = Utils.Server.EntityManager.GetComponentData<User>(userEntity);
                Entity player = user.LocalCharacter.GetEntityOnServer();
                playerEntityIndices.Add(player.Index);
            }
            catch(Exception ex)
            {
                Plugin.LogInstance.LogError(ex);
            }
        }

    }
}
