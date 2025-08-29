using NaughtyAttributes;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#if UNITY_EDITOR
#endif

public class MatrixManager : MonoBehaviour
{
    [SerializeField] LevelSO m_LevelSO;
    [SerializeField] SpriteElement m_SpriteElementPrefab;
    [SerializeField] Transform m_MatrixSpriteParentTransform;
    [SerializeField] Transform m_MatrixMapPrefabParentTransform;
    [SerializeField] ColorManager m_ColorManager;

    [Space(10)]
    [Header("Debug Matrix")]
    [SerializeField, TextArea(5, 20)]
    private string matrixDebugText;

    private int[,] _matrixData;
    private Dictionary<Vector2Int, SpriteElement> _spriteElements = new Dictionary<Vector2Int, SpriteElement>();

    public List<SpriteElement> wallList = new List<SpriteElement>();

    // --- Wall index bookkeeping (song song với wallList) ---
    // _wallBaseIndices: vị trí gốc ngay sau LoadRuntimeJob (dùng cho xoay absolute)
    // _wallIndices: vị trí hiện thời của từng wall (dùng cho xoay relative/tích lũy)
    private readonly List<Vector2Int> _wallBaseIndices = new List<Vector2Int>();
    private readonly List<Vector2Int> _wallIndices = new List<Vector2Int>();

    // Rotation
    [Header("Rotation Debug")]
    [SerializeField, Range(-1080f, 1080f)]
    private float m_RotateAngle = 0f; // góc nhập trên inspector (độ)

    // --- Buttons / API ---

    [Button("Load Runtime Job")]
    public void LoadRuntimeJob()
    {
        // Clear state
        wallList.Clear();
        _wallBaseIndices.Clear();
        _wallIndices.Clear();

        if (m_LevelSO == null || m_SpriteElementPrefab == null ||
            m_MatrixSpriteParentTransform == null || m_MatrixMapPrefabParentTransform == null)
        {
            Debug.LogError("⚠️ Missing references in MatrixManager!");
            return;
        }

        // --- Release / Clear existing sprite elements via pooling ---
        foreach (SpriteElement child in _spriteElements.Values)
        {
            if (child != null)
                PoolingManager.instance.Release(child);
        }
        _spriteElements.Clear();

        // --- Clear mapPrefab parent children ---
        foreach (Transform child in m_MatrixMapPrefabParentTransform)
        {
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        // --- Load matrix từ SO ---
        _matrixData = m_LevelSO.GetMatrix();
        _spriteElements = new Dictionary<Vector2Int, SpriteElement>();

        // --- Debug matrix ra TextArea ---
        UpdateMatrixDebugText();

        int width = _matrixData.GetLength(0);
        int height = _matrixData.GetLength(1);
        int total = width * height;

        NativeArray<CellData> results = new NativeArray<CellData>(total, Allocator.TempJob);
        NativeArray<int> gridNative = new NativeArray<int>(_matrixData.Length, Allocator.TempJob);

        // copy grid vào NativeArray (theo cùng thứ tự mà BuildMatrixJob mong)
        int idx = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                gridNative[idx++] = _matrixData[x, y];

        // setup job (giữ nguyên từ project của bạn)
        var job = new BuildMatrixJob
        {
            width = width,
            height = height,
            startPos = new Vector2(m_LevelSO.startPositionX, m_LevelSO.startPositionY),
            distanceBetweenTiles = m_LevelSO.distaceBetweenTiles,
            pixelPerUnit = m_LevelSO.pixelPerUnit,
            grid = gridNative,
            results = results
        };

        // chạy job
        JobHandle handle = job.Schedule(total, 64);
        handle.Complete();

        // --- Spawn background (full map) ---
        SpriteElement background = PoolingManager.instance.Get(typeof(SpriteElement)) as SpriteElement;
        background.transform.SetParent(m_MatrixSpriteParentTransform);
        background.transform.localPosition = new Vector3(
            m_LevelSO.startPositionX + (width * m_LevelSO.distaceBetweenTiles) / 2f - m_LevelSO.distaceBetweenTiles / 2f,
            m_LevelSO.startPositionY + (height * m_LevelSO.distaceBetweenTiles) / 2f - m_LevelSO.distaceBetweenTiles / 2f,
            0f
        );
        float fullWidth = width * m_LevelSO.distaceBetweenTiles;
        float fullHeight = height * m_LevelSO.distaceBetweenTiles;
        background.transform.localScale = new Vector3(fullWidth, fullHeight, 1f);
        background.spriteRenderer.color = Color.white;
        background.spriteRenderer.sortingOrder = -1;
        background.name = "Background";

        // --- Apply kết quả (spawn chỉ ô >= 1) ---
        idx = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellData data = results[idx++];

                if (data.val < 1) continue; // bỏ qua ô trống

                SpriteElement element = PoolingManager.instance.Get(typeof(SpriteElement)) as SpriteElement;
                element.gridPosition = new Vector2Int(x, y);

                element.transform.SetParent(m_MatrixSpriteParentTransform);
                element.transform.localPosition = data.pos;
                element.transform.localScale = data.scale;

                // màu
                if (data.val == 1)
                {
                    element.spriteRenderer.color = new Color(0f, 0f, 0f, 0.8f); // tường
                    wallList.Add(element);

                    // ghi lại chỉ số lưới tương ứng (x,y) vào cả base và current
                    _wallBaseIndices.Add(new Vector2Int(x, y));
                    _wallIndices.Add(new Vector2Int(x, y));
                }
                else
                {
                    element.spriteRenderer.color = (m_ColorManager != null) ?
                        m_ColorManager.GetSandColor(data.val - 2) : Color.magenta;
                }

                // lưu mapping từ grid -> element
                _spriteElements[new Vector2Int(x, y)] = element;
            }
        }

