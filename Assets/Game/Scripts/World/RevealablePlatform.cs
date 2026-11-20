using UnityEngine;
namespace Shorok
{
    public sealed class RevealablePlatform : MonoBehaviour
    {
        public Possessable source;
        public RoomController room;
        public SpriteRenderer visual;
        public Collider2D solid;
        public float Visibility { get; private set; }
        void Update()
        {
            bool show = (source != null && source.Active) || (room != null && room.Solved);
            Visibility = Mathf.MoveTowards(Visibility, show ? 1 : 0, Time.deltaTime * 5);
            visual.color = new Color(1, 1, 1, Mathf.Lerp(.12f, 1, Visibility));
            solid.enabled = Visibility >= .65f;
        }
    }
}
