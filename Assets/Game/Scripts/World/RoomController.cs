using UnityEngine;
namespace Shorok
{
    public sealed class RoomController : MonoBehaviour
    {
        public int index;
        public Vector2 Spawn;
        public float Right, ExitHeight;
        public bool Solved { get; private set; }
        public RoomDefinition Definition { get; private set; }
        public string Id { get; private set; }
        Possessable[] objects;
        PressurePlate[] plates;
        MovingPlatform[] platforms;
        Rigidbody2D[] rigidbodies;
        Vector2[] positions;
        public void Initialize(LevelRoot level)
        {
            Definition = level.definition.rooms[index]; Id = level.definition.sceneName + "." + index;
            Solved = GameApp.Instance.Data.solved.Contains(Id);
            objects = GetComponentsInChildren<Possessable>(true);
            foreach (var obj in objects) obj.Room = this;
            plates = GetComponentsInChildren<PressurePlate>(true); platforms = GetComponentsInChildren<MovingPlatform>(true);
            rigidbodies = GetComponentsInChildren<Rigidbody2D>(true); positions = new Vector2[rigidbodies.Length];
            for (int i = 0; i < positions.Length; i++) positions[i] = rigidbodies[i].position;
        }
        public void Complete()
        {
            if (Solved) return; Solved = true; GameApp.Instance.Data.solved.Add(Id); GameApp.Instance.Save();
        }
        public void ResetRoom()
        {
            Solved = false; GameApp.Instance.Data.solved.Remove(Id);
            foreach (var obj in objects) obj.ResetObject(); foreach (var plate in plates) plate.ResetPlate();
            foreach (var platform in platforms) platform.ResetMotion();
            for (int i = 0; i < positions.Length; i++) { rigidbodies[i].position = positions[i]; rigidbodies[i].linearVelocity = Vector2.zero; }
            GameApp.Instance.Save();
        }
        public void Hint()
        {
            foreach (var obj in objects) obj.Highlight(true);
            GameApp.Instance.UI.Toast("hint." + Definition.puzzle.ToString().ToLowerInvariant(), 7);
        }
    }
}
