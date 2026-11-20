using UnityEngine;
namespace Shorok
{
    public sealed class EndingDawn : MonoBehaviour
    {
        public SpriteRenderer background;
        public Transform hand;
        float elapsed;
        SpriteRenderer[] memories;
        void Start()
        {
            if (GameApp.Instance.Data.memories.Count != 20) return;
            memories = new SpriteRenderer[20];
            for (int i = 0; i < memories.Length; i++)
            {
                var spark = new GameObject("Remembered light").AddComponent<SpriteRenderer>();
                spark.transform.SetParent(transform, false); spark.transform.localScale = Vector3.one * .5f;
                spark.sprite = GameApp.Instance.Config.memory; spark.sortingOrder = 30; spark.enabled = false;
                memories[i] = spark;
            }
        }
        void Update()
        {
            elapsed += Time.deltaTime;
            float light = Mathf.SmoothStep(1, 1.85f, elapsed / 12);
            background.color = new Color(light, light, light, 1);
            if (hand != null) hand.Rotate(0, 0, -22 * Time.deltaTime);
            if (memories == null) return;
            float reunion = Mathf.Clamp01((elapsed - 12) / 15);
            for (int i = 0; i < memories.Length; i++)
            {
                float angle = i * Mathf.PI * .1f + elapsed * .28f;
                float radius = Mathf.Lerp(4, .1f, Mathf.SmoothStep(0, 1, reunion));
                memories[i].transform.position = new Vector3(17 + Mathf.Cos(angle) * radius, 1.9f + Mathf.Sin(angle) * radius * .35f, 0);
                memories[i].color = new Color(1, 1, 1, Mathf.Sin(reunion * Mathf.PI));
                memories[i].enabled = elapsed > 12 && reunion < 1;
            }
        }
    }
}
