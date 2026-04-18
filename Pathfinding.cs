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
        }

        /// <summary>
        /// Находит путь с учётом размера юнита в клетках.
        /// unitTileWidth/Height = количество клеток, которые занимает юнит по ширине/высоте.
        /// </summary>
        public static List<Vector2> FindPath(TileMap tileMap, Vector2 start, Vector2 end, int unitTileWidth = 1, int unitTileHeight = 1)
        {
            var startPoint = tileMap.WorldToGridPosition(start);
            var endPoint = tileMap.WorldToGridPosition(end);

            // Если стартовая позиция уже блокирована размером юнита, путь невозможен
            if (!tileMap.IsAreaWalkable(startPoint.X, startPoint.Y, unitTileWidth, unitTileHeight))
                return new List<Vector2>();

            var startNode = new Node
            {
                Position = startPoint,
                GCost = 0,
                HCost = Heuristic(startPoint, endPoint)
            };

            var openList = new List<Node> { startNode };
            var closedList = new HashSet<Point>();
            Node closestNode = startNode;

            while (openList.Count > 0)
            {
                // Выбираем узел с наименьшим FCost
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

                if (current.HCost < closestNode.HCost)
                    closestNode = current;

                openList.RemoveAt(currentIndex);
                closedList.Add(current.Position);

                // Проверяем, достигли ли цели (цель попадает в прямоугольник юнита)
                if (IsTargetReached(current.Position, endPoint, unitTileWidth, unitTileHeight))
                {
                    return ReconstructPath(tileMap, current);
                }

                // Перебираем соседей (4 направления)
                foreach (var neighbor in GetNeighbors(tileMap, current.Position))
                {
                    if (closedList.Contains(neighbor)) continue;

                    // ГЛАВНОЕ: проверяем, помещается ли весь юнит в соседнюю позицию
                    if (!tileMap.IsAreaWalkable(neighbor.X, neighbor.Y, unitTileWidth, unitTileHeight)) continue;

                    float tentativeGCost = current.GCost + Vector2.Distance(
                        tileMap.GridToWorldPosition(current.Position.X, current.Position.Y),
                        tileMap.GridToWorldPosition(neighbor.X, neighbor.Y));

                    var neighborNode = new Node
                    {
                        Position = neighbor,
                        GCost = tentativeGCost,
                        HCost = Heuristic(neighbor, endPoint)
                    };

                    if (neighborNode.HCost < closestNode.HCost)
                        closestNode = neighborNode;

                    var existingNode = openList.Find(n => n.Position == neighbor);
                    if (existingNode == null)
                    {
                        openList.Add(neighborNode);
                        neighborNode.Parent = current;
                    }
                    else if (tentativeGCost < existingNode.GCost)
                    {
                        existingNode.GCost = tentativeGCost;
                        existingNode.Parent = current;
                    }
                }
            }

            // Если путь до цели не найден, возвращаем ближайшую допустимую точку
            return ReconstructPath(tileMap, closestNode);
        }

        private static bool IsTargetReached(Point current, Point target, int unitW, int unitH)
        {
            // Цель достигнута, если целевая клетка находится внутри прямоугольника юнита
            return current.X <= target.X && target.X < current.X + unitW &&
                   current.Y <= target.Y && target.Y < current.Y + unitH;
        }

        private static float Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static List<Point> GetNeighbors(TileMap tileMap, Point position)
        {
            return new List<Point>
            {
                new Point(position.X + 1, position.Y),
                new Point(position.X - 1, position.Y),
                new Point(position.X, position.Y + 1),
                new Point(position.X, position.Y - 1)
            }.FindAll(p => p.X >= 0 && p.X < tileMap.Width && p.Y >= 0 && p.Y < tileMap.Height);
        }

        private static List<Vector2> ReconstructPath(TileMap tileMap, Node current)
        {
            var path = new List<Vector2>();
            while (current != null)
            {
                // Возвращаем мировую позицию верхнего левого угла узла
                path.Add(tileMap.GridToWorldPosition(current.Position.X, current.Position.Y));
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}