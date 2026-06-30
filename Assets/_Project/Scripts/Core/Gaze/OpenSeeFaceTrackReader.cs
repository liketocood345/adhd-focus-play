using System;
using UnityEngine;
using OSF = OpenSee.OpenSee;

namespace ADHDTraining.Core.Gaze
{
    [Serializable]
    public struct GazeSample
    {
        public float Yaw;
        public float Pitch;
        public bool Valid;

        public Vector2 ToVector2() => new(Yaw, Pitch);

        public static GazeSample Invalid => new() { Valid = false };
    }

    /// <summary>
    /// 与 EasyVtuberStudio 相同数据源：OpenSeeFace 3D 眼球朝向 + 特征量。
    /// </summary>
    public struct FaceTrackFrame
    {
        public bool Valid;
        public bool Got3D;
        public float Fit3DError;
        public Vector3 HeadEuler;
        public Vector3 Translation;
        public Quaternion LeftGaze;
        public Quaternion RightGaze;
        public float EyeLeft;
        public float EyeRight;
        public GazeSample CombinedGaze;
        public Vector2 PupilLeft;
        public Vector2 PupilRight;
        public Vector2 CameraResolution;
    }

    public static class OpenSeeFaceTrackReader
    {
        public static FaceTrackFrame Read(OSF.OpenSeeData data)
        {
            if (data == null)
                return default;

            var frame = new FaceTrackFrame
            {
                Valid = true,
                Got3D = data.got3DPoints,
                Fit3DError = data.fit3DError,
                HeadEuler = data.rotation,
                Translation = data.translation,
                LeftGaze = data.leftGaze,
                RightGaze = data.rightGaze,
                EyeLeft = data.features.EyeLeft,
                EyeRight = data.features.EyeRight,
                CameraResolution = data.cameraResolution,
                CombinedGaze = ExtractCombinedGaze(data)
            };

            if (data.points != null && data.points.Length > 68)
            {
                frame.PupilLeft = data.points[66];
                frame.PupilRight = data.points[67];
            }

            return frame;
        }

        public static GazeSample ExtractCombinedGaze(OSF.OpenSeeData data)
        {
            if (data == null || !data.got3DPoints)
                return GazeSample.Invalid;

            var leftDir = (data.leftGaze * Vector3.forward).normalized;
            var rightDir = (data.rightGaze * Vector3.forward).normalized;
            if (leftDir.sqrMagnitude < 1e-6f || rightDir.sqrMagnitude < 1e-6f)
                return GazeSample.Invalid;

            var combined = (leftDir + rightDir).normalized;
            return DirectionToGazeSample(combined);
        }

        public static GazeSample DirectionToGazeSample(Vector3 dir)
        {
            dir.Normalize();
            return new GazeSample
            {
                Yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg,
                Pitch = Mathf.Asin(Mathf.Clamp(-dir.y, -1f, 1f)) * Mathf.Rad2Deg,
                Valid = true
            };
        }
    }
}
