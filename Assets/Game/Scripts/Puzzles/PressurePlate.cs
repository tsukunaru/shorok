using UnityEngine;
namespace Shorok
{
    public sealed class PressurePlate : MonoBehaviour
    {
        public bool requireHeavy;
        public bool Latched { get; private set; }
        public SpriteRenderer visual;
        public void ResetPlate() { Latched = false; if (visual != null) visual.color = Color.gray; }
        void OnTriggerStay2D(Collider2D other)
        {
            if (Latched) return;
            if (requireHeavy ? other.GetComponent<HeavyPossessable>() == null : other.attachedRigidbody == null || other.GetComponent<GhostMotor>() != null) return;
            Latched = true; if (visual != null) visual.color = Color.white;
            GameApp.Instance.Audio.Play(6); GameApp.Instance.Level.Particles.Burst(transform.position, 8);
        }
    }
}
