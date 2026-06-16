using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public static class Pathfinding
    {
        private class Node
        {
            public Point Position { get; set; }
            public float GCost { get; set; }
            public float HCost { get; set; }
            public float FCost => GCost + HCost;
            public Node Parent { get; set; }
            public int OpenedVersion { get; set; } = 0;
        }

        // 🔒 Делаем версию потокобезопасной для параллельных вызовов
        private static int _version = 0;

        /// <summary>
        /// Асинхронная версия поиска пути. Выполняется в пуле потоков.
        /// </summary>
        public static async Task<List<Vector2>> FindPathAsync(TileMap tileMap, Vector2 startWorld, Vector2 endWorld,
                                                              int unitTileWidth = 1, int unitTileHeight = 1,
                                                              CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FindPath(tileMap, startWorld, endWorld, unitTileWidth, unitTileHeight), cancellationToken);
        }

        /// <summary>
        /// Оригинальная синхронная версия (оставлена для совместимости).
        /// </summary>
        public static List<Vector2> FindPath(TileMap tileMap, Vector2 startWorld, Vector2 endWorld,
                                           int unitTileWidth = 1, int unitTileHeight = 1)
        {
            if (tileMap == null) return new List<Vector2>();

            var startPoint = tileMap.WorldToGridPosition(startWorld);
            var endPoint = tileMap.WorldToGridPosition(endWorld);
            var ignoreTile = startPoint;

            if (!tileMap.IsAreaWalkable(startPoint.X, startPoint.Y, unitTileWidth, unitTileHeight, ignoreTile))
                return new List<Vector2>();

            // 🔒 Потокобезопасное инкрементирование версии
            int currentVersion = Interlocked.Increment(ref _version);

            var openList = new PriorityQueue<Node, float>();
            var cameFrom = new Dictionary<Point, Node>();
            var gScore = new Dictionary<Point, float>();

            var startNode = new Node
            {
                Position = startPoint,
                GCost = 0,
                HCost = Heuristic(startPoint, endPoint),
                OpenedVersion = currentVersion
            };

            openList.Enqueue(startNode, startNode.FCost);
            cameFrom[startPoint] = startNode;
            gScore[startPoint] = 0;

            Node closestNode = startNode;
            float closestH = startNode.HCost;

            while (openList.Count > 0)
            {
                var current = openList.Dequeue();

                if (IsTargetReached(current.Position, endPoint, unitTileWidth, unitTileHeight))
                    return ReconstructPath(tileMap, current);

                if (current.HCost < closestH)
                {
                    closestNode = current;
                    closestH = current.HCost;
                }

                foreach (var neighbor in GetNeighbors(tileMap, current.Position))
                {
                    if (!tileMap.IsAreaWalkable(neighbor.X, neighbor.Y, unitTileWidth, unitTileHeight, ignoreTile))
                        continue;

                    float tentativeGCost = current.GCost +
                        Vector2.DistanceSquared(
                            tileMap.GridToWorldPosition(current.Position.X, current.Position.Y),
                            tileMap.GridToWorldPosition(neighbor.X, neighbor.Y));

                    if (!gScore.TryGetValue(neighbor, out float currentGCost) || tentativeGCost < currentGCost)
                    {
                        var neighborNode = new Node
                        {
                            Position = neighbor,
                            GCost = tentativeGCost,
                            HCost = Heuristic(neighbor, endPoint),
                            Parent = current,
                            OpenedVersion = currentVersion
                        };

                        gScore[neighbor] = tentativeGCost;
                        cameFrom[neighbor] = neighborNode;

                        if (neighborNode.HCost < closestH)
                        {
                            closestNode = neighborNode;
                            closestH = neighborNode.HCost;
                        }

                        openList.Enqueue(neighborNode, neighborNode.FCost);
                    }
                }
            }

            return ReconstructPath(tileMap, closestNode);
        }

        private static bool IsTargetReached(Point unitTopLeft, Point target, int unitW, int unitH) =>
            target.X >= unitTopLeft.X && target.X < unitTopLeft.X + unitW &&
            target.Y >= unitTopLeft.Y && target.Y < unitTopLeft.Y + unitH;

        private static float Heuristic(Point a, Point b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return dx + dy + 0.001f * Math.Min(dx, dy);
        }

        private static List<Point> GetNeighbors(TileMap tileMap, Point pos)
        {
            var neighbors = new List<Point>(4);
            var directions = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };

            foreach (var dir in directions)
            {
                Point n = new Point(pos.X + dir.X, pos.Y + dir.Y);
                if (n.X >= 0 && n.X < tileMap.Width && n.Y >= 0 && n.Y < tileMap.Height)
                    neighbors.Add(n);
            }
            return neighbors;
        }

        private static List<Vector2> ReconstructPath(TileMap tileMap, Node current)
        {
            var path = new List<Vector2>();
            while (current != null)
            {
                path.Add(tileMap.GridToWorldPosition(current.Position.X, current.Position.Y));
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}