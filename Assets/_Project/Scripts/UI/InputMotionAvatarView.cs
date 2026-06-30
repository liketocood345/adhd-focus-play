using ADHDTraining.Core;
using ADHDTraining.Core.Gaze;
using ADHDTraining.Core.MediaPipe;
using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    public class InputMotionAvatarView : MonoBehaviour
    {
        private const float Smoothing = 0.18f;
        private const float TranslationScale = 2.8f;

        private RawImage _image;
        private RenderTexture _rt;
        private Camera _cam;
        private Transform _rigRoot;
        private Transform _chest;
        private Transform _neck;
        private Transform _head;
        private Transform _shoulderL;
        private Transform _shoulderR;
        private Transform _leftEye;
        private Transform _rightEye;

        private readonly MediaPipeAvatarCalibration _calibration = new();
        private Vector3 _smoothRoot;
        private Vector3 _smoothHeadPos;
        private Quaternion _smoothHeadRot = Quaternion.identity;
        private Vector3 _smoothNeckPos;
        private Quaternion _smoothNeckRot = Quaternion.identity;
        private Vector3 _smoothShoulderL;
        private Vector3 _smoothShoulderR;
        private bool _hasSmooth;

        public void Initialize()
        {
            _image = gameObject.AddComponent<RawImage>();
            _rt = new RenderTexture(320, 320, 16);
            _image.texture = _rt;

            var stage = new GameObject("AvatarStage");
            stage.transform.position = new Vector3(1000f, 1000f, 0f);

            var camGo = new GameObject("AvatarCam");
            camGo.transform.SetParent(stage.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 1.55f, -2.8f);
            camGo.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            _cam = camGo.AddComponent<Camera>();
            _cam.targetTexture = _rt;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = new Color(0.1f, 0.12f, 0.16f);
            _cam.fieldOfView = 38f;

            _rigRoot = new GameObject("RigRoot").transform;
            _rigRoot.SetParent(stage.transform, false);

            _chest = CreatePart(_rigRoot, "Chest", PrimitiveType.Capsule,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.42f, 0.5f, 0.28f),
                new Color(0.35f, 0.42f, 0.55f));

            _neck = CreatePart(_rigRoot, "Neck", PrimitiveType.Cylinder,
                new Vector3(0f, 1.38f, 0f), new Vector3(0.14f, 0.1f, 0.14f),
                new Color(0.82f, 0.72f, 0.62f));

            _head = CreatePart(_rigRoot, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.62f, 0f), new Vector3(0.42f, 0.48f, 0.4f),
                new Color(0.9f, 0.78f, 0.68f));

            _shoulderL = CreatePart(_rigRoot, "ShoulderL", PrimitiveType.Cube,
                new Vector3(-0.38f, 1.28f, 0f), new Vector3(0.28f, 0.1f, 0.2f),
                new Color(0.4f, 0.48f, 0.6f));

            _shoulderR = CreatePart(_rigRoot, "ShoulderR", PrimitiveType.Cube,
                new Vector3(0.38f, 1.28f, 0f), new Vector3(0.28f, 0.1f, 0.2f),
                new Color(0.4f, 0.48f, 0.6f));

            _leftEye = CreateEye(_head, new Vector3(-0.11f, 0.06f, 0.36f));
            _rightEye = CreateEye(_head, new Vector3(0.11f, 0.06f, 0.36f));
        }

        public void ResetCalibration() => _calibration.Reset();

        public void Apply(BciInputSnapshot snap, MediaPipeMotionFrame motion)
        {
            if (_head == null) return;

            if (!motion.IsTracking ||
                !_calibration.TrySolve(motion, TranslationScale, out var pose))
            {
                ApplyGestureFallback(snap);
                return;
            }

            var t = 1f - Smoothing;
            if (!_hasSmooth)
            {
                _smoothRoot = pose.RootPosition;
                _smoothHeadPos = pose.HeadPosition;
                _smoothHeadRot = pose.HeadRotation;
                _smoothNeckPos = pose.NeckPosition;
                _smoothNeckRot = pose.NeckRotation;
                _smoothShoulderL = pose.LeftShoulder;
                _smoothShoulderR = pose.RightShoulder;
                _hasSmooth = true;
            }
            else
            {
                _smoothRoot = Vector3.Lerp(_smoothRoot, pose.RootPosition, t);
                _smoothHeadPos = Vector3.Lerp(_smoothHeadPos, pose.HeadPosition, t);
                _smoothHeadRot = Quaternion.Slerp(_smoothHeadRot, pose.HeadRotation, t);
                _smoothNeckPos = Vector3.Lerp(_smoothNeckPos, pose.NeckPosition, t);
                _smoothNeckRot = Quaternion.Slerp(_smoothNeckRot, pose.NeckRotation, t);
                _smoothShoulderL = Vector3.Lerp(_smoothShoulderL, pose.LeftShoulder, t);
                _smoothShoulderR = Vector3.Lerp(_smoothShoulderR, pose.RightShoulder, t);
            }

            _rigRoot.localPosition = _smoothRoot;
            _head.localPosition = _smoothHeadPos;
            _head.localRotation = _smoothHeadRot;
            _neck.localPosition = _smoothNeckPos;
            _neck.localRotation = _smoothNeckRot;
            _shoulderL.localPosition = _smoothShoulderL;
            _shoulderR.localPosition = _smoothShoulderR;

            if (motion.LeftHandRaised)
                _shoulderL.localPosition += Vector3.up * 0.15f;
            if (motion.RightHandRaised)
                _shoulderR.localPosition += Vector3.up * 0.15f;

            ApplyEyeGaze(pose);
            ApplyEyeBlink(pose.EyeLeftOpen, pose.EyeRightOpen);
        }

        private void ApplyEyeGaze(MediaPipeAvatarRigPose pose)
        {
            if (_leftEye != null) _leftEye.localRotation = pose.LeftGaze;
            if (_rightEye != null) _rightEye.localRotation = pose.RightGaze;
        }

        private void ApplyEyeBlink(float eyeLeft, float eyeRight)
        {
            var leftScale = EyeOpenScale(eyeLeft);
            var rightScale = EyeOpenScale(eyeRight);
            if (_leftEye != null) _leftEye.localScale = Vector3.one * leftScale;
            if (_rightEye != null) _rightEye.localScale = Vector3.one * rightScale;
        }

        private static float EyeOpenScale(float evtsEyeValue) =>
            Mathf.Lerp(0.04f, 0.12f, Mathf.InverseLerp(0.82f, 0.28f, evtsEyeValue));

        private void ApplyGestureFallback(BciInputSnapshot snap)
        {
            var targetYaw = snap.Head switch
            {
                HeadGesture.TurnLeft => -18f,
                HeadGesture.TurnRight => 18f,
                HeadGesture.Shake => Mathf.Sin(Time.time * 12f) * 12f,
                _ => 0f
            };
            var targetPitch = snap.Head == HeadGesture.Nod ? 15f : 0f;
            _head.localRotation = Quaternion.Euler(targetPitch, targetYaw, 0f);
            var eyeScale = snap.Blink ? 0.05f : 0.12f;
            if (_leftEye != null) _leftEye.localScale = Vector3.one * eyeScale;
            if (_rightEye != null) _rightEye.localScale = Vector3.one * eyeScale;
        }

        private static Transform CreatePart(
            Transform parent, string name, PrimitiveType type,
            Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            var r = go.GetComponent<Renderer>();
            if (r != null) r.material.color = color;
            Object.Destroy(go.GetComponent<Collider>());
            return go.transform;
        }

        private static Transform CreateEye(Transform parent, Vector3 localPos)
        {
            var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.transform.SetParent(parent, false);
            eye.transform.localPosition = localPos;
            eye.transform.localScale = Vector3.one * 0.1f;
            var r = eye.GetComponent<Renderer>();
            if (r != null) r.material.color = Color.black;
            Object.Destroy(eye.GetComponent<Collider>());
            return eye.transform;
        }

        private void OnDestroy()
        {
            if (_rt != null) _rt.Release();
        }
    }
}
