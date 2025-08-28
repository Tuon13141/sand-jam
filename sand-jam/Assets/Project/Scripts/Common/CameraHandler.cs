using UnityEngine;
using System.Collections;

public class CameraHandler : Kit.Common.Singleton<CameraHandler>
{
    [SerializeField] Camera m_Camera;
    [SerializeField] Vector2Int targetSize;

    public Transform topObject;
    public Transform bottomObject;

    public static float topOffset = -0.5f;
    public static float bottomOffset = 3.25f;
    public static float minCameraOrthographicSize = 6f;

    public Camera mainCamera => m_Camera;

    protected float previousUpdateAspect;
    protected float cameraZoom = 1;

    protected float targetAspect;
    protected float initialSize;

    protected Vector2 initCamSize;
    protected Vector2 camSize;

    protected Vector3 initPosition;


    protected override void Awake()
    {
        base.Awake();

        initialSize = mainCamera.orthographicSize;
        targetAspect = (float)targetSize.x / targetSize.y;

        initCamSize = new Vector2(2f * initialSize * mainCamera.aspect, 2f * initialSize);
        initPosition = mainCamera.transform.localPosition;
    }

    private void OnDrawGizmos()
    {
        var view = GetZoneView();
        var center = new Vector3(view.Item1.position.x, 0, view.Item1.position.y);
        var size = view.Item1.size;

        Kit.GizmosExtend.DrawSquare(new Vector3(view.Item2.position.x, 0, view.Item2.position.y), Vector3.forward, view.Item2.size, 0f, Color.red);

        Kit.GizmosExtend.DrawSquare(center, Vector3.forward, view.Item1.size, 0f, Color.green);
        Kit.GizmosExtend.DrawPoint(center);

        //var z1 = (center.z + size.y / 2f) - 6.1f;
        //Kit.GizmosExtend.DrawPoint(new Vector3(0, 0, z1));
        //Kit.GizmosExtend.DrawLine(new Vector3(-500, 0, z1), new Vector3(1000, 0, z1));

        //var z2 = (center.z - size.y / 2f + 3.25f);
        //Kit.GizmosExtend.DrawPoint(new Vector3(0, 0, z2));
        //Kit.GizmosExtend.DrawLine(new Vector3(-500, 0, z2), new Vector3(1000, 0, z2));
    }

    //void Update()
    //{
    //    if (!Mathf.Approximately(previousUpdateAspect, mainCamera.aspect))
    //    {
    //        previousUpdateAspect = mainCamera.aspect;
    //        UpdateCameraSize();
    //    }
    //}

    protected virtual (Rect, Rect) GetZoneView()
    {
        Rect zoneView, safeView;
        if (Application.isPlaying)
        {
            if (!Mathf.Approximately(previousUpdateAspect, mainCamera.aspect))
            {
                previousUpdateAspect = mainCamera.aspect;
                UpdateCameraSize();
            }
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        Plane dragPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        dragPlane.Raycast(ray, out distance);
        Vector3 center = ray.GetPoint(distance);
        Vector2 size = new Vector2(2f * mainCamera.orthographicSize * mainCamera.aspect, 2f * mainCamera.orthographicSize);
        zoneView = new Rect(new Vector2(center.x, center.z), size);
        safeView = new Rect(new Vector2(center.x, center.z), size);

        //safe area
        Rect safeArea = Screen.safeArea;
        Resolution screenSize = Screen.currentResolution;
        if (UnityEngine.Device.SystemInfo.deviceType != DeviceType.Desktop)
        {
            float top = 1 - ((safeArea.position.y + safeArea.height) / screenSize.height);
            float bottom = safeArea.position.y / screenSize.height;
            float left = safeArea.position.x / screenSize.width;
            float right = 1 - (safeArea.position.x + safeArea.width) / screenSize.width;

            safeView = new Rect(new Vector2(center.x, center.z + (size.y * (bottom - top) / 2)), new Vector2(size.x, size.y * (1 - (top + bottom))));
        }

        return (zoneView, safeView);
    }

    public void UpdateCameraSize()
    {
        if (targetAspect > mainCamera.aspect)
        {
            mainCamera.orthographicSize = initialSize * (targetAspect / mainCamera.aspect) / cameraZoom;
        }
        else
        {
            mainCamera.orthographicSize = initialSize / cameraZoom;
        }

        var view = GetZoneView();

        if (topObject != null) topObject.position = new Vector3(0, 0, view.Item2.position.y + view.Item2.size.y / 2 + topOffset);
        if (bottomObject != null) bottomObject.position = new Vector3(0, 0, view.Item2.position.y - view.Item2.size.y / 2 + bottomOffset);
    }
}