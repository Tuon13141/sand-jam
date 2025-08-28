using System;
using System.Collections.Generic;
using UnityEngine;

public class MatrixConst
{
    public static readonly Vector2Int DEFAULT_GRID_SIZE = new Vector2Int(3, 3);
    public static readonly Vector2Int DEFAULT_CELL_SIZE = new Vector2Int(64, 20);
}

[System.Serializable]
public sealed class Matrix<T>
{
    [System.Serializable]
    public class MatrixRow<T1>
    {
        public T1[] row;

        public MatrixRow(int size)
        {
            row = new T1[size];
        }
    }

    public Matrix()
    {
        size = MatrixConst.DEFAULT_GRID_SIZE;
        cellSize = MatrixConst.DEFAULT_CELL_SIZE;
    }

    public Matrix(Vector2Int size)
    {
        this.size = size;
        cellSize = MatrixConst.DEFAULT_CELL_SIZE;
    }

    public Vector2Int size
    {
        get
        {
            return new Vector2Int(cells.Length, cells[0].row.Length);
        }

        set
        {
            cells = new MatrixRow<T>[value.x];
            for (int x = 0; x < value.x; x++)
            {
                var matrixRow = new MatrixRow<T>(value.y);
                for (int y = 0; y < value.y; y++)
                {
                    matrixRow.row[y] = default(T);
                }

                cells[x] = matrixRow;
            }
        }
    }

    public Vector2Int cellSize;

    public MatrixRow<T>[] cells;

    public T this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= cells.Length || y < 0 || y >= cells[x].row.Length)
            {
                //Debug.LogError("Index was outside the bounds of the matrix.");
                return default;
            }

            return cells[x].row[y];
        }
        set
        {
            if (x < 0 || x >= cells.Length || y < 0 || y >= cells[x].row.Length)
            {
                //Debug.LogError("Index was outside the bounds of the matrix.");
                return;
            }

            cells[x].row[y] = value;
        }
    }

    public T[,] ToAray2D()
    {
        var array = new T[size.x, size.y];
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                array[x, y] = cells[x].row[y];
            }
        }

        return array;
    }

    public Matrix<T> GetArrayClockwise90()
    {
        int rows = size.x;
        int cols = size.y;

        // New array with swapped dimensions
        Matrix<T> rotatedArray = new Matrix<T>(new Vector2Int(cols, rows));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // The rotation formula for 90° clockwise:
                rotatedArray[j, rows - 1 - i] = cells[i].row[j];
            }
        }

        return rotatedArray;
    }

    public Matrix<T> GetArrayClockwise180()
    {
        int rows = size.x;
        int cols = size.y;

        // Same dimensions as original array
        Matrix<T> rotatedArray = new Matrix<T>(new Vector2Int(rows, cols));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // The rotation formula for 180°:
                rotatedArray[rows - 1 - i, cols - 1 - j] = cells[i].row[j];
            }
        }

        return rotatedArray;
    }

    public Matrix<T> GetArrayClockwise270()
    {
        int rows = size.x;
        int cols = size.y;

        // New array with swapped dimensions (same as 90° but different formula)
        Matrix<T> rotatedArray = new Matrix<T>(new Vector2Int(cols, rows));

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // The rotation formula for 270° clockwise (or 90° counterclockwise):
                rotatedArray[cols - 1 - j, i] = cells[i].row[j];
            }
        }

        return rotatedArray;
    }
}
