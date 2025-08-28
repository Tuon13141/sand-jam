using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, 1),  // up
        new Vector2Int(1, 0),  // right
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0)  // left
    };

    private class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public int G; // Cost from start
        public int H; // Heuristic (Manhattan)
        public int F => G + H;

        public Node(Vector2Int pos, Node parent, int g, int h)
        {
            Position = pos;
            Parent = parent;
            G = g;
            H = h;
        }
    }

    public static bool AStarToEndPoints(bool[,] map, Vector2Int start, List<Vector2Int> endPoints, bool[,] shape, out Stack<Vector2Int> path)
    {
        path = new Stack<Vector2Int>();
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        Vector2Int pivot = new Vector2Int(2, 2); // fixed for 5x5 shape

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(start, null, 0, HeuristicToClosestEnd(start, endPoints));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = openList[0];
            foreach (var node in openList)
            {
                if (node.F < current.F || (node.F == current.F && node.H < current.H))
                    current = node;
            }

            // Điều kiện kết thúc: pivot nằm trong endPoints
            if (endPoints.Contains(current.Position))
            {
                while (current != null)
                {
                    path.Push(current.Position);
                    current = current.Parent;
                }
                return true;
            }

            openList.Remove(current);
            closedSet.Add(current.Position);

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = current.Position + dir;

                if (closedSet.Contains(neighborPos))
                    continue;

                if (!CanPlaceShapeAt(map, neighborPos, shape, pivot))
                    continue;

                int tentativeG = current.G + 1;
                Node neighborNode = openList.Find(n => n.Position == neighborPos);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, current, tentativeG, HeuristicToClosestEnd(neighborPos, endPoints));
                    openList.Add(neighborNode);
                }
                else if (tentativeG < neighborNode.G)
                {
                    neighborNode.G = tentativeG;
                    neighborNode.Parent = current;
                }
            }
        }

        //end false, with all pos check
        foreach (var pos in closedSet)
        {
            if (pos != start)
                path.Push(pos);
        }

        return false;
    }

    private static int HeuristicToClosestEnd(Vector2Int pos, List<Vector2Int> ends)
    {
        int min = int.MaxValue;
        foreach (var end in ends)
        {
            int dist = Mathf.Abs(pos.x - end.x) + Mathf.Abs(pos.y - end.y);
            if (dist < min) min = dist;
        }
        return min;
    }

    private static bool CanPlaceShapeAt(bool[,] map, Vector2Int pos, bool[,] shape, Vector2Int pivot)
    {
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);
        int shapeWidth = shape.GetLength(0);
        int shapeHeight = shape.GetLength(1);

        for (int x = 0; x < shapeWidth; x++)
        {
            for (int y = 0; y < shapeHeight; y++)
            {
                if (!shape[x, y]) continue;

                int offsetX = x - pivot.x;
                int offsetY = y - pivot.y;

                int mapX = pos.x + offsetX;
                int mapY = pos.y + offsetY;

                // Pivot bắt buộc nằm trong bản đồ
                if (offsetX == 0 && offsetY == 0)
                {
                    if (mapX < 0 || mapY < 0 || mapX >= mapWidth || mapY >= mapHeight)
                        return false;

                    if (!map[mapX, mapY])
                        return false;
                }
                else
                {
                    // Các phần khác của shape có thể nằm ngoài map, nhưng nếu nằm trong thì phải walkable
                    if (mapX >= 0 && mapY >= 0 && mapX < mapWidth && mapY < mapHeight)
                    {
                        if (!map[mapX, mapY])
                            return false;
                    }
                    // Nếu nằm ngoài map => được phép, bỏ qua
                }
            }
        }

        return true;
    }

}
