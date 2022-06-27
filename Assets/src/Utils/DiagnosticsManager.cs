using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game.Utils
{
    public class DiagnosticsManager
    {
        public enum Tag { Saving, Loading, Saves }

        public static bool IsRunning { get { return loggedTags != null; } }

        private static List<Tag> loggedTags = null;
        private static Dictionary<string, Stopwatch> watches = new Dictionary<string, Stopwatch>();
        private static Dictionary<string, long> totals = new Dictionary<string, long>();

        public static void Start(List<Tag> tags = null)
        {
            if(tags == null || tags.Count == 0) {
                //Log all
                loggedTags = new List<Tag>();
                foreach(Tag tag in Enum.GetValues(typeof(Tag))) {
                    loggedTags.Add(tag);
                }
            } else {
                loggedTags = tags;
            }
            watches.Clear();
            totals.Clear();
        }

        public static Dictionary<string, long> End()
        {
            loggedTags = null;
            return totals;
        }

        public static void StartSegment(LString name, Tag tag)
        {
            StartSegment(name, new List<Tag>() { tag });
        }

        public static void StartSegment(LString name, params Tag[] tags)
        {
            StartSegment(name, tags.ToList());
        }

        public static void StartSegment(LString name, List<Tag> tags)
        {
            if(!IsRunning || !tags.Any(tag => loggedTags.Contains(tag))) {
                //Not running or this segment should not be logged
                return;
            }
            if (watches.ContainsKey(name)) {
                //This segment has already been started
                CustomLogger.Warning("DiagnosticsSegmentAlreadyStarted", name.ToString());
                return;
            }
            watches.Add(name, Stopwatch.StartNew());
        }

        public static void EndSegment(LString name)
        {
            if(!IsRunning) {
                return;
            }
            if (!watches.ContainsKey(name)) {
                //This segment has not been started
                CustomLogger.Warning("DiagnosticsSegmentNotStarted", name.ToString());
                return;
            }
            watches[name].Stop();
            if (totals.ContainsKey(name)) {
                totals[name] += watches[name].ElapsedMilliseconds;
            } else {
                totals.Add(name, watches[name].ElapsedMilliseconds);
            }
            watches.Remove(name);
        }
    }
}
