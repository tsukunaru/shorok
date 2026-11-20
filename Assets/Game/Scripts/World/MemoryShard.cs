using UnityEngine;
namespace Shorok
{
    public sealed class MemoryShard : MonoBehaviour
    {
        public string id;
        public Possessable revealSource;
        Vector3 origin;
        SpriteRenderer visual;
        void Start() { origin = transform.position; visual = GetComponent<SpriteRenderer>(); if (GameApp.Instance.Data.memories.Contains(id)) gameObject.SetActive(false); }
        void Update()
        {
            transform.position = origin + Vector3.up * Mathf.Round(Mathf.Sin(Time.time * 2.2f) * 3) / 32f;
            visual.color = new Color(1, 1, 1, revealSource != null && !revealSource.Active ? .14f : .8f + Mathf.Sin(Time.time * 3) * .2f);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<GhostMotor>() == null || (revealSource != null && !revealSource.Active)) return;
            var app = GameApp.Instance;
            if (!app.Data.memories.Contains(id)) { app.Data.memories.Add(id); app.Save(); app.Audio.Play(5); app.Level.Particles.Burst(transform.position, 16); app.UI.Toast("memory.found"); }
            gameObject.SetActive(false);
        }
    }
}
