using System;
using UnityEngine;
namespace Shorok
{
    public abstract class PlatformService : MonoBehaviour
    {
        public bool Initialized { get; protected set; }
        public string Language { get; protected set; } = "en";
        public string CloudJson { get; protected set; }
        public bool TouchDevice { get; protected set; }
        public abstract void Initialize();
        public abstract void Ready();
        public abstract void Gameplay(bool playing);
        public abstract void SaveCloud(string json);
        public abstract void Advertisement(bool rewarded, Action<bool> complete);
    }
    public sealed class LocalPlatformService : PlatformService
    {
        public override void Initialize() { Language = Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en"; Initialized = true; }
        public override void Ready() { }
        public override void Gameplay(bool playing) { }
        public override void SaveCloud(string json) { }
        public override void Advertisement(bool rewarded, Action<bool> complete) => complete(false);
    }
}
