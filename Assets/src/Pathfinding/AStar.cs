using Game.Utils;
using System.Collections.Generic;

namespace Game.Pathfinding
{
    public class AStar<TTarget>
    {
        public delegate double HeuristicFunctionDelegate(PathfindingNode<TTarget> node, PathfindingNode<TTarget> end);
        public delegate Dictionary<PathfindingNode<TTarget>, double> FindNeighborsDelegate(PathfindingNode<TTarget> node);

        public HeuristicFunctionDelegate HeuristicFunction { get; set; }
        public FindNeighborsDelegate FindNeighbors { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="heuristicFunction">Returns estimated cost for reaching end from node</param>
        /// <param name="findNeighbors">Used to find node's neighbors and costs for moving to them. If left as null, PathfindingNode.Neighbors property is used instead.</param>
        public AStar(HeuristicFunctionDelegate heuristicFunction, FindNeighborsDelegate findNeighbors = null)
        {
            HeuristicFunction = heuristicFunction;
            FindNeighbors = findNeighbors;
        }

        private List<PathfindingNode<TTarget>> ReconstructPath(Dictionary<PathfindingNode<TTarget>, PathfindingNode<TTarget>> cameFrom, PathfindingNode<TTarget> current)
        {
            List<PathfindingNode<TTarget>> totalPath = new List<PathfindingNode<TTarget>>();
            totalPath.Add(current);
            while (cameFrom.ContainsKey(current)) {
                current = cameFrom[current];
                totalPath.Add(current);
            }
            return totalPath;
        }

        public List<PathfindingNode<TTarget>> Path(PathfindingNode<TTarget> start, PathfindingNode<TTarget> end)
        {
            List<PathfindingNode<TTarget>> openSet = new List<PathfindingNode<TTarget>>();
            openSet.Add(start);

            Dictionary<PathfindingNode<TTarget>, PathfindingNode<TTarget>> cameFrom = new Dictionary<PathfindingNode<TTarget>, PathfindingNode<TTarget>>();

            Dictionary<PathfindingNode<TTarget>, double> gScore = new Dictionary<PathfindingNode<TTarget>, double>();
            gScore.Add(start, 0.0d);

            Dictionary<PathfindingNode<TTarget>, double> fScore = new Dictionary<PathfindingNode<TTarget>, double>();
            fScore.Add(start, HeuristicFunction(start, end));

            while(openSet.Count != 0) {
                PathfindingNode<TTarget> current = openSet[0];
                if(current.Target.Equals(end.Target)) {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                Dictionary<PathfindingNode<TTarget>, double> neighbors = FindNeighbors != null ? FindNeighbors(current) : current.Neighbors;
                foreach (KeyValuePair<PathfindingNode<TTarget>, double> neighborsAndCost in neighbors) {
                    PathfindingNode<TTarget> neighbor = neighborsAndCost.Key;
                    double cost = neighborsAndCost.Value;
                    double tentativeGScore = gScore[current] + cost;
                    double gScoreNeighbor = gScore.ContainsKey(neighbor) ? gScore[neighbor] : double.MaxValue;
                    if (tentativeGScore < gScoreNeighbor) {
                        DictionaryHelper.Set(cameFrom, neighbor, current);
                        DictionaryHelper.Set(gScore, neighbor, tentativeGScore);
                        DictionaryHelper.Set(fScore, neighbor, tentativeGScore + HeuristicFunction(neighbor, end));
                        if (!openSet.Contains(neighbor)) {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }
    }
}

