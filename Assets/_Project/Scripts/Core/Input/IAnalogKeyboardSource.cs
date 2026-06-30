namespace ADHDTraining.Core.Input
{
    public enum KeyBinding
    {
        FocusPrimary,
        Blink,
        Nod,
        Shake,
        TurnLeft,
        TurnRight
    }

    public interface IAnalogKeyboardSource
    {
        bool IsAvailable { get; }
        string SourceName { get; }
        void Poll();
        float ReadKeyTravel(KeyBinding binding);
        bool WasPressedThisFrame(KeyBinding binding);
    }
}
