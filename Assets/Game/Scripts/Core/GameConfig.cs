using UnityEngine;

namespace Shorok
{
    [CreateAssetMenu(menuName = "Shorok/Game config")]
    public sealed class GameConfig : ScriptableObject
    {
        public string title = "ШОРОХ";
        public string englishTitle = "SHOROK";
        public GhostMovementConfig movement;
        public LevelDefinition[] levels;
        public Font font;
        public Sprite ghost, blink, pixel, memory;
        public AudioClip[] effects;
        public AudioClip[] music;
        public GameObject ghostPrefab, platformPrefab;
        public float adFirstDelay = 180, adCooldown = 180;
        public int totalMemories = 20;
    }
}
