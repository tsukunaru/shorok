using UnityEngine;
namespace Shorok
{
    public sealed class LampPossessable : Possessable
    {
        public SpriteRenderer beam;
        protected override void Update() { base.Update(); if (beam != null) beam.enabled = Active; }
    }
}
