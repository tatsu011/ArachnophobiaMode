using BepInEx.Logging;
using ProjectM;
using ProjectM.Physics;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ArachnophobiaMode
{
    class Utils
    {
        private static ManualLogSource _log => Plugin.LogInstance;
        private static World? _clientWorld;
        private static World? _serverWorld;

        public static bool IsServer => Application.productName == "VRisingServer";
        public static bool IsClient => Application.productName == "VRising";
        public static World Default => World.DefaultGameObjectInjectionWorld;

        public static World Server
        {
            get
            {
                if (_serverWorld != null && _serverWorld.IsCreated)
                    return _serverWorld;

                _serverWorld = GetWorld("Server")
                    ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
                return _serverWorld;
            }
        }

        public static World Client
        {
            get
            {
                if (_clientWorld != null && _clientWorld.IsCreated)
                    return _clientWorld;

                _clientWorld = GetWorld("Client_0")
                    ?? throw new System.Exception("There is no Client world (yet). Did you install a client mod on the server?");
                return _clientWorld;
            }
        }

        private static World? GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                {
                    _serverWorld = world;
                    return world;
                }
            }

            return null;
        }


    }
}
