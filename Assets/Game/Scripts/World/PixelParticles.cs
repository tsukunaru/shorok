using UnityEngine;
namespace Shorok
{
    public sealed class PixelParticles : MonoBehaviour
    {
        readonly SpriteRenderer[] particles = new SpriteRenderer[64];
        readonly Vector2[] velocity = new Vector2[64];
        readonly float[] life = new float[64];
        int cursor;
        public void Initialize(Sprite pixel)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                var obj = new GameObject("Dust"); obj.transform.SetParent(transform); var s = obj.AddComponent<SpriteRenderer>();
                s.sprite = pixel; s.sortingOrder = 25; s.enabled = false; particles[i] = s;
            }
        }
        public void Burst(Vector2 p, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int n = cursor++ % particles.Length; if (particles[n] == null) return;
                particles[n].transform.position = p; particles[n].enabled = true; life[n] = Random.Range(.3f, .8f);
                velocity[n] = Random.insideUnitCircle * 2; particles[n].transform.localScale = Vector3.one * Random.Range(1, 3);
            }
        }
        void Update()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (life[i] <= 0 || particles[i] == null) continue;
                life[i] -= Time.deltaTime; particles[i].transform.position += (Vector3)velocity[i] * Time.deltaTime;
                particles[i].color = new Color(1, 1, 1, Mathf.Clamp01(life[i] * 2)); particles[i].enabled = life[i] > 0;
            }
        }
    }
}
