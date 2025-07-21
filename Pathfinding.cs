
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov
{
    public class Pathfinding
    {
        private class Node
        {
            public Point Position { get; set; }
            public float GCost { get; set; }
            public float HCost { get; set; }
            public float FCost => GCost + HCost;
            public Node Parent { get; set; }
        }

        public static List<Vector2> FindPath(TileMap tileMap, Vector2 start, Vector2 end)
        {
            var startPoint = tileMap.WorldToGridPosition(start);
            var endPoint = tileMap.WorldToGridPosition(end);

            var openList = new List<Node> { new Node { Position = startPoint, GCost = 0, HCost = Heuristic(startPoint, endPoint) } };
            var closedList = new HashSet<Point>();
            var cameFrom = new Dictionary<Point, Node>();

            while (openList.Count > 0)
            {
                var current = openList[0];
                int currentIndex = 0;
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FCost < current.FCost || (openList[i].FCost == current.FCost && openList[i].HCost < current.HCost))
                    {
                        current = openList[i];
                        currentIndex = i;
                    }
                }

                openList.RemoveAt(currentIndex);
                closedList.Add(current.Position);

                if (current.Position == endPoint)
                {
                    return ReconstructPath(tileMap, cameFrom, current);
                }

                foreach (var neighbor in GetNeighbors(tileMap, current.Position))
                {
                    if (closedList.Contains(neighbor)) continue;
                    if (!tileMap.IsWalkable(neighbor.X, neighbor.Y)) continue;

                    float tentativeGCost = current.GCost + Vector2.Distance(tileMap.GridToWorldPosition(current.Position.X, current.Position.Y), tileMap.GridToWorldPosition(neighbor.X, neighbor.Y));

                    var neighborNode = new Node
                    {
                        Position = neighbor,
                        GCost = tentativeGCost,
                        HCost = Heuristic(neighbor, endPoint),
                        Parent = current
                    };

                    var existingNode = openList.Find(n => n.Position == neighbor);
                    if (existingNode == null)
                    {
                        openList.Add(neighborNode);
                        cameFrom[neighbor] = current;
                    }
                    else if (tentativeGCost < existingNode.GCost)
                    {
                        existingNode.GCost = tentativeGCost;
                        existingNode.Parent = current;
                        cameFrom[neighbor] = current;
                    }
                }
            }

            return new List<Vector2>();
        }

        private static float Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static List<Point> GetNeighbors(TileMap tileMap, Point position)
        {
            var neighbors = new List<Point>
            {
                new Point(position.X + 1, position.Y),
                new Point(position.X - 1, position.Y),
                new Point(position.X, position.Y + 1),
                new Point(position.X, position.Y - 1)
            };

            return neighbors.FindAll(p => p.X >= 0 && p.X < tileMap.Width && p.Y >= 0 && p.Y < tileMap.Height);
        }

        private static List<Vector2> ReconstructPath(TileMap tileMap, Dictionary<Point, Node> cameFrom, Node current)
        {
            var path = new List<Vector2>();
            while (current != null)
            {
                path.Add(tileMap.GridToWorldPosition(current.Position.X, current.Position.Y));
                cameFrom.TryGetValue(current.Position, out var parent);
                current = parent;
            }
            path.Reverse();
            return path;
        }
    }
}
