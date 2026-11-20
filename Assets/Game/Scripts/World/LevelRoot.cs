using UnityEngine;
namespace Shorok
{
    public sealed class LevelRoot : MonoBehaviour
    {
        public LevelDefinition definition;
        public GhostMotor Ghost;
        public PixelCamera cameraRig;
        public PixelParticles Particles;
        public RoomController[] rooms;
        public Possessable[] Possessables { get; private set; }
        public int CurrentRoom { get; private set; }
        public RoomController Room => rooms[CurrentRoom];
        readonly System.Collections.Generic.Dictionary<Collider2D, MovingPlatform> carriers = new System.Collections.Generic.Dictionary<Collider2D, MovingPlatform>();
        public MovingPlatform PlatformAt(Collider2D collider) => carriers.TryGetValue(collider, out var platform) ? platform : null;
        public void Initialize(int checkpoint)
        {
            Possessables = GetComponentsInChildren<Possessable>(true);
            carriers.Clear();
            foreach (var platform in GetComponentsInChildren<MovingPlatform>(true)) carriers[platform.GetComponent<Collider2D>()] = platform;
            foreach (var room in rooms) room.Initialize(this);
            CurrentRoom = Mathf.Clamp(checkpoint, 0, rooms.Length - 1);
            Ghost.Teleport(Room.Spawn); cameraRig.Initialize(this); Particles.Initialize(GameApp.Instance.Config.pixel);
        }
        void Update()
        {
            var app = GameApp.Instance;
            if (app == null || app.Level != this || !app.State.Playing) return;
            Vector2 position = Ghost.Possession.Current != null ? Ghost.Possession.Current.transform.position : Ghost.transform.position;
            if (CurrentRoom > 0 && position.x < rooms[CurrentRoom - 1].Right - 1.75f)
            {
                CurrentRoom--; app.Data.checkpoint = CurrentRoom; app.Save();
            }
            if (Ghost.Possession.Current == null && position.x > Room.Right - 1 && position.y > Room.ExitHeight - .8f)
            {
                Room.Complete();
                if (CurrentRoom + 1 < rooms.Length)
                {
                    CurrentRoom++; app.Data.checkpoint = CurrentRoom; app.Save();
                    app.Audio.Play(6); app.UI.Story(Room.Definition.storyRu, Room.Definition.storyEn);
                }
                else app.CompleteArea();
            }
        }
        public void ResetCurrent() { Room.ResetRoom(); Ghost.Possession.ForceExit(Room.Spawn); cameraRig.Snap(); }
    }
}
