using System;
namespace Shorok
{
    public enum GamePhase { Boot, Menu, Loading, Playing, Ending }
    [Flags] public enum PauseReason { None = 0, Manual = 1, Platform = 2, Advertisement = 4, Orientation = 8, Focus = 16, Respawn = 32, Transition = 64 }
    public sealed class GameState
    {
        public GamePhase Phase { get; private set; } = GamePhase.Boot;
        public PauseReason Reasons { get; private set; }
        public bool Playing => Phase == GamePhase.Playing && Reasons == PauseReason.None;
        public event Action Changed;
        public void SetPhase(GamePhase phase) { if (Phase == phase) return; Phase = phase; Changed?.Invoke(); }
        public void SetPause(PauseReason reason, bool on)
        {
            var next = on ? Reasons | reason : Reasons & ~reason;
            if (next == Reasons) return;
            Reasons = next; Changed?.Invoke();
        }
    }
    public sealed class AdPolicy
    {
        readonly float first, cooldown;
        float last = float.NegativeInfinity;
        public AdPolicy(float firstDelay, float interval) { first = firstDelay; cooldown = interval; }
        public bool CanShow(float seconds) => seconds >= first && seconds - last >= cooldown;
        public void Record(float seconds) => last = seconds;
    }
}
