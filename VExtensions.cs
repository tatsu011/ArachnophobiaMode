using BepInEx.Logging;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ArachnophobiaMode
{
    public static class VExtensions
    {
        private static ManualLogSource _log => Plugin.LogInstance;

        public static bool ComparePrefabGuidString(this Entity entity, PrefabGUID comparingvalue)
        {
            try
            {
                Utils.Server.EntityManager.TryGetComponentData<PrefabGUID>(entity, out PrefabGUID componentData);
                return componentData.ToString()!.Equals(comparingvalue.ToString());

            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception e)
            {
                _log.LogError("Couldn't compare component data: " + e.Message);
                return false;
            }

        }

        public static void WithComponentDataC<T>(this Entity entity, ActionRefs<T> action) where T : struct
        {
            Utils.Server.EntityManager.TryGetComponentData<T>(entity, out T componentData);
            action(ref componentData);
            Utils.Server.EntityManager.SetComponentData<T>(entity, componentData);
        }

        public static bool SetEntityPrefab(this Entity entity, PrefabGUID target, PrefabGUID replacement)
        {
            try
            {
                Utils.Client.EntityManager.TryGetComponentData<PrefabGUID>(entity, out PrefabGUID componentData);

            }
            catch (Exception e)
            {
                _log.LogError("Could not swap data");
                return false;
            }
            return true;
        }

        public delegate void ActionRefs<T>(ref T item);
    }
}
