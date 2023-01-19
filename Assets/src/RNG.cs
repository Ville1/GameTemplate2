using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class RNG
    {
        public static UnityEngine.Random.State State { get { return UnityEngine.Random.state; } set { UnityEngine.Random.state = value; } }

        public void SetSeed(int seed)
        {
            UnityEngine.Random.InitState(seed);
        }

        /// <summary>
        /// Returns an integer in range: min <= value <= max
        /// </summary>
        public static int Range(int min, int max)
        {
            if(min >= max) {
                CustomLogger.Warning("{InvalidMethodParameters}");
                return min;
            }
            return UnityEngine.Random.Range(min, max + 1);
        }

        /// <summary>
        /// Returns an float in range: min <= value <= max
        /// </summary>
        public static float Range(float min, float max)
        {
            if (min >= max) {
                CustomLogger.Warning("{InvalidMethodParameters}");
                return min;
            }
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// Returns a random item from list
        /// </summary>
        public static TItem Item<TItem>(List<TItem> list)
        {
            if(list.Count == 0) {
                CustomLogger.Warning("{InvalidMethodParameters}");
                return default(TItem);
            }
            return list.Count == 1 ? list[0] : list[Range(0, list.Count - 1)];
        }
    }

    public class WeightedRandomizer<TItem>
    {
        private Dictionary<TItem, int> items;
        private Dictionary<TItem, int> breakpoints;
        private int lastBreakpoint;

        public WeightedRandomizer()
        {
            items = new Dictionary<TItem, int>();
            breakpoints = new Dictionary<TItem, int>();
            lastBreakpoint = 0;
        }

        public void Clear()
        {
            items.Clear();
            breakpoints.Clear();
            lastBreakpoint = 0;
        }

        /// <summary>
        /// Add item to randomizer options. If item was already added, add weights together
        /// </summary>
        public void Add(TItem item, int weight)
        {
            if (items.ContainsKey(item)) {
                //Adjust weight
                items[item] += weight;
                if (items[item] <= 0) {
                    throw new ArgumentException("Weight <= 0");
                }

                //Recalculate breakpoints
                breakpoints.Clear();
                foreach(KeyValuePair<TItem, int> pair in items) {
                    int breakpoint = lastBreakpoint + pair.Value;
                    breakpoints.Add(pair.Key, breakpoint);
                    lastBreakpoint = breakpoint;
                }
            } else {
                //Add a new item
                if (weight <= 0) {
                    throw new ArgumentException("Weight <= 0");
                }
                items.Add(item, weight);

                //Add breakpoint
                int breakpoint = lastBreakpoint + weight;
                breakpoints.Add(item, breakpoint);
                lastBreakpoint = breakpoint;
            }
        }

        public TItem Next()
        {
            if(breakpoints.Count == 0) {
                return default(TItem);//TODO: Log/throw exception?
            }
            if (breakpoints.Count == 1) {
                return breakpoints.Keys.First();
            }
            int random = RNG.Range(1, lastBreakpoint);
            foreach(KeyValuePair<TItem, int> pair in breakpoints) {
                if(random <= pair.Value) {
                    return pair.Key;
                }
            }
            //This should not happen
            throw new Exception(string.Format("Invalid breakpoints: random = {0}, lastBreakpoint = {1}", random, lastBreakpoint));
        }
    }
}
