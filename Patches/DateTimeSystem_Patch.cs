

using BepInEx.Logging;
using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using System;
using System.Collections.Generic;
using ProjectM.Scripting;
using UnityEngine;
using System.Linq;

namespace ArachnophobiaMode.Patches
{
    [HarmonyPatch(typeof(DateTimeSystem), nameof(DateTimeSystem.OnUpdate))]
    class DateTimeSystem_Patch
    {
        // ReSharper disable once InconsistentNaming
        private static ManualLogSource _log => Plugin.LogInstance;

        private static DateTime _noUpdateBefore = DateTime.MinValue;

        private static int _totalcullamount;

        private static Entity _queenEntity = Entity.Null;

        private static PrefabGUID _spiderQueen = new(StaticValues.SPIDER_ID_QUEEN);

        private static bool _queenDowned;

        private static float lastDownedTimesecondcheck = 0f;
        private static GameDateTime lastDownedTime = new GameDateTime();
        private static EntityManager entityManager = Utils.Server.EntityManager;

        public static void Prefix(DateTimeSystem instance)
        {
            if (!Utils.IsServer) return; //this should only run on servers.
            if (!Settings.AUTO_CULL.Value) return; //check if the auto-culling setting is disabled.
            if (_noUpdateBefore > DateTime.Now) return; //disable frame-0 updates.

            _noUpdateBefore = DateTime.Now.AddSeconds(Settings.CULL_WAIT_TIME.Value);

            List<int> playerEntities = InitializePlayer_Patch.playerEntityIndices; //on hold until the player patch is done.

            foreach(int playerIndex in playerEntities)
            {
                Entity player = entityManager.GetEntityByEntityIndex(playerIndex);

                if(Settings.CULL_QUEEN.Value)
                {
                    DayNightCycle cycle = Utils.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager.DayNightCycle;
                    GameDateTime now = cycle.GameDateTimeNow;
                    double dayDurationInSecond = cycle.DayDurationInSeconds;
                    double secondsPerInGameHour = dayDurationInSecond / 24;
                    double hoursFortenMins = (9 * 60) / secondsPerInGameHour;

                    if(!_queenDowned ||
                        Time.time - lastDownedTimesecondcheck >= 10f * 60f||
                        now.Year > lastDownedTime.Year ||
                        (now.Year == lastDownedTime.Year && now.Month > lastDownedTime.Month) ||
                        (now.Year == lastDownedTime.Year && now.Month == lastDownedTime.Month && now.Day > lastDownedTime.Day) ||
                        (now.Year == lastDownedTime.Year && now.Month == lastDownedTime.Month && now.Day == lastDownedTime.Day && now.Hour >= Math.Floor(lastDownedTime.Hour + hoursFortenMins)))
                    {
                        _queenEntity = Utils.GetQueen(player, Settings.CULL_RANGE.Value);

                        if(_queenEntity != Entity.Null)
                        {
                            _log.LogMessage("Queen found.");

                            Utils.DownQueen(_queenEntity);

                            _queenDowned = true;
                            lastDownedTimesecondcheck = Time.time;
                            lastDownedTime = cycle.GameDateTimeNow;
                            _queenEntity = Entity.Null;
                        }

                        var spiders = Utils.ClosestSpiders(player, Settings.CULL_RANGE.Value);
                        spiders.RemoveAll(e => e.ComparePrefabGuidString(_spiderQueen)); // *raises eyebrow.
                        int count = spiders.Count;
                        int remaining = count;
                        if (count == 0) continue;
                        foreach(var spider in spiders.TakeWhile(_ => remaining != 0)) //what?
                        {
                            remaining--;
                            if(spider.ComparePrefabGuidString(_spiderQueen))
                            {
                                continue;
                            }

                            KillSpider(spider, player);
                        }

                        if (!Settings.AUTO_LOOT_FROM_CULL.Value) continue;
                        AddCullAmount(count);
                        GiveExtraCullReward(player);

                    }

                }

            }

        }

        private static void KillSpider(Entity spider, Entity player)
        {
            var deathEvent = new DeathEvent
            {
                Died = spider,
                Killer = player,
                Source = player
            };
            var dead = new Dead
            {
                ServerTimeOfDeath = Time.time,
                DestroyAfterDuration = 5f,
                Killer = player,
                KillerSource = player,
                DoNotDestroy = false
            };
            var deathReason = new DeathReason();
#if DEBUG
            Plugin.LogInstance.LogMessage("A spider got killed");
#endif
            DeathUtilities.Kill(Utils.Server.EntityManager, spider, dead, deathEvent, deathReason);
        }

        private static void AddCullAmount(int amount)
        {
            _totalcullamount += amount;
        }

        private static int GetCullAmount()
        {
            return _totalcullamount;
        }

        private static void ResetCullAmount()
        {
            _totalcullamount = 0;
        }


        private static void GiveExtraCullReward(Entity player)
        {
            var threshold = 1; //changing this to any higher than 1 will cause you to most of the time not get anything due to the way the loop works
            var dropAmount = Settings.SILKWORM_GIVE_AMOUNT.Value;
            var silkworm = new PrefabGUID(-11246506);

            var currentCullAmount = GetCullAmount();
            if (currentCullAmount == 0) return;
            var i = 0;
            while (true)
            {
                if (i == 0)
                {
                    i++;
                }
                else
                {
                    i *= 2;
                }

                if (currentCullAmount >= threshold * i) continue;
                if (dropAmount * i > 0)
                {
                    var succeeded = AddItemToInventory(player, silkworm, dropAmount * i);
                    ResetCullAmount();
#if DEBUG
                    if (!succeeded)
                    {
                        _log.LogWarning("Failed to give extra cull reward");
                    }
#endif
                }

                break;
            }
        }

        public static bool AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
        {
            try
            {
                ServerGameManager serverGameManager =
                    Utils.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
                var inventoryResponse = serverGameManager.TryAddInventoryItem(recipient, guid, amount);
#if DEBUG
                _log.LogMessage($"AddItemToInventory: {inventoryResponse.Success}");
#endif
                return inventoryResponse.Success;
            }
            catch (Exception e)
            {
                _log.LogError(e);
            }

            return false;
        }
    }
}
