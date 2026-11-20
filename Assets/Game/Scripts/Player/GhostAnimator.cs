using UnityEngine;
namespace Shorok
{
    public sealed class GhostAnimator : MonoBehaviour
    {
        public SpriteRenderer body;
        public Sprite normal, blink;
        GhostMotor motor;
        float nextBlink, blinkEnd, land, arrival;
        public void Emerge() { arrival = .22f; }
        void Awake() { motor = GetComponent<GhostMotor>(); motor.Landed += OnLand; nextBlink = Time.time + 3.1f; }
        void OnLand(float speed)
        {
            if (speed < -2) land = .18f;
            if (speed < -7 && GameApp.Instance?.Level != null) GameApp.Instance.Level.Particles.Burst(transform.position + Vector3.down * .5f, 4);
            if (speed < -11 && GameApp.Instance?.Level != null) GameApp.Instance.Level.cameraRig.Shake(.065f);
        }
        void LateUpdate()
        {
            if (motor.Suspended) return;
            if (Time.time > nextBlink) { blinkEnd = Time.time + .11f; nextBlink = Time.time + Random.Range(3.2f, 6.5f); }
            body.sprite = Time.time < blinkEnd ? blink : normal;
            var v = motor.Body.linearVelocity;
            if (Mathf.Abs(v.x) > .2f) body.flipX = v.x > 0;
            land = Mathf.Max(0, land - Time.deltaTime);
            float squash = land > 0 ? Mathf.Sin(land / .18f * Mathf.PI) * .2f : 0;
            float stretch = !motor.Grounded && v.y > 1 ? .09f : !motor.Grounded ? -.04f : 0;
            body.transform.localPosition = new Vector3(-v.x * .008f, Mathf.Round(Mathf.Sin(Time.time * 2.5f) * 1.3f) / 32f, 0);
            arrival = Mathf.Max(0, arrival - Time.deltaTime);
            float grow = Mathf.SmoothStep(.3f, 1, 1 - arrival / .22f);
            body.transform.localScale = new Vector3(1 + squash - stretch, 1 - squash + stretch, 1) * grow;
            body.transform.localRotation = Quaternion.Euler(0, 0, -v.x * 1.4f);
        }
        void OnDestroy() { if (motor != null) motor.Landed -= OnLand; }
    }
}
