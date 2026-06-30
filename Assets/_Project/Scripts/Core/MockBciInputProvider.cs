using UnityEngine;

namespace ADHDTraining.Core
{
    /// <summary>
    /// 无摄像头时的键盘模拟输入。
    /// </summary>
    public class MockBciInputProvider : MonoBehaviour, IBciInputProvider
    {
        [SerializeField] private float focus = 75f;
        [SerializeField] private float scrollSensitivity = 80f;

        private BciInputSnapshot _current;

        public BciInputSnapshot Current => _current;
        public bool IsConnected => true;

        private void Update()
        {
            focus = FocusScrollController.ApplyScroll(focus, scrollSensitivity);

            var head = HeadGesture.None;
            if (Input.GetKeyDown(KeyCode.W)) head = HeadGesture.Nod;
            else if (Input.GetKeyDown(KeyCode.S)) head = HeadGesture.Shake;
            else if (Input.GetKeyDown(KeyCode.Q)) head = HeadGesture.TurnLeft;
            else if (Input.GetKeyDown(KeyCode.E)) head = HeadGesture.TurnRight;

            _current = new BciInputSnapshot
            {
                Focus = focus,
                Blink = Input.GetKeyDown(KeyCode.Space),
                Head = head
            };
        }
    }
}