        // --- Restore mapPrefab nếu có ---
        if (m_LevelSO.mapPrefab != null)
        {
            GameObject mapObj = Instantiate(m_LevelSO.mapPrefab, m_MatrixMapPrefabParentTransform);
            mapObj.transform.localPosition = Vector3.zero;
            mapObj.name = "MapPrefab";
            mapObj.SetActive(false);
        }

        gridNative.Dispose();
        results.Dispose();

        Debug.Log($"✅ LoadRuntimeJob hoàn tất ({total} cells, nền + {_spriteElements.Count} ô >= 1) từ {m_LevelSO.name}");
    }

    private void UpdateMatrixDebugText()
    {
        if (_matrixData == null)
            return;

        int width = _matrixData.GetLength(0);
        int height = _matrixData.GetLength(1);

        StringBuilder sb = new StringBuilder();

        // Tạo chuỗi đơn giản với các giá trị cách nhau bằng dấu phẩy
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(_matrixData[x, y]);
                if (x < width - 1) sb.Append(", ");
            }
            if (y > 0) sb.AppendLine();
        }

        matrixDebugText = sb.ToString();
    }

    [Button("Update Matrix Debug Text")]
    private void UpdateMatrixDebugTextButton()
    {
        if (m_LevelSO != null)
        {
            _matrixData = m_LevelSO.GetMatrix();
            UpdateMatrixDebugText();
        }
    }

    [Button("Rotate Walls (Job)")]
    public void RotateWalls()
    {
        if (wallList.Count == 0)
        {
            Debug.LogWarning("⚠️ Chưa có wallList");
            return;
        }

        int n = m_LevelSO.width; // giả sử n x n, n lẻ
        float cx = (n - 1) / 2f;
        float cy = (n - 1) / 2f;

        float rad = m_RotateAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        // chuẩn bị dữ liệu job
        NativeArray<Vector2> indices = new NativeArray<Vector2>(wallList.Count, Allocator.TempJob);
        NativeArray<Vector3> results = new NativeArray<Vector3>(wallList.Count, Allocator.TempJob);

        for (int i = 0; i < wallList.Count; i++)
        {
            // giả sử bạn đã lưu index (x,y) cho từng wall khi spawn
            indices[i] = wallList[i].gridPosition;
        }

        var job = new RotateWallsJob
        {
            wallIndices = indices,
            results = results,
            cx = cx,
            cy = cy,
            cos = cos,
            sin = sin,
            startX = m_LevelSO.startPositionX,
            startY = m_LevelSO.startPositionY,
            tileDist = m_LevelSO.distaceBetweenTiles,
        };

        JobHandle handle = job.Schedule(wallList.Count, 64);
        handle.Complete();

        // áp kết quả
        for (int i = 0; i < wallList.Count; i++)
        {
            wallList[i].transform.localPosition = results[i];
        }

        indices.Dispose();
        results.Dispose();

        Debug.Log($"✅ Đã xoay {wallList.Count} walls theo góc {m_RotateAngle}°");
    }
}