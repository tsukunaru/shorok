using UnityEngine;
namespace Shorok
{
    public sealed class ToyCarPossessable : Possessable
    {
        float move;
        public override string ActionKey => "action.drive";
        public override void HandleInput(InputRouter input) { move = input.Move; }
        void FixedUpdate()
        {
            if (body == null) return;
            body.linearVelocity = new Vector2(Mathf.MoveTowards(body.linearVelocity.x, Possessed ? move * 6 : 0, 22 * Time.fixedDeltaTime), body.linearVelocity.y);
            if (detail != null) detail.Rotate(0, 0, -body.linearVelocity.x * 80 * Time.fixedDeltaTime);
        }
    }
}
