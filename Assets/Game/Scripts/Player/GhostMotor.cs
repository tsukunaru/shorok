using UnityEngine;
namespace Shorok
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public sealed class GhostMotor : MonoBehaviour
    {
        public GhostMovementConfig config;
        public bool Grounded { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public PossessionController Possession { get; private set; }
        public float Wind { get; set; }
        public bool Suspended { get; set; }
        public event System.Action<float> Landed;
        CapsuleCollider2D capsule;
        ContactFilter2D groundFilter;
        readonly RaycastHit2D[] hits = new RaycastHit2D[6];
        float coyote, buffered, floatTime, previousFall;
        InputRouter input;
        MovingPlatform carrier;
        void Awake()
        {
            Body = GetComponent<Rigidbody2D>(); capsule = GetComponent<CapsuleCollider2D>();
            Possession = GetComponent<PossessionController>();
            groundFilter = new ContactFilter2D { useLayerMask = true, layerMask = (1 << 6) | (1 << 7) | (1 << 8), useTriggers = false };
        }
        void Start() { input = GameApp.Instance.Input; }
        void Update()
        {
            if (GameApp.Instance == null || !GameApp.Instance.State.Playing || Suspended) return;
            if (input.JumpPressed) buffered = config.jumpBuffer;
        }
        void FixedUpdate()
        {
            if (input == null || Suspended || GameApp.Instance == null || !GameApp.Instance.State.Playing) return;
            bool wasGrounded = Grounded; Grounded = false; carrier = null;
            int count = capsule.Cast(Vector2.down, groundFilter, hits, .09f);
            for (int i = 0; i < count; i++)
            {
                if (hits[i].normal.y < .6f || Body.linearVelocity.y > .5f) continue;
                Grounded = true; carrier = GameApp.Instance.Level.PlatformAt(hits[i].collider); break;
            }
            if (Grounded && !wasGrounded) { Landed?.Invoke(previousFall); if (previousFall < -3) GameApp.Instance.Audio.Play(1); }
            coyote = Grounded ? config.coyoteTime : coyote - Time.fixedDeltaTime;
            buffered -= Time.fixedDeltaTime;
            var velocity = Body.linearVelocity;
            float acceleration = Mathf.Abs(input.Move) < .01f ? config.deceleration : Grounded ? config.groundAcceleration : config.airAcceleration;
            velocity.x = Mathf.MoveTowards(velocity.x, input.Move * config.speed, acceleration * Time.fixedDeltaTime);
            if (buffered > 0 && coyote > 0)
            {
                velocity.y = config.jumpSpeed; coyote = buffered = 0; Grounded = false; floatTime = 0;
                GameApp.Instance.Audio.Play(0);
            }
            float g = config.gravity;
            if (velocity.y < 0)
            {
                g *= config.fallMultiplier;
                if (input.JumpHeld && floatTime < config.hoverDuration) { g *= .6f; floatTime += Time.fixedDeltaTime; }
            }
            else if (!input.JumpHeld) g *= config.lowJumpMultiplier;
            else if (velocity.y < 1.4f) g *= .7f;
            velocity.y = Mathf.Max(velocity.y - g * Time.fixedDeltaTime, -config.maxFallSpeed);
            if (Wind > 0) velocity.y = Mathf.MoveTowards(velocity.y, 6.3f, Wind * Time.fixedDeltaTime);
            Body.linearVelocity = velocity;
            if (carrier != null) Body.position += carrier.Delta;
            previousFall = velocity.y; Wind = 0;
            if (Body.position.y < -4) GameApp.Instance.Respawn();
        }
        public void Teleport(Vector2 position)
        {
            Body.position = position; transform.position = position; Body.linearVelocity = Vector2.zero;
            coyote = buffered = floatTime = 0; Grounded = false;
        }
    }
}
