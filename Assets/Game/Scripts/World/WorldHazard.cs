using UnityEngine;
namespace Shorok
{
    public enum HazardKind { Shadow, Steam, Gear, Barrier }
    public sealed class WorldHazard : MonoBehaviour, ITimeAffected
    {
        public HazardKind kind;
        public ClockPossessable clock;
        public Possessable calmSource;
        public float interval = 3.8f;
        public float LocalTimeScale { get; set; } = 1;
        SpriteRenderer visual;
        Collider2D trigger;
        float phase;
        void Awake() { visual = GetComponent<SpriteRenderer>(); trigger = GetComponent<Collider2D>(); }
        void Update()
        {
            LocalTimeScale = clock != null && clock.Active ? .25f : 1;
            phase += Time.deltaTime * LocalTimeScale;
            bool danger = calmSource == null || !calmSource.Active;
            if (kind == HazardKind.Steam || kind == HazardKind.Gear) danger &= phase % interval < interval * .48f;
            trigger.enabled = danger; visual.color = new Color(1, 1, 1, danger ? .9f : .16f);
            if (kind == HazardKind.Gear) transform.Rotate(0, 0, -70 * Time.deltaTime * LocalTimeScale);
        }
        void OnTriggerStay2D(Collider2D other) { if (other.GetComponent<GhostMotor>() != null) GameApp.Instance.Respawn(); }
    }
}
