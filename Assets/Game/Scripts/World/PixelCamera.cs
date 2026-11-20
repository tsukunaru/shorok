using UnityEngine;
namespace Shorok
{
    public sealed class PixelCamera : MonoBehaviour
    {
        LevelRoot level;
        Vector3 smooth;
        Vector3 velocity;
        float ahead;
        Camera cameraComponent;
        float shake;
        public void Shake(float amount) { if (GameApp.Instance.Data.settings.shake) shake = amount; }
        public void Initialize(LevelRoot root) { level = root; cameraComponent = GetComponent<Camera>(); Snap(); }
        Vector3 Framing(Vector3 target)
        {
            float halfWidth = cameraComponent.orthographicSize * cameraComponent.aspect;
            return new Vector3(Mathf.Clamp(target.x + ahead, halfWidth + .5f, level.rooms[level.rooms.Length - 1].Right - halfWidth), Mathf.Max(4.05f, target.y + 1), -10);
        }
        public void Snap() { if (level == null) return; ahead = 0; smooth = Framing(level.Ghost.transform.position); transform.position = smooth; velocity = Vector3.zero; }
        void LateUpdate()
        {
            if (level == null || GameApp.Instance == null) return;
            var possession = level.Ghost.Possession;
            Transform target = possession.Current != null ? possession.Current.transform : level.Ghost.transform;
            ahead = Mathf.Lerp(ahead, GameApp.Instance.Input.Move * 1.1f, 1 - Mathf.Exp(-3 * Time.deltaTime));
            var desired = Framing(target.position);
            if (Mathf.Abs(desired.y - smooth.y) < .65f) desired.y = smooth.y;
            smooth = Vector3.SmoothDamp(smooth, desired, ref velocity, .18f, 50, Time.deltaTime);
            shake = Mathf.MoveTowards(shake, 0, Time.deltaTime * .4f);
            var offset = Random.insideUnitCircle * shake;
            transform.position = new Vector3(Mathf.Round((smooth.x + offset.x) * 32) / 32, Mathf.Round((smooth.y + offset.y) * 32) / 32, -10);
        }
    }
}
