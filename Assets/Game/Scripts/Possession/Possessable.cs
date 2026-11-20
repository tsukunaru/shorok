using UnityEngine;
namespace Shorok
{
    public abstract class Possessable : MonoBehaviour
    {
        public SpriteRenderer visual;
        public Transform detail;
        public string key;
        public bool Active { get; protected set; }
        public bool Possessed { get; private set; }
        public RoomController Room { get; set; }
        protected Vector3 origin;
        protected Rigidbody2D body;
        bool highlighted;
        public virtual bool CanPossess => enabled && gameObject.activeInHierarchy;
        public virtual string ActionKey => "action.toggle";
        public virtual float Reach => 1.8f;
        protected virtual void Awake() { origin = transform.position; body = GetComponent<Rigidbody2D>(); }
        public virtual void Enter() { Possessed = true; }
        public virtual void Exit() { Possessed = false; if (body != null) body.linearVelocity = Vector2.zero; }
        public virtual void HandleInput(InputRouter input) { if (input.ActionPressed) SetActive(!Active); }
        public virtual void SetActive(bool on) { Active = on; if (GameApp.Instance != null) GameApp.Instance.Audio.Play(4); }
        public virtual void ResetObject()
        {
            Exit(); Active = false; transform.position = origin;
            if (body != null) { body.position = origin; body.linearVelocity = Vector2.zero; }
        }
        public void Highlight(bool on) { highlighted = on; }
        protected virtual void Update()
        {
            if (visual != null)
            {
                float value = Possessed ? 1 : highlighted ? .88f + .12f * Mathf.Sin(Time.time * 5) : Active ? .96f : .75f;
                visual.color = new Color(value, value, value);
            }
            if (transform.position.y < -3) ResetObject();
        }
    }
}
