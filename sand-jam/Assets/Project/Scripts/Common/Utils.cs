using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    /// <summary>
	/// Returns world space position at a given viewport coordinate for a given depth.
	/// </summary>
	public static Vector3 ViewportToWorldPlanePoint(Camera theCamera, float zDepth, Vector2 viewportCord)
    {
        Vector2 angles = ViewportPointToAngle(theCamera, viewportCord);
        float xOffset = Mathf.Tan(angles.x) * zDepth;
        float yOffset = Mathf.Tan(angles.y) * zDepth;
        Vector3 cameraPlanePosition = new Vector3(xOffset, yOffset, zDepth);
        cameraPlanePosition = theCamera.transform.TransformPoint(cameraPlanePosition);
        return cameraPlanePosition;
    }

    public static Vector3 ScreenToWorldPlanePoint(Camera camera, float zDepth, Vector3 screenCoord)
    {
        var point = Camera.main.ScreenToViewportPoint(screenCoord);
        return ViewportToWorldPlanePoint(camera, zDepth, point);
    }

    /// <summary>
    /// Returns X and Y frustum angle for the given camera representing the given viewport space coordinate.
    /// </summary>
    public static Vector2 ViewportPointToAngle(Camera cam, Vector2 ViewportCord)
    {
        float adjustedAngle = AngleProportion(cam.fieldOfView / 2, cam.aspect) * 2;
        float xProportion = ((ViewportCord.x - .5f) / .5f);
        float yProportion = ((ViewportCord.y - .5f) / .5f);
        float xAngle = AngleProportion(adjustedAngle / 2, xProportion) * Mathf.Deg2Rad;
        float yAngle = AngleProportion(cam.fieldOfView / 2, yProportion) * Mathf.Deg2Rad;
        return new UnityEngine.Vector2(xAngle, yAngle);
    }

    /// <summary>
    /// Distance between the camera and a plane parallel to the viewport that passes through a given point.
    /// </summary>
    public static float CameraToPointDepth(Camera cam, Vector3 point)
    {
        Vector3 localPosition = cam.transform.InverseTransformPoint(point);
        return localPosition.z;
    }

    public static float AngleProportion(float angle, float proportion)
    {
        float oppisite = Mathf.Tan(angle * Mathf.Deg2Rad);
        float oppisiteProportion = oppisite * proportion;
        return Mathf.Atan(oppisiteProportion) * Mathf.Rad2Deg;
    }

    //check fdf
    public static bool CanMoveToBusStopCells(bool[,] levelMap, Vector2Int humanIndex, out Stack<Vector2Int> validIndexes)
    {
        if (humanIndex.y == levelMap.GetLength(1) - 1 && humanIndex.x >= 0 && humanIndex.x < levelMap.GetLength(0))// Nếu ở đầu hàng
        {
            validIndexes = new Stack<Vector2Int>();
            return true;
        }

        List<Vector2Int> checkedNonValidCells = new List<Vector2Int>();
        validIndexes = new Stack<Vector2Int>();
        return CheckCells(levelMap, humanIndex, ref checkedNonValidCells, ref validIndexes, ref humanIndex);
    }

    public static bool CanMoveToBusStopCellsAStar(bool[,] levelMap, Vector2Int humanIndex, out Stack<Vector2Int> validIndexes)
    {
        if (humanIndex.y == levelMap.GetLength(1) - 1 && humanIndex.x >= 0 && humanIndex.x < levelMap.GetLength(0))// Nếu ở đầu hàng
        {
            validIndexes = new Stack<Vector2Int>();
            return true;
        }

        //var path = AStarPathfinding.GeneratePathSync(currentX, currentY, randomX, randomY, walkableMap);

        //if (path.Length != 0)
        //{
        //    path_index = 0;
        //    target = DemoGrid.Instance.cordinateToWorldSpace(path[path_index].Item1, path[path_index].Item2);
        //    break;
        //}

        validIndexes = validIndexes = new Stack<Vector2Int>();
        return false;
    }

    static bool CheckCells(bool[,] levelMap, Vector2Int index,
                           ref List<Vector2Int> checkedNonValidCells,
                           ref Stack<Vector2Int> validIndexes,
                           ref Vector2Int startIndex)
    {
        if (validIndexes.Contains(index)) //điều kiện dừng khi đệ quy
        {
            return false;
        }
        if (index != startIndex)
        {
            if (checkedNonValidCells.Contains(index)) return false;

            if (index.y < 0 || index.y >= levelMap.GetLength(1) ||
                   index.x < 0 || index.x >= levelMap.GetLength(0) ||
                   !levelMap[index.x, index.y])
            {
                if (!checkedNonValidCells.Contains(index))
                    checkedNonValidCells.Add(index);
                return false;
            }
            validIndexes.Push(index);
        }

        if (index.y == levelMap.GetLength(1) - 1 && index.x >= 0 && index.x < levelMap.GetLength(0))
        {
            if (levelMap[index.x, index.y]) return true;
        }

        if (CheckCells(levelMap, index + Vector2Int.up, ref checkedNonValidCells,
                ref validIndexes, ref startIndex) ||
            CheckCells(levelMap, index + Vector2Int.left, ref checkedNonValidCells,
                ref validIndexes, ref startIndex) ||
            CheckCells(levelMap, index + Vector2Int.right, ref checkedNonValidCells,
                ref validIndexes, ref startIndex) ||
            CheckCells(levelMap, index + Vector2Int.down, ref checkedNonValidCells,
                ref validIndexes, ref startIndex))
        {
            return true;
        }
        else
        {
            if (validIndexes.Contains(index))
                checkedNonValidCells.Add(validIndexes.Pop());
            return false;
        }
    }
}