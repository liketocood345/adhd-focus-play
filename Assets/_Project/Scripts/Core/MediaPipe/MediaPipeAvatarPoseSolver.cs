using UnityEngine;

namespace ADHDTraining.Core.MediaPipe
{
  public struct MediaPipeAvatarRigPose
  {
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

  public class MediaPipeAvatarCalibration
  {
    private bool _ready;
    private float _headPitch0;
    private float _headYaw0;
    private float _headRoll0;

    public void Reset() => _ready = false;

    public bool TrySolve(MediaPipeMotionFrame frame, float translationScale, out MediaPipeAvatarRigPose pose)
    {
      pose = default;
      if (!frame.FaceValid && !frame.PoseValid) return false;

      if (!_ready)
      {
        _headPitch0 = frame.HeadPitch;
        _headYaw0 = frame.HeadYaw;
        _headRoll0 = frame.HeadRoll;
        _ready = true;
      }

      var pitch = (frame.HeadPitch - _headPitch0) * 0.85f;
      var yaw = -(frame.HeadYaw - _headYaw0) * 0.85f;
      var roll = -(frame.HeadRoll - _headRoll0) * 0.6f;
      var headRot = Quaternion.Euler(pitch, yaw, roll);
      var rootY = frame.PoseValid ? (0.5f - frame.NeckY) * translationScale : 0f;
      var rootX = frame.PoseValid ? (frame.NeckX - 0.5f) * translationScale * 0.5f : 0f;

      pose.RootPosition = new Vector3(rootX, rootY, 0f);
      pose.HeadPosition = new Vector3(0f, 1.62f, 0f);
      pose.HeadRotation = headRot;
      pose.NeckPosition = new Vector3(0f, 1.38f, 0f);
      pose.NeckRotation = Quaternion.Euler(pitch * 0.45f, yaw * 0.45f, roll * 0.3f);

      if (frame.PoseValid)
      {
        pose.LeftShoulder = NormToLocal(frame.LsX, frame.LsY, frame.LsZ, translationScale, -0.38f, 1.28f);
        pose.RightShoulder = NormToLocal(frame.RsX, frame.RsY, frame.RsZ, translationScale, 0.38f, 1.28f);
      }
      else
      {
        pose.LeftShoulder = new Vector3(-0.38f, 1.28f, 0f);
        pose.RightShoulder = new Vector3(0.38f, 1.28f, 0f);
      }

      var gazePitch = Mathf.Clamp(frame.GazePitch, -25f, 25f);
      var gazeYaw = Mathf.Clamp(frame.GazeYaw, -25f, 25f);
      pose.LeftGaze = Quaternion.Euler(-gazePitch, gazeYaw, 0f);
      pose.RightGaze = Quaternion.Euler(-gazePitch, gazeYaw, 0f);
      pose.EyeLeftOpen = frame.EyeL;
      pose.EyeRightOpen = frame.EyeR;
      return true;
    }

    private static Vector3 NormToLocal(float nx, float ny, float nz, float scale, float defaultX, float defaultY)
    {
      if (nx <= 0f && ny <= 0f) return new Vector3(defaultX, defaultY, 0f);
      return new Vector3((nx - 0.5f) * scale, (0.5f - ny) * scale + 1.05f, -nz * scale * 0.3f);
    }
  }
}
