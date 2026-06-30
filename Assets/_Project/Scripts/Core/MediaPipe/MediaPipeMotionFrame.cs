using System;
using UnityEngine;

namespace ADHDTraining.Core.MediaPipe
{
  [Serializable]
  public struct MediaPipeMotionFrame
  {
    public int Seq;
    public bool FaceValid;
    public bool PoseValid;
    public float Fps;

    public float EyeL;
    public float EyeR;
    public float HeadPitch;
    public float HeadYaw;
    public float HeadRoll;
    public float GazeYaw;
    public float GazePitch;

    public float NeckX;
    public float NeckY;
    public float NeckZ;
    public float LsX, LsY, LsZ;
    public float RsX, RsY, RsZ;

    public bool LeftHandRaised;
    public bool RightHandRaised;
    public int BodyLean;

    public bool IsTracking => FaceValid || PoseValid;
  }
}
