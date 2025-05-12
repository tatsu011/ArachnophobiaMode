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


        private static NativeArray<Entity> GetSpiders()
        {
            var spiderQuery = Server.EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<Team>(),
                ComponentType.ReadOnly<AiMoveSpeeds>(),
                ComponentType.ReadOnly<AiMove_Server>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ServantConvertable>(), 
                    ComponentType.ReadOnly<BlueprintData>(), 
                    ComponentType.ReadOnly<PhysicsRubble>(), 
                    ComponentType.ReadOnly<Dead>(), 
                    ComponentType.ReadOnly<DestroyTag>()
                }
            });

            return spiderQuery.ToEntityArray(Allocator.Temp);
        }

        internal static List<Entity> ClosestSpiders(Entity e, float radius, int team = 21)
        {
            NativeArray<Entity> spiders = GetSpiders();
            List<Entity> results = new();

            if (Server.EntityManager.TryGetComponentData<LocalToWorld>(e, out var localToWorld))
            {
                var origin = localToWorld.Position;
                foreach (Entity spider in spiders)
                {
                    EntityManager em = Server.EntityManager;
                    float3 position = em.GetComponentData<LocalToWorld>(spider).Position;
                    float distance = Vector3.Distance(origin, position);

                    if(!em.HasComponent<Team>(spider))
                    {
                        Plugin.LogInstance.LogMessage("found irrelevant entity from query.");
                        continue;
                    }
                    if(em.GetComponentData<Team>(spider).FactionIndex == team && distance < radius)
                    {
                        Plugin.LogInstance.LogMessage($"Spider found that was too close at: x {position.x} y {position.y}, z {position.z}");
                        results.Add(spider);
                    }
                }
            }

            return results;
        }

        internal static Entity GetQueen(Entity player, float range)
        {
            var spiderQueen = new PrefabGUID(-548489519);
            var spiders = ClosestSpiders(player, range);
            var count = spiders.Count;
            var remaining = count;

            foreach (var spider in spiders.TakeWhile(_ => remaining != 0))
            {
                var isQueen = spider.ComparePrefabGuidString(spiderQueen);
                if (isQueen)
                {
                    return spider;
                }

                remaining--;
            }
            return Entity.Null;
        }

        internal static bool DownQueen(Entity queen)
        {
            if (queen == Entity.Null)
            {
                return false;
            }

            queen.WithComponentDataC((ref Health h) =>
            {
                h.Value = 0.1f;
                h.MaxRecoveryHealth = 0.1f;
                h.MaxHealth._Value = 0.1f;
            });

            queen.WithComponentDataC((ref AggroConsumer ac) =>
            {
                ac.Active._Value = false;
            });

            queen.WithComponentDataC((ref UnitLevel lvl) => { lvl.Level = new ModifiableInt(1); });

            queen.WithComponentDataC((ref Vision vs) => { vs.Range = new ModifiableFloat(0); });

            queen.WithComponentDataC((ref UnitStats us) =>
            {
                us.PhysicalPower = new ModifiableFloat(0);
                us.PassiveHealthRegen = new ModifiableFloat(0);
                us.HealthRecovery = new ModifiableFloat(0);
                us.SiegePower = new ModifiableFloat(0);
                us.SpellPower = new ModifiableFloat(0);
            });

            // queen.WithComponentDataC((ref Script_ApplyBuffUnderHealthThreshold_DataServer abuhtds) =>
            // {
            //     abuhtds.HealthFactor = new ModifiableFloat(0.1f);
            //     abuhtds.ThresholdMet = false;
            // });

#if DEBUG
            Plugin.LogInstance.LogMessage("Queen got downed");
#endif
            return true;
        }

    }
}
