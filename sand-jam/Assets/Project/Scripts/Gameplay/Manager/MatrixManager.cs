using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class MatrixManager : MonoBehaviour
{
    [SerializeField] LevelSO m_LevelSO;
    [SerializeField] SpriteElement m_SpriteElementPrefab;
    [SerializeField] Transform m_MatrixSpriteParentTransform;     // Parent cho sprite 
    [SerializeField] Transform m_MatrixMapPrefabParentTransform;  // Parent cho LevelSO.mapPrefab 
    [SerializeField] ColorManager m_ColorManager;

    private int[,] _grid;
    private Dictionary<Vector2Int, SpriteElement> _spriteElements;

    [Button("Load Runtime")]
    public void LoadRuntime()
    {
        if (m_LevelSO == null)
        {
            Debug.LogError("⚠️ LevelSO is null!");
            return;
        }
        if (m_SpriteElementPrefab == null)
        {
            Debug.LogError("⚠️ SpriteElement prefab is null!");
            return;
        }
        if (m_MatrixSpriteParentTransform == null)
        {
            Debug.LogError("⚠️ MatrixSpriteParentTransform is null!");
            return;
        }
        if (m_MatrixMapPrefabParentTransform == null)
        {
            Debug.LogError("⚠️ MatrixMapPrefabParentTransform is null!");
            return;
        }

        // --- Clear sprite cũ ---
        foreach (Transform child in m_MatrixSpriteParentTransform)
        {
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        // --- Clear mapPrefab cũ ---
        foreach (Transform child in m_MatrixMapPrefabParentTransform)
        {
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        // --- Load matrix từ SO ---
        _grid = m_LevelSO.GetMatrix();
        _spriteElements = new Dictionary<Vector2Int, SpriteElement>();

        int width = _grid.GetLength(0);
        int height = _grid.GetLength(1);

        float cellSize = m_LevelSO.distaceBetweenTiles;
        Vector2 startPos = new Vector2(m_LevelSO.startPositionX, m_LevelSO.startPositionY);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Tạo cell từ prefab
                SpriteElement element = Instantiate(m_SpriteElementPrefab, m_MatrixSpriteParentTransform);

                // Vị trí local
                Vector3 pos = new Vector3(
                    startPos.x + (x * cellSize),
                    startPos.y + (y * cellSize),
                    0f
                );
                element.transform.localPosition = pos;

                // Scale sprite
                float spriteSize = cellSize / element.spriteRenderer.sprite.bounds.size.x;
                element.transform.localScale = new Vector3(spriteSize, spriteSize, 1f);

                // Gán màu
                int val = _grid[x, y];
                if (val == 0)
                {
                    element.spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f); // ô trống
                }
                else if (val == 1)
                {
                    element.spriteRenderer.color = new Color(0f, 0f, 0f, 0.8f); // tường
                }
                else
                {
                    if (m_ColorManager != null)
                        element.spriteRenderer.color = m_ColorManager.GetSandColor(val - 2);
                    else
                        element.spriteRenderer.color = Color.magenta; // fallback khi chưa có colorManager
                }

                _spriteElements[new Vector2Int(x, y)] = element;
            }
        }

        // --- Restore mapPrefab nếu có ---
        if (m_LevelSO.mapPrefab != null)
        {
            GameObject mapObj = Instantiate(m_LevelSO.mapPrefab, m_MatrixMapPrefabParentTransform);
            mapObj.transform.localPosition = Vector3.zero;
            mapObj.name = "MapPrefab";
            mapObj.SetActive(false); // giống editor load
        }

        Debug.Log($"✅ LoadRuntime thành công từ {m_LevelSO.name}");
    }
}
