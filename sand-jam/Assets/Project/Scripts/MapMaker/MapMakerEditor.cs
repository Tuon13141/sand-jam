using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class MapMakerEditor : MonoBehaviour
{
    // --- Public Fields for Editor UI ---
    [Header("Original Prefab to Scan")]
    [Tooltip("The 3D prefab to be scanned and converted to a 2D grid.")]
    public GameObject m_MapPrefab;

    [Header("Sprite Element Prefab")]
    [Tooltip("The SpriteElement prefab to be instantiated for each grid cell.")]
    public SpriteElement m_SpriteElementPrefab;

    [Header("Scan Configuration")]
    [Tooltip("Multiplier to determine the resolution of the grid based on the prefab's size.")]
    [SerializeField, Range(10, 200)]
    private int m_ResolutionMultiplier = 20;

    [Header("Color Configuration")]
    [Tooltip("Reference to a ColorManager script that provides color palettes for painting.")]
    public ColorManager colorManager;

    [Header("Painting/Erasing Tools")]
    [Tooltip("Controls whether the tool is in drawing mode (true) or erasing mode (false).")]
    [ShowIf("HasScanned")]
    public bool m_IsDrawingMode = true;

    [Tooltip("The size of the brush used for drawing or erasing.")]
    [ShowIf("HasScanned")]
    [SerializeField, Range(1, 50)]
    private int m_BrushSize = 1;

    [Tooltip("The index of the color to be used for drawing.")]
    [ShowIf("HasScannedAndInDrawingMode")]
    [Dropdown("GetColorIndexes")]
    [SerializeField]
    public int selectedColorIndex = 0;

    private List<int> GetColorIndexes()
    {
        if (colorManager == null) return new List<int>() { 0 };
        List<int> result = new List<int>();
        for (int i = 0; i < colorManager.GetSandColorCount(); i++)
            result.Add(i);
        return result;
    }

    // --- Private Internal State ---
    private int[,] _grid;
    private GameObject _levelRoot;
    private GameObject _spriteHolder;
    private Dictionary<Vector2Int, SpriteElement> _spriteElements;
    private Bounds _currentBounds;
    private float _currentCellSize;
    private int _currentResolution;
    private bool _hasScanned = false;
    private float _pixelPerUnit = 1f;

    private bool HasScanned() => _hasScanned;
    public bool HasScannedAndInDrawingMode() => _hasScanned && m_IsDrawingMode;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    [Button("ScanPrefab")]
    public void ScanPrefab()
    {
        if (m_MapPrefab == null)
        {
            Debug.LogError("⚠️ Please assign the MapPrefab field!");
            return;
        }

        if (m_SpriteElementPrefab == null)
        {
            Debug.LogError("⚠️ Please assign the SpriteElementPrefab field!");
            return;
        }

        ClearAll();

        _levelRoot = new GameObject("Level");
        _levelRoot.transform.position = Vector3.zero;
        GameObject mapObj = (GameObject)PrefabUtility.InstantiatePrefab(m_MapPrefab);
        mapObj.transform.SetParent(_levelRoot.transform);
        mapObj.transform.position = Vector3.zero;
        mapObj.name = "MapPrefab";

        Collider[] existingColliders = mapObj.GetComponentsInChildren<Collider>();
        foreach (Collider col in existingColliders)
        {
            col.enabled = false;
        }

        MeshFilter[] meshFilters = mapObj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in meshFilters)
        {
            if (filter.sharedMesh != null)
            {
                MeshCollider meshCollider = filter.gameObject.AddComponent<MeshCollider>();
                meshCollider.convex = false;
            }
        }

        // Create a parent object to hold all the grid sprites
        _spriteHolder = new GameObject("SpriteHolder");
        _spriteHolder.transform.SetParent(_levelRoot.transform);
        _spriteHolder.transform.position = Vector3.zero;

        _currentBounds = CalculateMeshBounds(mapObj);
        Debug.Log($"Bounds: {_currentBounds}");

        float maxDimension = Mathf.Max(_currentBounds.size.x, _currentBounds.size.y);
        _currentResolution = Mathf.Clamp(Mathf.CeilToInt(maxDimension * m_ResolutionMultiplier), 10, 500);
        _currentCellSize = maxDimension / _currentResolution;

        _currentBounds.Expand(_currentCellSize * 2f);

        // 🔹 Đảm bảo số lẻ
        if (_currentResolution % 2 == 0)
        {
            // Test 2 cạnh: trái và dưới
            Vector3 testLeft = new Vector3(_currentBounds.min.x + 0.5f * _currentCellSize, _currentBounds.center.y, _currentBounds.center.z + _currentBounds.size.z);
            Vector3 testBottom = new Vector3(_currentBounds.center.x, _currentBounds.min.y + 0.5f * _currentCellSize, _currentBounds.center.z + _currentBounds.size.z);

            bool wallNearLeft = PointInsideMeshByRaycast(testLeft, mapObj);
            bool wallNearBottom = PointInsideMeshByRaycast(testBottom, mapObj);

            _currentResolution += 1;
            if (wallNearLeft && !wallNearBottom)
            {
                _currentBounds.Expand(new Vector3(_currentCellSize, 0, 0));
                Debug.Log("📐 Added padding on X axis (left side wall).");
            }
            else
            {
                _currentBounds.Expand(new Vector3(0, _currentCellSize, 0));
                Debug.Log("📐 Added padding on Y axis (bottom side wall).");
            }
        }

        _grid = new int[_currentResolution, _currentResolution];
        _spriteElements = new Dictionary<Vector2Int, SpriteElement>();
        Vector3 center = _currentBounds.center;

        for (int x = 0; x < _currentResolution; x++)
        {
            for (int y = 0; y < _currentResolution; y++)
            {
                Vector3 samplePos = new Vector3(
                    _currentBounds.min.x + (x + 0.5f) * _currentCellSize,
                    _currentBounds.min.y + (y + 0.5f) * _currentCellSize,
                    center.z + _currentBounds.size.z
                );

                bool inside = PointInsideMeshByRaycast(samplePos, mapObj);
                _grid[x, y] = inside ? 1 : 0;

                Vector3 spritePosition = new Vector3(
                    _currentBounds.min.x + (x + 0.5f) * _currentCellSize,
                    _currentBounds.min.y + (y + 0.5f) * _currentCellSize,
                    0
                );

                // Instantiate SpriteElement prefab instead of creating GameObject manually
                SpriteElement spriteElement = (SpriteElement)PrefabUtility.InstantiatePrefab(m_SpriteElementPrefab);
                spriteElement.transform.SetParent(_spriteHolder.transform);
                spriteElement.transform.position = spritePosition;

                // Scale the SpriteElement to fit the grid cell
                SpriteRenderer sr = spriteElement.spriteRenderer;
                if (sr != null && sr.sprite != null)
                {
                    float spriteSize = _currentCellSize / sr.sprite.bounds.size.x;
                    spriteElement.transform.localScale = new Vector3(spriteSize, spriteSize, 1f);
                    _pixelPerUnit = spriteSize;
                }

                // Add collider
                BoxCollider2D collider = spriteElement.gameObject.AddComponent<BoxCollider2D>();
                if (sr != null && sr.sprite != null)
                {
                    collider.size = sr.sprite.bounds.size;
                }

                // Set color based on inside/outside status
                if (sr != null)
                {
                    if (inside)
                        sr.color = new Color(0f, 0f, 0f, 0.8f);
                    else
                        sr.color = new Color(1f, 1f, 1f, 0.3f);
                }

                _spriteElements[new Vector2Int(x, y)] = spriteElement;
            }
        }

        MeshCollider[] tempColliders = mapObj.GetComponentsInChildren<MeshCollider>();
        foreach (MeshCollider col in tempColliders)
        {
            DestroyImmediate(col);
        }

        // Re-enable the original colliders
        foreach (Collider col in existingColliders)
        {
            col.enabled = true;
        }

        _hasScanned = true;
        mapObj.SetActive(false);
        Debug.Log($"✅ ScanPrefab complete! Resolution: {_currentResolution}x{_currentResolution}, Cell size: {_currentCellSize}");
    }

    [Button("ClearAll")]
    public void ClearAll()
    {
        if (_levelRoot != null)
            DestroyImmediate(_levelRoot);

        _grid = null;
        _spriteElements = null;
        _hasScanned = false;
        Debug.Log("🗑️ All objects cleared!");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_hasScanned) return;

        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Alpha1)
        {
            Vector2 mousePosition = e.mousePosition;
            mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;
            Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);

            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if (hit.collider != null)
            {
                Vector3 worldPos = hit.point;
                Vector2Int gridPos = WorldToGridPosition(worldPos);

                if (m_IsDrawingMode)
                {
                    DrawAtPosition(gridPos);
                }
                else
                {
                    EraseAtPosition(gridPos);
                }
                e.Use();
            }
        }
    }

    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - _currentBounds.min.x) / _currentCellSize);
        int y = Mathf.FloorToInt((worldPos.y - _currentBounds.min.y) / _currentCellSize);
        x = Mathf.Clamp(x, 0, _currentResolution - 1);
        y = Mathf.Clamp(y, 0, _currentResolution - 1);
        return new Vector2Int(x, y);
    }

    private void DrawAtPosition(Vector2Int centerPos)
    {
        if (colorManager == null)
        {
            Debug.LogError("ColorManager is not assigned!");
            return;
        }

        int halfSize = Mathf.FloorToInt(m_BrushSize / 2f);
        for (int x = centerPos.x - halfSize; x <= centerPos.x + halfSize; x++)
        {
            for (int y = centerPos.y - halfSize; y <= centerPos.y + halfSize; y++)
            {
                if (x >= 0 && x < _currentResolution && y >= 0 && y < _currentResolution)
                {
                    if (_grid[x, y] != 1)
                    {
                        _grid[x, y] = selectedColorIndex + 2;
                        Vector2Int pos = new Vector2Int(x, y);
                        if (_spriteElements.ContainsKey(pos) && _spriteElements[pos].spriteRenderer != null)
                        {
                            _spriteElements[pos].spriteRenderer.color = colorManager.GetSandColor(selectedColorIndex);
                        }
                    }
                }
            }
        }
    }

    private void EraseAtPosition(Vector2Int centerPos)
    {
        int halfSize = Mathf.FloorToInt(m_BrushSize / 2f);
        for (int x = centerPos.x - halfSize; x <= centerPos.x + halfSize; x++)
        {
            for (int y = centerPos.y - halfSize; y <= centerPos.y + halfSize; y++)
            {
                if (x >= 0 && x < _currentResolution && y >= 0 && y < _currentResolution)
                {
                    if (_grid[x, y] >= 2)
                    {
                        _grid[x, y] = 0; // Reset to empty
                        Vector2Int pos = new Vector2Int(x, y);
                        if (_spriteElements.ContainsKey(pos) && _spriteElements[pos].spriteRenderer != null)
                        {
                            _spriteElements[pos].spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
                        }
                    }
                }
            }
        }
    }

    private Bounds CalculateMeshBounds(GameObject obj)
    {
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0)
            return new Bounds(obj.transform.position, Vector3.one);

        Bounds bounds = filters[0].sharedMesh.bounds;
        bounds = bounds.Transform(filters[0].transform);

        // Encapsulate the bounds of all other mesh filters
        for (int i = 1; i < filters.Length; i++)
        {
            if (filters[i].sharedMesh == null) continue;
            Bounds meshBounds = filters[i].sharedMesh.bounds.Transform(filters[i].transform);
            bounds.Encapsulate(meshBounds);
        }
        return bounds;
    }

    private bool PointInsideMeshByRaycast(Vector3 origin, GameObject obj)
    {
        int hitCount = 0;
        // Cast rays in both forward and backward directions to ensure accuracy
        Ray rayForward = new Ray(origin, Vector3.back);
        Ray rayBackward = new Ray(origin, Vector3.forward);

        RaycastHit[] hitsForward = Physics.RaycastAll(rayForward, 100f);
        RaycastHit[] hitsBackward = Physics.RaycastAll(rayBackward, 100f);

        foreach (var h in hitsForward)
        {
            if (h.collider != null && h.collider.transform.IsChildOf(obj.transform))
                hitCount++;
        }
        foreach (var h in hitsBackward)
        {
            if (h.collider != null && h.collider.transform.IsChildOf(obj.transform))
                hitCount++;
        }

        return hitCount % 2 == 1;
    }

    [Button("SaveLevel")]
    public void SaveLevel()
    {
        if (!_hasScanned || _grid == null)
        {
            Debug.LogError("⚠️ You need to scan and edit before saving!");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Level",
            "NewLevel",
            "asset",
            "Please enter a file name to save the level",
            "Assets/Resources/LevelData"
        );

        if (string.IsNullOrEmpty(path)) return;

        var firstElement = _spriteElements.First();
        var secondElement = _spriteElements.Skip(1).First();
        Vector3 firstPosition = firstElement.Value.transform.localPosition;
        Vector3 secondPosition = secondElement.Value.transform.localPosition;

        float distance = Vector2.Distance(firstPosition, secondPosition);

        LevelSO asset = ScriptableObject.CreateInstance<LevelSO>();
        asset.SaveData(_grid, m_MapPrefab, _pixelPerUnit, distance, firstPosition);

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"✅ Level saved at: {path}");
    }

    [Button("LoadLevel")]
    public void LoadLevel(LevelSO levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("⚠️ LevelSO is null!");
            return;
        }

        if (m_SpriteElementPrefab == null)
        {
            Debug.LogError("⚠️ Please assign the SpriteElementPrefab field!");
            return;
        }

        ClearAll();

        // --- Restore grid ---
        _grid = levelData.GetMatrix();
        m_MapPrefab = levelData.mapPrefab;
        _currentResolution = _grid.GetLength(0);
        _spriteElements = new Dictionary<Vector2Int, SpriteElement>();

        _levelRoot = new GameObject("Level");
        _spriteHolder = new GameObject("SpriteHolder");
        _spriteHolder.transform.SetParent(_levelRoot.transform);

        _currentCellSize = levelData.distaceBetweenTiles;
        _pixelPerUnit = levelData.pixelPerUnit;
        Vector2 startPos = new Vector2(levelData.startPositionX, levelData.startPositionY);

        for (int x = 0; x < _currentResolution; x++)
        {
            for (int y = 0; y < _currentResolution; y++)
            {
                Vector3 spritePosition = new Vector3(
                    startPos.x + (x * _currentCellSize),
                    startPos.y + (y * _currentCellSize),
                    0
                );

                // Instantiate SpriteElement prefab
                SpriteElement spriteElement = (SpriteElement)PrefabUtility.InstantiatePrefab(m_SpriteElementPrefab);
                spriteElement.transform.SetParent(_spriteHolder.transform);
                spriteElement.transform.localPosition = spritePosition;

                // Scale the SpriteElement
                SpriteRenderer sr = spriteElement.spriteRenderer;
                if (sr != null && sr.sprite != null)
                {
                    float spriteSize = _currentCellSize / sr.sprite.bounds.size.x;
                    spriteElement.transform.localScale = new Vector3(spriteSize, spriteSize, 1f);
                }

                // Add collider
                BoxCollider2D collider = spriteElement.gameObject.AddComponent<BoxCollider2D>();
                if (sr != null && sr.sprite != null)
                {
                    collider.size = sr.sprite.bounds.size;
                }

                // --- restore màu ---
                int val = _grid[x, y];
                if (sr != null)
                {
                    if (val == 0)
                        sr.color = new Color(1f, 1f, 1f, 0.3f);
                    else if (val == 1)
                        sr.color = new Color(0f, 0f, 0f, 0.8f);
                    else
                        sr.color = colorManager != null
                            ? colorManager.GetSandColor(val - 2)
                            : Color.magenta;
                }

                _spriteElements[new Vector2Int(x, y)] = spriteElement;
            }
        }

        // --- restore prefab gốc ---
        if (levelData.mapPrefab != null)
        {
            GameObject mapObj = (GameObject)PrefabUtility.InstantiatePrefab(levelData.mapPrefab);
            mapObj.transform.SetParent(_levelRoot.transform);
            mapObj.transform.position = Vector3.zero;
            mapObj.name = "MapPrefab";
            mapObj.SetActive(false);
        }

        _hasScanned = true;
        Debug.Log($"✅ LoadLevel thành công từ {levelData.name}");
    }
}

#endif

public static class BoundsExtensions
{
    public static Bounds Transform(this Bounds b, Transform t)
    {
        var center = t.TransformPoint(b.center);
        Vector3 extents = b.extents;
        Vector3 axisX = t.TransformVector(extents.x, 0, 0);
        Vector3 axisY = t.TransformVector(0, extents.y, 0);
        Vector3 axisZ = t.TransformVector(0, 0, extents.z);
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);
        return new Bounds(center, extents * 2);
    }
}