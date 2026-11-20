using UnityEngine;
namespace Shorok
{
    public sealed class AmbientMotion : MonoBehaviour
    {
        public float parallax = .03f, sway = 1;
        Vector3 origin;
        Transform cameraTransform;
        void Start() { origin = transform.position; if (Camera.main != null) cameraTransform = Camera.main.transform; }
        void LateUpdate()
        {
            if (cameraTransform == null) return;
            transform.position = origin + new Vector3(Mathf.Round(cameraTransform.position.x * parallax * 32) / 32, 0, 0);
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * .5f + origin.x) * sway);
        }
    }
}
