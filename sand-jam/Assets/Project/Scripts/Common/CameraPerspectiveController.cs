using EmeraldPowder.CameraScaler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(40)]
public class CameraPerspectiveController : Kit.Common.Singleton<CameraPerspectiveController>
{
    [SerializeField] Camera m_Camera;
    [SerializeField] Canvas m_CanvasGame;
    [SerializeField] Renderer m_Background;

    public static readonly float _fixedTopUI = 300f;
    public static readonly float _fixedBottomUI = 464f;
    public static readonly float minCameraOrthographicSize = 6f;

    public Camera mainCamera => m_Camera;
    public Canvas gameCanvas => m_CanvasGame;
    public Plane plane = new Plane(Vector3.forward, Vector3.zero);

    public float cosRotationCamera => Mathf.Cos(mainCamera.transform.eulerAngles.x * Mathf.Deg2Rad);
    protected float initialSize;
    protected Vector3 initPosition;
    protected float offsetCamY;

    protected override void Awake()
    {
        base.Awake();

        initialSize = mainCamera.fieldOfView;

        initPosition = mainCamera.transform.localPosition;
        offsetCamY = mainCamera.transform.position.y;
    }


    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        Vector3[] hits = GetCameraFrustumIntersections(mainCamera, plane);
        if (hits == null || hits.Length != 5) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < 5; i++)
            Gizmos.DrawLine(mainCamera.transform.position, hits[i]);

        DrawBound(hits, Color.red);
        Kit.GizmosExtend.DrawPoint(hits[4], Color.red);

        var boundView = GetOffsetBound(hits, _fixedTopUI, _fixedBottomUI);
        Kit.GizmosExtend.DrawPoint(boundView[4], Color.green);

        DrawBound(boundView, Color.green);

        var hight = Mathf.Abs(boundView[0].y - boundView[2].y);
        var widthTop = Mathf.Abs(boundView[2].x - boundView[3].x);
        var widthBottom = Mathf.Abs(boundView[0].x - boundView[1].x);
    }

    void DrawBound(Vector3[] pts, Color color)
    {
        Gizmos.color = color;

        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(pts[i], pts[(i + 1) % 4]);
    }

    Vector3[] GetOffsetBound(Vector3[] worldPoints, float offsetTopUI, float offsetBottomUI)
    {
        var canvasSize = gameCanvas.pixelRect.size;
        var scaleFactor = gameCanvas.scaleFactor;

        float safeTop = 0;
        float safeBottom = 0;
        if (UnityEngine.Device.SystemInfo.deviceType != DeviceType.Desktop)
        {
            Rect safeArea = Screen.safeArea;
            Resolution screenSize = Screen.currentResolution;

            float top = 1 - ((safeArea.position.y + safeArea.height) / screenSize.height);
            float bottom = safeArea.position.y / screenSize.height;
            float left = safeArea.position.x / screenSize.width;
            float right = 1 - (safeArea.position.x + safeArea.width) / screenSize.width;

            safeTop = top;
            safeBottom = bottom;
        }

        // Switch to screen space
        Vector3[] screenPts = new Vector3[worldPoints.Length];
        for (int i = 0; i < worldPoints.Length; i++)
            screenPts[i] = mainCamera.WorldToScreenPoint(worldPoints[i]);

        screenPts[0].y += (offsetBottomUI + safeBottom * canvasSize.y) * scaleFactor;
        screenPts[1].y += (offsetBottomUI + safeBottom * canvasSize.y) * scaleFactor;
        screenPts[2].y -= (offsetTopUI + safeTop * canvasSize.y) * scaleFactor;
        screenPts[3].y -= (offsetTopUI + safeTop * canvasSize.y) * scaleFactor;

        screenPts[4].y = (screenPts[0].y + screenPts[2].y) * 0.5f;

        // Switch back to world space
        Vector3[] newWorldPts = new Vector3[worldPoints.Length];
        for (int i = 0; i < screenPts.Length; i++)
        {
            var ray = mainCamera.ScreenPointToRay(screenPts[i]);
            if (plane.Raycast(ray, out float enter))
                newWorldPts[i] = ray.GetPoint(enter);
            else
                return null; // Not intersecting with plane
        }

        return newWorldPts;
    }

    Vector3[] GetCameraFrustumIntersections(Camera cam, Plane plane)
    {
        Vector3[] corners = new Vector3[]
        {
            new Vector3(0, 0, 0), // bottom-left
            new Vector3(1, 0, 0), // bottom-right
            new Vector3(1, 1, 0), // top-right
            new Vector3(0, 1, 0),  // top-left
            new Vector3(0.5f, 0.5f, 0)  // center
        };

        Vector3[] hits = new Vector3[corners.Length];

        for (int i = 0; i < corners.Length; i++)
        {
            Ray ray = cam.ViewportPointToRay(corners[i]);
            if (plane.Raycast(ray, out float enter))
                hits[i] = ray.GetPoint(enter);
            else
                return null; // Error
        }

        return hits;
    }


    public void InitCamera(Rect bound)
    {
        //Debug.Log("InitCamera");
        //set fieldOfView cam
        Vector3[] hits = GetCameraFrustumIntersections(mainCamera, plane);
        if (hits == null || hits.Length != 5) return;

        var boundView = GetOffsetBound(hits, _fixedTopUI, _fixedBottomUI);

        var hight = Mathf.Abs(boundView[0].y - boundView[2].y);
        var widthTop = Mathf.Abs(boundView[2].x - boundView[3].x);
        var widthBottom = Mathf.Abs(boundView[0].x - boundView[1].x);

        float widthRatio = widthBottom / bound.size.x;
        float heightRatio = hight / bound.size.y;
        var ratio = Mathf.Min(widthRatio, heightRatio);

        mainCamera.fieldOfView = Mathf.Max(initialSize / ratio, minCameraOrthographicSize);

        //set position cam
        SetCamPos();
    }



    [NaughtyAttributes.Button]
    void SetCamPos()
    {
        var view = GetCameraFrustumIntersections(mainCamera, plane);
        var boundView = GetOffsetBound(view, _fixedTopUI, _fixedBottomUI);

        var pivot = (view[0] + view[1] + view[2] + view[3]) / 4f;

        var backgroundSize = new Vector2(m_Background.transform.localScale.x, m_Background.transform.localScale.z) * 10f;
        var hight = Mathf.Abs(view[0].y - view[2].y);
        var widthTop = Mathf.Abs(view[2].x - view[3].x);
        var widthBottom = Mathf.Abs(view[0].x - view[1].x);

        var widthRatio = backgroundSize.x / widthTop;
        var heightRatio = (backgroundSize.y / hight);
        var ratio = Mathf.Min(widthRatio, heightRatio);
        m_Background.transform.localScale /= ratio;

        mainCamera.transform.SetPosition(y: transform.position.y - boundView[4].y);
        m_Background.transform.SetPosition(y: pivot.y - boundView[4].y);
    }
}
