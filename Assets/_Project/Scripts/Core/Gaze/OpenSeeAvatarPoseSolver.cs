using System;
using UnityEngine;
using OSF = OpenSee.OpenSee;

namespace ADHDTraining.Core.Gaze
{
    [Serializable]
    public struct AvatarRigPose
    {
        public bool Valid;
        public Vector3 RootPosition;
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;
        public Vector3 NeckPosition;
        public Quaternion NeckRotation;
        public Vector3 LeftShoulder;
        public Vector3 RightShoulder;
        public Quaternion LeftGaze;
        public Quaternion RightGaze;
        public float EyeLeftOpen;
        public float EyeRightOpen;
    }

    /// <summary>
    /// 将 OpenSeeFace 位姿转为 Unity 头肩骨骼（与 OpenSeeIKTarget / EasyVtuber 同坐标系）。
    /// </summary>
    public class AvatarPoseCalibration
    {
        private Quaternion _neutralRotation = Quaternion.identity;
        private Vector3 _neutralPosition;
        public bool IsCalibrated { get; private set; }

        public void Reset() => IsCalibrated = false;

        public void EnsureCalibrated(Quaternion rotation, Vector3 position)
        {
            if (IsCalibrated) return;
            _neutralRotation = rotation;
            _neutralPosition = position;
            IsCalibrated = true;
        }

        public Quaternion RelativeRotation(Quaternion worldRot) =>
            Quaternion.Inverse(_neutralRotation) * worldRot;

        public Vector3 RelativePosition(Vector3 worldPos) =>
            worldPos - _neutralPosition;
    }

    public static class OpenSeeAvatarPoseSolver
    {
        public const float DefaultTranslationScale = 0.12f;
        private const float MaxFitError = 100f;

        public static bool TrySolve(
            OSF.OpenSeeData data,
            AvatarPoseCalibration calibration,
            float translationScale,
            out AvatarRigPose pose)
        {
            pose = default;
            if (data == null || !data.got3DPoints || data.fit3DError > MaxFitError)
                return false;

            var headRot = ToUnityHeadRotation(data);
            var headPos = ToUnityTranslation(data) * translationScale;
            calibration.EnsureCalibrated(headRot, headPos);

            pose.RootPosition = calibration.RelativePosition(headPos);
            pose.HeadRotation = calibration.RelativeRotation(headRot);

            if (data.points3D != null && data.points3D.Length > 34)
            {
                var noseBridge = ToUnityPoint(data.points3D[27]) * translationScale;
                pose.HeadPosition = calibration.RelativePosition(noseBridge);
            }
            else
            {
                pose.HeadPosition = pose.RootPosition + Vector3.up * 0.2f;
            }

            pose.LeftGaze = data.leftGaze;
            pose.RightGaze = data.rightGaze;
            pose.EyeLeftOpen = data.features.EyeLeft;
            pose.EyeRightOpen = data.features.EyeRight;

            if (data.points3D != null && data.points3D.Length >= 68)
            {
                var chin = ToUnityPoint(data.points3D[8]) * translationScale;
                var noseBottom = ToUnityPoint(data.points3D[33]) * translationScale;
                var noseBridge = ToUnityPoint(data.points3D[27]) * translationScale;
                var jawLeft = ToUnityPoint(data.points3D[0]) * translationScale;
                var jawRight = ToUnityPoint(data.points3D[16]) * translationScale;

                var neckBase = Vector3.Lerp(chin, noseBottom, 0.35f);
                pose.NeckPosition = neckBase;

                var faceUp = (noseBridge - chin).normalized;
                if (faceUp.sqrMagnitude < 1e-6f) faceUp = Vector3.up;

                var shoulderAxis = (jawRight - jawLeft);
                var shoulderWidth = Mathf.Max(shoulderAxis.magnitude * 1.55f, 0.08f);
                var lateral = shoulderAxis.sqrMagnitude > 1e-6f
                    ? shoulderAxis.normalized
                    : Vector3.right;

                var shoulderCenter = neckBase - faceUp * shoulderWidth * 0.42f;
                pose.LeftShoulder = shoulderCenter - lateral * shoulderWidth * 0.5f;
                pose.RightShoulder = shoulderCenter + lateral * shoulderWidth * 0.5f;

                pose.NeckRotation = Quaternion.Slerp(
                    Quaternion.identity,
                    pose.HeadRotation,
                    0.45f);
            }
            else
            {
                pose.NeckPosition = pose.RootPosition + Vector3.down * 0.12f;
                pose.NeckRotation = pose.HeadRotation;
                pose.LeftShoulder = pose.NeckPosition + Vector3.left * 0.22f;
                pose.RightShoulder = pose.NeckPosition + Vector3.right * 0.22f;
            }

            pose.Valid = true;
            MirrorForWebcamPreview(ref pose);
            return true;
        }

        private static void MirrorForWebcamPreview(ref AvatarRigPose pose)
        {
            pose.RootPosition.x = -pose.RootPosition.x;
            pose.HeadPosition.x = -pose.HeadPosition.x;
            pose.NeckPosition.x = -pose.NeckPosition.x;
            (pose.LeftShoulder, pose.RightShoulder) = (MirrorX(pose.RightShoulder), MirrorX(pose.LeftShoulder));

            pose.HeadRotation = new Quaternion(
                -pose.HeadRotation.x, pose.HeadRotation.y, pose.HeadRotation.z, -pose.HeadRotation.w);
            pose.NeckRotation = new Quaternion(
                -pose.NeckRotation.x, pose.NeckRotation.y, pose.NeckRotation.z, -pose.NeckRotation.w);

            pose.LeftGaze = MirrorGaze(pose.LeftGaze);
            pose.RightGaze = MirrorGaze(pose.RightGaze);
        }

        private static Vector3 MirrorX(Vector3 v) => new(-v.x, v.y, v.z);

        private static Quaternion MirrorGaze(Quaternion q) =>
            new Quaternion(-q.x, q.y, q.z, -q.w);

        private static Vector3 ToUnityTranslation(OSF.OpenSeeData data)
        {
            var t = data.translation;
            return new Vector3(-t.x, t.y, -t.z);
        }

        private static Quaternion ToUnityHeadRotation(OSF.OpenSeeData data)
        {
            var q = data.rawQuaternion;
            return new Quaternion(-q.y, -q.x, q.z, q.w);
        }

        private static Vector3 ToUnityPoint(Vector3 p) => new(-p.y, p.x, -p.z);
    }
}
