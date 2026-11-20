using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Shorok;
using YG;

public sealed class YandexPlatformService : PlatformService
{
    [Serializable] sealed class InitData { public string language, save; public bool online, touch; }
    bool online;
    Action<bool> adComplete;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void ShorokInit(string target);
    [DllImport("__Internal")] static extern void ShorokSave(string json);
    [DllImport("__Internal")] static extern void ShorokAd(bool rewarded);
#endif
    public override void Initialize()
    {
        gameObject.name = "ShorokPlatform";
#if UNITY_WEBGL && !UNITY_EDITOR
        ShorokInit(gameObject.name);
#else
        Language = Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en"; Initialized = true;
#endif
    }
    public void OnPlatformInitialized(string json)
    {
        if (Initialized) return;
        try { var result = JsonUtility.FromJson<InitData>(json); Language = result.language; CloudJson = result.save; online = result.online; TouchDevice = result.touch; }
        catch (Exception e) { Debug.LogWarning("Shorok: platform fallback: " + e.Message); }
        Initialized = true;
    }
    public override void Ready() { if (online) YG2.GameReadyAPI(); }
    public override void Gameplay(bool playing) { if (!online) return; if (playing) YG2.GameplayStart(); else YG2.GameplayStop(); }
    public override void SaveCloud(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (online && !SaveSystem.FutureLocalSave) ShorokSave(json);
#endif
    }
    public override void Advertisement(bool rewarded, Action<bool> complete)
    {
        if (adComplete != null || !online) { complete(false); return; }
        adComplete = complete;
#if UNITY_WEBGL && !UNITY_EDITOR
        ShorokAd(rewarded);
#else
        OnAdClosed("false");
#endif
    }
    public void OnAdClosed(string rewarded) { var callback = adComplete; adComplete = null; callback?.Invoke(rewarded == "true"); }
    public void OnPlatformPause(string paused) => GameApp.Instance.State.SetPause(PauseReason.Platform, paused == "true");
    public void OnVisibility(string visible) => GameApp.Instance.State.SetPause(PauseReason.Focus, visible != "true");
}
