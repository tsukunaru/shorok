using UnityEngine;
namespace Shorok
{
    public enum PuzzleKind { Awakening, Lamp, Car, Fan, Clock, Heavy, Music, LampCar, FanCrate, ClockHeavy, Finale }
    [System.Serializable]
    public sealed class RoomDefinition
    {
        public string key;
        public PuzzleKind puzzle;
        public bool memory;
        public string storyRu, storyEn;
    }
    [CreateAssetMenu(menuName = "Shorok/Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public int index;
        public string sceneName, nameRu, nameEn;
        public RoomDefinition[] rooms;
    }
}
