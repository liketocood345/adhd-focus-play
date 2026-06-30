using UnityEngine;

namespace ADHDTraining.Core.Input
{
    public class StandardKeyboardSource : IAnalogKeyboardSource
    {
        public bool IsAvailable => true;
        public string SourceName => "standard_keyboard";

        public void Poll() { }

        public float ReadKeyTravel(KeyBinding binding) => binding switch
        {
            KeyBinding.FocusPrimary => BciLegacyInput.GetKey(KeyCode.F) ? 1f : 0f,
            KeyBinding.Blink => BciLegacyInput.GetKey(KeyCode.Space) ? 1f : 0f,
            KeyBinding.Nod => BciLegacyInput.GetKey(KeyCode.W) ? 1f : 0f,
            KeyBinding.Shake => BciLegacyInput.GetKey(KeyCode.S) ? 1f : 0f,
            KeyBinding.TurnLeft => BciLegacyInput.GetKey(KeyCode.Q) ? 1f : 0f,
            KeyBinding.TurnRight => BciLegacyInput.GetKey(KeyCode.E) ? 1f : 0f,
            _ => 0f
        };

        public bool WasPressedThisFrame(KeyBinding binding) => binding switch
        {
            KeyBinding.Blink => BciLegacyInput.GetKeyDown(KeyCode.Space),
            KeyBinding.Nod => BciLegacyInput.GetKeyDown(KeyCode.W),
            KeyBinding.Shake => BciLegacyInput.GetKeyDown(KeyCode.S),
            KeyBinding.TurnLeft => BciLegacyInput.GetKeyDown(KeyCode.Q),
            KeyBinding.TurnRight => BciLegacyInput.GetKeyDown(KeyCode.E),
            _ => false
        };
    }
}
