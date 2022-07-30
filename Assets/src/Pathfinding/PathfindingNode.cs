using System.Collections.Generic;

namespace Game.Pathfinding
{
    public class PathfindingNode<TTarget>
    {
        public TTarget Target { get; set; }
        /// <summary>
        /// Neighbors and costs to get to them
        /// </summary>
        public Dictionary<PathfindingNode<TTarget>, double> Neighbors { get; set; }
    }
}
