using UnityEngine;
namespace Shorok
{
    public interface ITimeAffected { float LocalTimeScale { get; set; } }
    public sealed class ClockPossessable : Possessable
    {
        public override string ActionKey => "action.clock";
        protected override void Update() { base.Update(); if (detail != null) detail.Rotate(0, 0, -(Active ? 12 : 48) * Time.deltaTime); }
    }
}
