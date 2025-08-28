using UnityEngine;

namespace EmeraldPowder.CameraScaler
{
    [AddComponentMenu("Rendering/Camera Scaler")]
    [HelpURL("https://emeraldpowder.github.io/unity-assets/camera-scaler/")]
    public class CameraScaler : MonoBehaviour
    {
        [Tooltip("Set this to the resolution you have set in Game View, or resolution you usually test you game with")]
        public Vector2 ReferenceResolution = new Vector2(720, 1280);

        public WorkingMode Mode = WorkingMode.ConstantWidth;
        public float MatchWidthOrHeight = 0.5f;
        [Tooltip("Fix Anchor Camera Orthographic, not use when cam perspective")]
        public TextAnchor Anchor = TextAnchor.MiddleCenter;

        private Camera componentCamera;
        private float targetAspect;
        private float cameraZoom = 1;

        private float initialSize;

        private float initialFov;
        private float horizontalFov;

        private float previousUpdateAspect;
        private WorkingMode previousUpdateMode;
        private float previousUpdateMatch;

        public float HorizontalSize => initialSize * targetAspect;
        public float HorizontalFov => horizontalFov;

        public float CameraZoom
        {
            get => cameraZoom;
            set
            {
                cameraZoom = value;
                UpdateCamera();
            }
        }

        public enum WorkingMode
        {
            ConstantHeight,
            ConstantWidth,
            MatchWidthOrHeight,
            Expand,
            Shrink
        }

        public float firstHight;
        public float firstWidth;
        public Vector3 firstPosition;
        public float height;
        public float width;

        private void Awake()
        {
            componentCamera = GetComponent<Camera>();
            initialSize = componentCamera.orthographicSize;

            targetAspect = ReferenceResolution.x / ReferenceResolution.y;

            initialFov = componentCamera.fieldOfView;
            horizontalFov = CalcVerticalFov(initialFov, 1 / targetAspect);

            firstHight = 2f * componentCamera.orthographicSize;
            firstWidth = firstHight * componentCamera.aspect;

            firstPosition = componentCamera.transform.position;
        }

        private void Update()
        {
            if (!Mathf.Approximately(previousUpdateAspect, componentCamera.aspect) ||
                previousUpdateMode != Mode ||
                !Mathf.Approximately(previousUpdateMatch, MatchWidthOrHeight))
            {
                UpdateCamera();
                CalcPositionCam();
                previousUpdateAspect = componentCamera.aspect;
                previousUpdateMode = Mode;
                previousUpdateMatch = MatchWidthOrHeight;
            }
        }

        private void UpdateCamera()
        {
            if (componentCamera.orthographic)
            {
                UpdateOrtho();
            }
            else
            {
                UpdatePerspective();
            }
        }

        private void UpdateOrtho()
        {
            switch (Mode)
            {
                case WorkingMode.ConstantHeight:
                    componentCamera.orthographicSize = initialSize / cameraZoom;
                    break;

                case WorkingMode.ConstantWidth:
                    componentCamera.orthographicSize = initialSize * (targetAspect / componentCamera.aspect) / cameraZoom;
                    break;

                case WorkingMode.MatchWidthOrHeight:
                    float vSize = initialSize;
                    float hSize = initialSize * (targetAspect / componentCamera.aspect);
                    float vLog = Mathf.Log(vSize, 2);
                    float hLog = Mathf.Log(hSize, 2);
                    float logWeightedAverage = Mathf.Lerp(hLog, vLog, MatchWidthOrHeight);
                    componentCamera.orthographicSize = Mathf.Pow(2, logWeightedAverage) / cameraZoom;
                    break;

                case WorkingMode.Expand:
                    if (targetAspect > componentCamera.aspect)
                    {
                        componentCamera.orthographicSize = initialSize * (targetAspect / componentCamera.aspect) / cameraZoom;
                    }
                    else
                    {
                        componentCamera.orthographicSize = initialSize / cameraZoom;
                    }

                    break;

                case WorkingMode.Shrink:
                    if (targetAspect < componentCamera.aspect)
                    {
                        componentCamera.orthographicSize = initialSize * (targetAspect / componentCamera.aspect) / cameraZoom;
                    }
                    else
                    {
                        componentCamera.orthographicSize = initialSize / cameraZoom;
                    }

                    break;
                default:
                    Debug.LogError("Incorrect CameraScaler.Mode: " + Mode);
                    break;
            }
        }

        private void UpdatePerspective()
        {
            switch (Mode)
            {
                case WorkingMode.ConstantHeight:
                    componentCamera.fieldOfView = initialFov / cameraZoom;
                    break;

                case WorkingMode.ConstantWidth:
                    componentCamera.fieldOfView = CalcVerticalFov(horizontalFov, componentCamera.aspect) / cameraZoom;
                    break;

                case WorkingMode.MatchWidthOrHeight:
                    float vFov = initialFov;
                    float hFov = CalcVerticalFov(horizontalFov, componentCamera.aspect);
                    float vLog = Mathf.Log(vFov, 2);
                    float hLog = Mathf.Log(hFov, 2);
                    float logWeightedAverage = Mathf.Lerp(hLog, vLog, MatchWidthOrHeight);
                    componentCamera.fieldOfView = Mathf.Pow(2, logWeightedAverage) / cameraZoom;
                    break;

                case WorkingMode.Expand:
                    if (targetAspect > componentCamera.aspect)
                    {
                        componentCamera.fieldOfView = CalcVerticalFov(horizontalFov, componentCamera.aspect) / cameraZoom;
                    }
                    else
                    {
                        componentCamera.fieldOfView = initialFov / cameraZoom;
                    }

                    break;

                case WorkingMode.Shrink:
                    if (targetAspect < componentCamera.aspect)
                    {
                        componentCamera.fieldOfView = CalcVerticalFov(horizontalFov, componentCamera.aspect) / cameraZoom;
                    }
                    else
                    {
                        componentCamera.fieldOfView = initialFov / cameraZoom;
                    }

                    break;
                default:
                    Debug.LogError("Incorrect CameraScaler.Mode: " + Mode);
                    break;
            }
        }

        private static float CalcVerticalFov(float hFovInDeg, float aspectRatio)
        {
            float hFovInRads = hFovInDeg * Mathf.Deg2Rad;

            float vFovInRads = 2 * Mathf.Atan(Mathf.Tan(hFovInRads / 2) / aspectRatio);

            return vFovInRads * Mathf.Rad2Deg;
        }

        private void CalcPositionCam()
        {
            if (!componentCamera.orthographic) return;

            height = 2f * componentCamera.orthographicSize;
            width = height * componentCamera.aspect;

            switch (Anchor)
            {
                case TextAnchor.UpperLeft:
                case TextAnchor.UpperRight:
                case TextAnchor.UpperCenter:
                    componentCamera.transform.Translate(new Vector3(0, -1 * (height - firstHight) / 2), 0);
                    break;


                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    break;

                case TextAnchor.LowerLeft:
                case TextAnchor.LowerRight:
                case TextAnchor.LowerCenter:
                    componentCamera.transform.Translate(new Vector3(0, (height - firstHight) / 2, 0));
                    break;

                default:
                    break;
            }
        }
    }
}