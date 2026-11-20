#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;
namespace Shorok
{
    public sealed class DebugOverlay : MonoBehaviour
    {
        bool visible;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create() { if (GameApp.Instance != null) GameApp.Instance.gameObject.AddComponent<DebugOverlay>(); }
        void Update() { if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) visible = !visible; }
        void OnGUI()
        {
            if (!visible || GameApp.Instance == null) return;
            var app = GameApp.Instance;
            GUILayout.BeginArea(new Rect(10, 60, 360, 340), GUI.skin.box);
            GUILayout.Label("SHOROK DEBUG | " + Mathf.RoundToInt(1 / Mathf.Max(.001f, Time.unscaledDeltaTime)) + " FPS");
            GUILayout.Label(app.State.Phase + " / " + app.State.Reasons + " / checkpoint " + app.Data.checkpoint);
            if (app.Level != null)
            {
                GUILayout.Label("Possession: " + (app.Level.Ghost.Possession.Current == null ? "ghost" : app.Level.Ghost.Possession.Current.key));
                if (GUILayout.Button("Restart room")) app.ResetRoom();
                if (GUILayout.Button("Return to checkpoint")) app.Level.Ghost.Possession.ForceExit(app.Level.Room.Spawn);
                if (GUILayout.Button("Complete room")) app.Level.Ghost.Teleport(new Vector2(app.Level.Room.Right - .5f, app.Level.Room.ExitHeight + 1));
            }
            if (GUILayout.Button("All memories")) { foreach (var level in app.Config.levels) for (int i = 0; i < level.rooms.Length; i++) if (level.rooms[i].memory) { string id = level.sceneName + ".memory." + i; if (!app.Data.memories.Contains(id)) app.Data.memories.Add(id); } app.Save(); }
            for (int i = 0; i < app.Config.levels.Length; i++) if (GUILayout.Button(app.Config.levels[i].sceneName)) StartCoroutine(app.LoadArea(i, 0));
            GUILayout.EndArea();
        }
    }
}
#endif
