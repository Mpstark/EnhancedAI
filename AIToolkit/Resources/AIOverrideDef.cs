﻿using System.Collections.Generic;
using System.Linq;
using AIToolkit.Selectors;

namespace AIToolkit.Resources
{
    public abstract class AIOverrideDef<T>
    {
        public string Name;
        public List<SelectorValue> Selectors;
        public int Priority;


        public bool Matches(T obj)
        {
            if (Selectors == null || Selectors.Count == 0)
                return true;

            return Selectors.All(selector => selector.Matches(obj));
        }


        public static AIOverrideDef<T> SelectOverride(T obj, IEnumerable<AIOverrideDef<T>> overrides)
        {
            var matching = overrides.Where(o => o.Matches(obj)).ToArray();

            if (matching.Length == 0)
                return null;

            if (matching.Length == 1)
                return matching[0];

            Main.HBSLog?.Log("Had multiple matching AIOverrides, picking one with highest priority:");
            foreach (var overrideDef in matching)
                Main.HBSLog?.Log($"  {overrideDef.Name} Priority: {overrideDef.Priority}");

            // find the one with the highest priority, and then just choose the first
            // one with that priority
            var maxPriority = matching.Max(o => o.Priority);
            return matching.First(o => o.Priority == maxPriority);
        }
    }
}
