using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
struct BuildMatrixJob : IJobParallelFor
{
    [ReadOnly] public int width;
    [ReadOnly] public int height;
    [ReadOnly] public float distanceBetweenTiles;
    [ReadOnly] public Vector2 startPos;
    [ReadOnly] public float pixelPerUnit;

    [ReadOnly] public NativeArray<int> grid;
    [WriteOnly] public NativeArray<CellData> results;

    public void Execute(int index)
    {
        int x = index / height;
        int y = index % height;

        int val = grid[index];

        Vector3 pos = new Vector3(
            startPos.x + (x * distanceBetweenTiles),
            startPos.y + (y * distanceBetweenTiles),
            0f
        );

        // scale dựa vào pixelPerUnit
        float scaleValue = pixelPerUnit;
        Vector3 scale = new Vector3(scaleValue, scaleValue, pixelPerUnit);

        results[index] = new CellData
        {
            pos = pos,
            scale = scale,
            val = val
        };
    }
}

struct CellData
{
    public Vector3 pos;
    public Vector3 scale;
    public int val;
}

[BurstCompile]
struct RotateWallsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector2> wallIndices;
    public NativeArray<Vector3> results;

    public float cx, cy;
    public float cos, sin;
    public float startX, startY, tileDist;

    public void Execute(int index)
    {
        Vector2 p = wallIndices[index];

        float dx = p.x - cx;
        float dy = p.y - cy;

        float rx = dx * cos - dy * sin;
        float ry = dx * sin + dy * cos;

        float newX = startX + (rx + cx) * tileDist;
        float newY = startY + (ry + cy) * tileDist;

        results[index] = new Vector3(newX, newY, 0f);
    }
}
