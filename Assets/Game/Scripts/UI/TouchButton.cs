using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Shorok
{
    public sealed class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public int kind;
        readonly HashSet<int> pointers = new HashSet<int>();
        public void OnPointerDown(PointerEventData e) { if (pointers.Add(e.pointerId)) GameApp.Instance.Input.TouchPress(kind); }
        public void OnPointerUp(PointerEventData e) { if (pointers.Remove(e.pointerId)) GameApp.Instance.Input.TouchRelease(kind); }
        void OnDisable() { foreach (var unused in pointers) if (GameApp.Instance != null) GameApp.Instance.Input.TouchRelease(kind); pointers.Clear(); }
    }
}
