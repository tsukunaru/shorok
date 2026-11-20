using UnityEngine;
namespace Shorok
{
    public sealed class MusicBoxPossessable : Possessable
    {
        public float duration = 18;
        public float Remaining { get; private set; }
        public override string ActionKey => "action.music";
        public override void HandleInput(InputRouter input) { if (input.ActionPressed) { Remaining = duration; Active = true; GameApp.Instance.Audio.Play(5); GameApp.Instance.Level.Particles.Burst(transform.position, 14); } }
        protected override void Update() { base.Update(); Remaining = Mathf.Max(0, Remaining - Time.deltaTime); Active = Remaining > 0; if (detail != null && Active) detail.Rotate(0, 0, 45 * Time.deltaTime); }
        public override void ResetObject() { base.ResetObject(); Remaining = 0; }
    }
}
