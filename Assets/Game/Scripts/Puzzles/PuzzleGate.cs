using UnityEngine;
namespace Shorok
{
    public sealed class PuzzleGate : MonoBehaviour
    {
        public PressurePlate plate;
        public Possessable source;
        public RoomController room;
        public bool brittle;
        public HeavyPossessable heavy;
        Vector3 closed;
        Collider2D solid;
        float amount;
        void Awake() { closed = transform.position; solid = GetComponent<Collider2D>(); }
        void Update()
        {
            bool open = room != null && room.Solved;
            if (plate != null) open |= plate.Latched;
            if (source != null) open |= source.Active;
            if (brittle && heavy != null) open |= heavy.Stomping && Vector2.Distance(heavy.transform.position, closed) < 2;
            amount = Mathf.MoveTowards(amount, open ? 1 : 0, Time.deltaTime * 2);
            transform.position = closed + Vector3.up * amount * 4;
            solid.enabled = amount < .8f;
        }
    }
}
