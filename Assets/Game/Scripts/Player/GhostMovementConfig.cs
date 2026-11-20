using UnityEngine;
namespace Shorok
{
    [CreateAssetMenu(menuName = "Shorok/Movement")]
    public sealed class GhostMovementConfig : ScriptableObject
    {
        public float speed = 4.5f, groundAcceleration = 36, airAcceleration = 24, deceleration = 42;
        public float jumpSpeed = 9.7f, gravity = 24, fallMultiplier = 1.35f, lowJumpMultiplier = 1.8f;
        public float coyoteTime = .12f, jumpBuffer = .14f, maxFallSpeed = 13, hoverDuration = .35f;
        public float interactionRadius = 1.8f;
    }
}
