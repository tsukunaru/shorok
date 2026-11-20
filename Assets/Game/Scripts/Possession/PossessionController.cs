using System.Collections;
using UnityEngine;
namespace Shorok
{
    public sealed class PossessionController : MonoBehaviour
    {
        public Possessable Current { get; private set; }
        public Possessable Nearest { get; private set; }
        public bool Transitioning { get; private set; }
        GhostMotor motor;
        GhostAnimator anim;
        InputRouter input;
        readonly Collider2D[] exits = new Collider2D[12];
        void Awake() { motor = GetComponent<GhostMotor>(); anim = GetComponent<GhostAnimator>(); }
        void Start() { input = GameApp.Instance.Input; }
        void Update()
        {
            if (input == null || GameApp.Instance == null || !GameApp.Instance.State.Playing || Transitioning) return;
            if (Current != null)
            {
                if (input.InteractPressed) TryExit(); else Current.HandleInput(input);
                return;
            }
            Possessable best = null; float distance = motor.config.interactionRadius;
            var objects = GameApp.Instance.Level.Possessables;
            foreach (var obj in objects)
            {
                if (obj == null || !obj.CanPossess) continue;
                float d = Vector2.Distance(transform.position, obj.transform.position);
                if (d >= distance) continue;
                if (Physics2D.Linecast(transform.position, obj.transform.position, 1 << 6).collider != null) continue;
                best = obj; distance = d;
            }
            if (Nearest != best) { if (Nearest != null) Nearest.Highlight(false); Nearest = best; if (Nearest != null) Nearest.Highlight(true); }
            if (input.InteractPressed && Nearest != null) StartCoroutine(Enter(Nearest));
        }
        public IEnumerator Enter(Possessable target)
        {
            if (Current != null || Transitioning || target == null || !target.CanPossess) yield break;
            Transitioning = true; motor.Suspended = true; motor.Body.linearVelocity = Vector2.zero; motor.Body.simulated = false;
            var start = transform.position; float elapsed = 0;
            GameApp.Instance.Audio.Play(2);
            while (elapsed < .24f)
            {
                elapsed += Time.deltaTime; float t = Mathf.Clamp01(elapsed / .24f);
                transform.position = Vector3.Lerp(start, target.transform.position, t * t);
                anim.body.transform.localScale = Vector3.one * (1 - t); yield return null;
            }
            Current = target; target.Enter(); anim.body.enabled = false; Transitioning = false;
            GameApp.Instance.Level.Particles.Burst(target.transform.position, 10);
        }
        public bool FindSafeExit(Possessable target, out Vector2 result)
        {
            Vector2 p = target.transform.position;
            Vector2[] candidates = { p + Vector2.up * 1.35f, p + new Vector2(-1.15f, .6f), p + new Vector2(1.15f, .6f), p + new Vector2(-1.8f, 1.2f), p + new Vector2(1.8f, 1.2f), p + Vector2.up * 2.1f };
            foreach (var candidate in candidates)
            {
                int n = Physics2D.OverlapCapsule(candidate, new Vector2(.72f, 1.05f), CapsuleDirection2D.Vertical, 0,
                    new ContactFilter2D { useLayerMask = true, layerMask = (1 << 6) | (1 << 7) | (1 << 8) | (1 << 10), useTriggers = true }, exits);
                if (n == 0) { result = candidate; return true; }
            }
            result = default; return false;
        }
        public bool TryExit()
        {
            if (Current == null || Transitioning) return false;
            if (!FindSafeExit(Current, out var p)) { GameApp.Instance.UI.Toast("hint.exit_blocked"); return false; }
            ForceExit(p); GameApp.Instance.Audio.Play(3); GameApp.Instance.Level.Particles.Burst(p, 8); return true;
        }
        public void ForceExit(Vector2 position)
        {
            StopAllCoroutines(); if (Current != null) Current.Exit(); Current = null; Transitioning = false;
            anim.body.enabled = true; anim.body.transform.localScale = Vector3.one; anim.Emerge();
            motor.Body.simulated = true; motor.Suspended = false; motor.Teleport(position);
        }
    }
}
