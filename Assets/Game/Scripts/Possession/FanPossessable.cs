using UnityEngine;
namespace Shorok
{
    public sealed class FanPossessable : Possessable
    {
        public Vector2 windSize = new Vector2(2.5f, 8);
        public Vector2 windOffset = new Vector2(1.5f, 3);
        public Rigidbody2D lightCrate;
        public int direction = 1;
        public override string ActionKey => "action.fan";
        public override void HandleInput(InputRouter input) { base.HandleInput(input); if (input.SecondaryPressed) direction = -direction; }
        protected override void Update() { base.Update(); if (Active && detail != null) detail.Rotate(0, 0, -360 * Time.deltaTime); }
        void FixedUpdate()
        {
            if (!Active || GameApp.Instance?.Level == null) return;
            var ghost = GameApp.Instance.Level.Ghost;
            Vector2 delta = (Vector2)ghost.transform.position - ((Vector2)transform.position + windOffset);
            if (Mathf.Abs(delta.x) < windSize.x / 2 && Mathf.Abs(delta.y) < windSize.y / 2) ghost.Wind = 60;
            if (lightCrate != null && Mathf.Abs(lightCrate.position.x - transform.position.x) < 12)
                lightCrate.linearVelocity = new Vector2(Mathf.MoveTowards(lightCrate.linearVelocity.x, direction * 2.3f, 5 * Time.fixedDeltaTime), lightCrate.linearVelocity.y);
        }
    }
}
