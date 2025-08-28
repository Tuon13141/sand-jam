using System.Collections.Generic;
using UnityEngine;


public class LevelSO : ScriptableObject
{
    public int width;
    public int height;
    public List<int> flatMatrix;
    public GameObject mapPrefab;
    public float pixelPerUnit = 1f;
    public float distaceBetweenTiles = 0.1f;
    public float startPositionX = 0f;
    public float startPositionY = 0f;

    public void SaveData(int[,] matrix, GameObject prefab, float pixel, float distance, Vector2 startPosition)
    {
        mapPrefab = prefab;
        width = matrix.GetLength(0);
        height = matrix.GetLength(1);

        flatMatrix = new();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                flatMatrix.Add(matrix[i, j]);
            }
        }
        pixelPerUnit = pixel;
        distaceBetweenTiles = distance;
        startPositionX = startPosition.x;
        startPositionY = startPosition.y;
    }

    public int[,] GetMatrix()
    {
        int[,] matrix = new int[width, height];
        int index = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                matrix[i, j] = flatMatrix[index];
                index++;
            }
        }
        return matrix;
    }
}
