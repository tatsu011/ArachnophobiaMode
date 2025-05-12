using Cpp2IL.Core.Extensions;
using HarmonyLib;
using ProjectM;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArachnophobiaMode.Patches
{
    [HarmonyPatch]
    public static class UserDisconnected_Patch
    {
        [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
        [HarmonyPrefix]
        public static void OnUserDisconnected_Patch(ServerBootstrapSystem __instance, NetConnectionId netConnectionId,
            ConnectionStatusChangeReason connectionStatusReason, string extraData)
        {
            // Check if the NetConnectionId exists in the dictionary
            if (!__instance._NetEndPointToApprovedUserIndex.ContainsKey(netConnectionId))
            {
                // If it doesn't exist, return without doing anything further
                return;
            }

            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;
            var user = __instance.EntityManager.GetComponentData<ProjectM.Network.User>(userEntity);
            var player = user.LocalCharacter.GetEntityOnServer();

            // Find the index of the player entity in the list
            int playerIndex = InitializePlayer_Patch.playerEntityIndices.IndexOf(player.Index);

            // If the player entity is found in the list, remove it
            if (playerIndex != -1)
            {
                InitializePlayer_Patch.playerEntityIndices.RemoveAndReturn(playerIndex);
            }
        }
    }
}
