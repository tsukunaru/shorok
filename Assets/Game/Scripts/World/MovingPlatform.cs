using UnityEngine;
namespace Shorok
{
    [DefaultExecutionOrder(-20)]
    public sealed class MovingPlatform : MonoBehaviour, ITimeAffected
    {
        public Vector2 travel = new Vector2(3, 0);
        public float speed = .7f;
        public ClockPossessable clock;
        public float LocalTimeScale { get; set; } = 1;
        public Vector2 Delta { get; private set; }
        Vector2 origin;
        float phase;
        void Awake() { origin = transform.position; }
        void FixedUpdate()
        {
            LocalTimeScale = clock != null && clock.Active ? .25f : 1;
            phase += Time.fixedDeltaTime * speed * LocalTimeScale;
            Vector2 next = origin + travel * (.5f - .5f * Mathf.Cos(phase));
            Delta = next - (Vector2)transform.position; transform.position = next;
        }
        public void ResetMotion() { phase = 0; transform.position = origin; Delta = Vector2.zero; }
    }
}
