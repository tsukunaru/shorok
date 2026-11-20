using System;
using System.Collections.Generic;
using UnityEngine;
namespace Shorok
{
    [Serializable] public sealed class UserSettings
    {
        public float master = .7f, music = .45f, sfx = .7f;
        public bool mute, shake = true, touchAlways;
        public string language = "";
    }
    [Serializable] public sealed class SaveData
    {
        public int schemaVersion = 1, saveRevision, area, checkpoint, unlockedAreas = 1, tutorialFlags;
        public bool started, completed;
        public float playTime;
        public long lastSaveTimestamp;
        public List<string> memories = new List<string>();
        public List<string> solved = new List<string>();
        public UserSettings settings = new UserSettings();
    }
    public static class SaveSystem
    {
        public const string Key = "shorok.save.v1";
        public static bool FutureLocalSave { get; private set; }
        public static SaveData Parse(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Length > 65536 || !json.TrimStart().StartsWith("{")) return null;
            try
            {
                var s = JsonUtility.FromJson<SaveData>(json);
                if (s == null || s.schemaVersion < 0 || s.schemaVersion > 1) return null;
                s.schemaVersion = 1; // v0 stored the same progress fields, without schema metadata.
                s.area = Mathf.Clamp(s.area, 0, 4); s.checkpoint = Mathf.Clamp(s.checkpoint, 0, 5);
                s.unlockedAreas = Mathf.Clamp(s.unlockedAreas, 1, 5);
                s.saveRevision = Math.Max(s.saveRevision, 0);
                s.memories ??= new List<string>(); s.solved ??= new List<string>(); s.settings ??= new UserSettings();
                s.memories = new List<string>(new HashSet<string>(s.memories));
                s.memories.RemoveAll(id => string.IsNullOrEmpty(id) || id.Length > 40);
                if (s.memories.Count > 20 || s.solved.Count > 40) return null;
                s.settings.master = FiniteVolume(s.settings.master);
                s.settings.music = FiniteVolume(s.settings.music);
                s.settings.sfx = FiniteVolume(s.settings.sfx);
                if (s.settings.language != "ru" && s.settings.language != "en") s.settings.language = "";
                if (float.IsNaN(s.playTime) || float.IsInfinity(s.playTime)) s.playTime = 0;
                return s;
            }
            catch (ArgumentException) { return null; }
        }
        static float FiniteVolume(float n) => float.IsNaN(n) || float.IsInfinity(n) ? .7f : Mathf.Clamp01(n);
        public static SaveData Resolve(SaveData local, SaveData cloud)
        {
            if (local == null) return cloud ?? new SaveData();
            if (cloud == null) return local;
            if (local.saveRevision != cloud.saveRevision) return local.saveRevision > cloud.saveRevision ? local : cloud;
            if (local.lastSaveTimestamp != cloud.lastSaveTimestamp) return local.lastSaveTimestamp > cloud.lastSaveTimestamp ? local : cloud;
            return Score(cloud) > Score(local) ? cloud : local;
        }
        static int Score(SaveData s) => (s.completed ? 10000 : 0) + s.unlockedAreas * 100 + s.checkpoint * 20 + s.memories.Count;
        public static SaveData Load()
        {
            var raw = PlayerPrefs.GetString(Key, "");
            FutureLocalSave = false;
            try { FutureLocalSave = !string.IsNullOrEmpty(raw) && JsonUtility.FromJson<SaveData>(raw)?.schemaVersion > 1; }
            catch (ArgumentException) { }
            return Parse(raw) ?? Parse(PlayerPrefs.GetString(Key + ".backup", "")) ?? new SaveData();
        }
        public static void Write(SaveData data)
        {
            if (FutureLocalSave) { Debug.LogWarning("Shorok: newer save schema preserved; progress not overwritten."); return; }
            string previous = PlayerPrefs.GetString(Key, "");
            if (Parse(previous) != null) PlayerPrefs.SetString(Key + ".backup", previous);
            data.saveRevision++; data.lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data)); PlayerPrefs.Save();
        }
        public static void Clear() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.DeleteKey(Key + ".backup"); PlayerPrefs.Save(); FutureLocalSave = false; }
    }
}
