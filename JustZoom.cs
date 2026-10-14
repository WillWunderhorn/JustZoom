using Il2Cpp;
using UnityEngine;

namespace JustZoom
{
    public class JustZoom
    {
        private const float PitchMin = -85f;
        private const float PitchMax = 85f;
        private float zoomFactor = 1f;
        private float zoomTarget = 2.5f;
        private const float ZoomMin = 1.1f;
        private const float ZoomMax = 20f;
        private float baseFov = 60f;
        private float baseSensitivity;
        private bool hasBaseSensitivity;
        private bool needSync = true;
        private float targetYaw, targetPitch;
        private float smoothYaw, smoothPitch;
        private Vector2 lastWritten;
        public float ZoomFactor => zoomFactor;
        public bool IsZoomed => zoomFactor > 1.01f;

        public void Update()
        {
            vp_FPSCamera cam = GameManager.GetVpFPSCamera();
            if (cam == null || cam.m_Camera == null)
            {
                zoomFactor = 1f;
                return;
            }

            UpdateZoom(cam);

            if (IsZoomed)
                UpdateSmoothLook(cam);
            else
                needSync = true;
        }

        public void Reset()
        {
            zoomFactor = 1f;
            hasBaseSensitivity = false;
            needSync = true;
        }

        private void UpdateZoom(vp_FPSCamera cam)
        {
            Settings s = Settings.Instance;
            bool hold = Input.GetKey(s.zoomKey);

            if (!hold && zoomFactor <= 1.005f)
            {
                if (zoomFactor != 1f)
                {
                    zoomFactor = 1f;
                    SetFov(cam, baseFov);
                    if (hasBaseSensitivity)
                        SetSensitivity(cam, baseSensitivity);
                }
                else
                {
                    baseFov = cam.m_Camera.fieldOfView;
                    baseSensitivity = cam.MouseSensitivity;
                    hasBaseSensitivity = true;
                }
                return;
            }

            if (hold)
            {
                float scroll = Input.mouseScrollDelta.y;
                if (scroll != 0f)
                {
                    float step = 1f + s.zoomWheelStep / 100f; 
                    zoomTarget = Mathf.Clamp(
                        zoomTarget * Mathf.Pow(step, scroll),
                        ZoomMin, ZoomMax);
                }
            }

            float target = hold ? zoomTarget : 1f;
            float k = 1f - Mathf.Exp(-s.zoomSmoothing * Time.deltaTime);
            zoomFactor = Mathf.Lerp(zoomFactor, target, k);

            float halfRad = baseFov * 0.5f * Mathf.Deg2Rad;
            float fov = 2f * Mathf.Atan(Mathf.Tan(halfRad) / zoomFactor) * Mathf.Rad2Deg;
            SetFov(cam, fov);

            if (!IsZoomed)
            {
                if (!hasBaseSensitivity)
                {
                    baseSensitivity = cam.MouseSensitivity;
                    hasBaseSensitivity = true;
                }
                SetSensitivity(cam, baseSensitivity / Mathf.Max(1f, zoomFactor));
            }
        }

        private void UpdateSmoothLook(vp_FPSCamera cam)
        {
            Vector2 a = cam.Angle;

            if (needSync)
            {
                Sync(a);
                return;
            }

            float dYaw = Mathf.DeltaAngle(lastWritten.x, a.x);
            float dPitch = a.y - lastWritten.y;

            if (Mathf.Abs(dYaw) > 90f || Mathf.Abs(dPitch) > 90f)
            {
                Sync(a);
                return;
            }

            float z = Mathf.Max(1f, zoomFactor);
            dYaw /= z;
            dPitch /= z;

            targetYaw += dYaw;
            targetPitch = Mathf.Clamp(targetPitch + dPitch, PitchMin, PitchMax);
            float rate = 41 - Settings.Instance.smoothLag;
            float kk = 1f - Mathf.Exp(-rate * Time.deltaTime);
            smoothYaw = Mathf.Lerp(smoothYaw, targetYaw, kk);
            smoothPitch = Mathf.Lerp(smoothPitch, targetPitch, kk);

            float y = Mathf.Repeat(smoothYaw, 360f);
            float p = Mathf.Clamp(smoothPitch, PitchMin, PitchMax);
            SetAllAngles(cam, y, p);
            lastWritten = new Vector2(y, p);
        }

        private void Sync(Vector2 a)
        {
            targetYaw = smoothYaw = a.x;
            targetPitch = smoothPitch = a.y;
            lastWritten = a;
            needSync = false;
        }

        private static void SetAllAngles(vp_FPSCamera cam, float y, float p)
        {
            cam.Angle = new Vector2(y, p);
            cam.m_Yaw = y;
            cam.m_Pitch = p;
            cam.m_CurrentYaw = y;
            cam.m_TargetYaw = y;
            cam.m_CurrentPitch = p;
            cam.m_TargetPitch = p;
        }

        private static void SetFov(vp_FPSCamera cam, float fov)
        {
            cam.m_Camera.fieldOfView = fov;
            cam.m_RenderingFieldOfView = fov;
        }

        private static void SetSensitivity(vp_FPSCamera cam, float value)
        {
            cam.MouseSensitivity = value;
            cam.m_MouseSensitivity = value;
        }
    }
}


