using System;
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
            public int OpenedVersion { get; set; } = 0; // для оптимизации без очистки списков
        }

        private static int _version = 0;

        /// <summary>
        /// Находит путь с учётом размера юнита в тайлах.
        /// Поддерживает юнитов больше 1x1 клетки.
        /// При недостижимой цели возвращает путь к ближайшей достижимой позиции.
        /// </summary>
        public static List<Vector2> FindPath(TileMap tileMap, Vector2 startWorld, Vector2 endWorld,
                                           int unitTileWidth = 1, int unitTileHeight = 1)
        {
            if (tileMap == null) return new List<Vector2>();

            var startPoint = tileMap.WorldToGridPosition(startWorld);
            var endPoint = tileMap.WorldToGridPosition(endWorld);

            // Игнорируем текущую позицию юнита при проверках проходимости
            var ignoreTile = startPoint;

            // Проверяем, может ли юнит вообще стоять на стартовой позиции
            if (!tileMap.IsAreaWalkable(startPoint.X, startPoint.Y, unitTileWidth, unitTileHeight, ignoreTile))
                return new List<Vector2>();

            _version++;

            var openList = new PriorityQueue<Node, float>();
            var cameFrom = new Dictionary<Point, Node>();
            var gScore = new Dictionary<Point, float>();

            var startNode = new Node
            {
                Position = startPoint,
                GCost = 0,
                HCost = Heuristic(startPoint, endPoint),
                OpenedVersion = _version
            };

            openList.Enqueue(startNode, startNode.FCost);
            cameFrom[startPoint] = startNode;
            gScore[startPoint] = 0;

            Node closestNode = startNode;
            float closestH = startNode.HCost;

            while (openList.Count > 0)
            {
                var current = openList.Dequeue();

                // Если мы достаточно близко к цели (цель попадает в тело юнита)
                if (IsTargetReached(current.Position, endPoint, unitTileWidth, unitTileHeight))
                {
                    return ReconstructPath(tileMap, current);
                }

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
                            OpenedVersion = _version
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

            // Если точная цель недостижима — идём к ближайшей достижимой точке
            return ReconstructPath(tileMap, closestNode);
        }

        private static bool IsTargetReached(Point unitTopLeft, Point target, int unitW, int unitH)
        {
            return target.X >= unitTopLeft.X && target.X < unitTopLeft.X + unitW &&
                   target.Y >= unitTopLeft.Y && target.Y < unitTopLeft.Y + unitH;
        }

        private static float Heuristic(Point a, Point b)
        {
            // Манхэттен + небольшая поправка для диагоналей (лучше для 4-направленного поиска)
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return dx + dy + 0.001f * Math.Min(dx, dy);
        }

        private static List<Point> GetNeighbors(TileMap tileMap, Point pos)
        {
            var neighbors = new List<Point>(4);

            // Можно добавить диагонали позже, если нужно (сейчас только 4 направления)
            var directions = new[]
            {
                new Point(1, 0), new Point(-1, 0),
                new Point(0, 1), new Point(0, -1)
            };

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
                // Возвращаем мировую позицию верхнего-левого угла занимаемой области юнита
                path.Add(tileMap.GridToWorldPosition(current.Position.X, current.Position.Y));
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}