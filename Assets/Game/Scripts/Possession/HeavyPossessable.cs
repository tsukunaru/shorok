using UnityEngine;
namespace Shorok
{
    public sealed class HeavyPossessable : Possessable
    {
        float move;
        public bool Stomping { get; private set; }
        public override string ActionKey => "action.heavy";
        public override void HandleInput(InputRouter input) { move = input.Move; if (input.ActionPressed) { Stomping = true; GameApp.Instance.Audio.Play(7); } }
        void FixedUpdate() { if (body != null) body.linearVelocity = new Vector2(Mathf.MoveTowards(body.linearVelocity.x, Possessed ? move * 1.65f : 0, 16 * Time.fixedDeltaTime), body.linearVelocity.y); }
        public override void ResetObject() { base.ResetObject(); Stomping = false; }
    }
}
