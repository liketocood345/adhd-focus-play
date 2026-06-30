using UnityEngine;
using UnityEngine.EventSystems;

namespace ADHDTraining.UI
{
    public static class UiEventSystem
    {
        public static void Ensure()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Object.DontDestroyOnLoad(es);
        }
    }
}
