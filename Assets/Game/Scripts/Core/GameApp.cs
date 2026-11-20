using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Shorok
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameApp : MonoBehaviour
    {
        public static GameApp Instance { get; private set; }
        public GameConfig Config;
        public GameState State { get; } = new GameState();
        public SaveData Data { get; private set; }
        public InputRouter Input { get; private set; }
        public GameUI UI { get; private set; }
        public GameAudio Audio { get; private set; }
        public PlatformService Platform { get; private set; }
        public LevelRoot Level { get; private set; }
        public Localization Text { get; private set; }
        public bool UsesTouch => Application.isMobilePlatform || (Platform != null && Platform.TouchDevice) || Data.settings.touchAlways;
        AdPolicy adPolicy;
        float pendingSave = -1, resetHold, sessionPlayTime;
        bool respawning, loading, wasPlaying;
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject); Application.targetFrameRate = 60;
            Input = new InputRouter(); Data = SaveSystem.Load(); Text = new Localization();
            Text.Language = Data.settings.language == "" ? (Application.systemLanguage == SystemLanguage.Russian ? "ru" : "en") : Data.settings.language;
            Audio = gameObject.AddComponent<GameAudio>(); Audio.Initialize(Config); Audio.Apply(Data.settings, false);
            UI = gameObject.AddComponent<GameUI>(); UI.Initialize(this);
            Platform = Config.platformPrefab != null ? Instantiate(Config.platformPrefab, transform).GetComponent<PlatformService>() : gameObject.AddComponent<LocalPlatformService>();
            adPolicy = new AdPolicy(Config.adFirstDelay, Config.adCooldown); State.Changed += ApplyState;
        }
        IEnumerator Start()
        {
            Platform.Initialize(); float deadline = Time.realtimeSinceStartup + 10;
            while (!Platform.Initialized && Time.realtimeSinceStartup < deadline) yield return null;
            Data = SaveSystem.Resolve(Data, SaveSystem.Parse(Platform.CloudJson));
            if (Data.settings.language == "") Text.Language = Platform.Language == "ru" ? "ru" : "en"; else Text.Language = Data.settings.language;
            yield return SceneManager.LoadSceneAsync("MainMenu"); State.SetPhase(GamePhase.Menu); Audio.SetMusic(0); UI.MainMenu();
            yield return null; Platform.Ready();
        }
        void Update()
        {
            if (Input == null) return; Input.Sample();
            bool portrait = Screen.height > Screen.width && UsesTouch;
            State.SetPause(PauseReason.Orientation, portrait); UI.Orientation(portrait);
            if (State.Phase == GamePhase.Playing && Input.PausePressed) TogglePause();
            if (State.Playing)
            {
                Data.playTime += Time.unscaledDeltaTime;
                sessionPlayTime += Time.unscaledDeltaTime;
                resetHold = Input.ResetHeld ? resetHold + Time.unscaledDeltaTime : 0;
                if (resetHold > .75f) { resetHold = 0; ResetRoom(); }
            }
            if (pendingSave >= 0 && Time.unscaledTime >= pendingSave) { pendingSave = -1; Platform.SaveCloud(JsonUtility.ToJson(Data)); }
        }
        void ApplyState()
        {
            bool playing = State.Playing;
            Time.timeScale = playing || State.Phase == GamePhase.Menu || State.Phase == GamePhase.Ending ? 1 : 0;
            if ((State.Reasons & ~PauseReason.Manual) != 0) Time.timeScale = 0;
            Audio.Apply(Data.settings, State.Reasons != PauseReason.None || State.Phase == GamePhase.Loading);
            if (!playing) Input.ClearTouch();
            if (wasPlaying != playing) { Platform.Gameplay(playing); wasPlaying = playing; }
        }
        public void TogglePause()
        {
            bool open = (State.Reasons & PauseReason.Manual) == 0;
            State.SetPause(PauseReason.Manual, open); if (open) { Save(); UI.PauseMenu(); } else UI.Hud();
        }
        public void NewGame()
        {
            var settings = Data.settings; int revision = Data.saveRevision;
            Data = new SaveData { settings = settings, saveRevision = revision, started = true };
            SaveSystem.Clear(); Save(); StartCoroutine(LoadArea(0, 0));
        }
        public void Continue() => StartCoroutine(LoadArea(Data.area, Data.checkpoint));
        public IEnumerator LoadArea(int area, int checkpoint)
        {
            if (loading) yield break; loading = true; State.SetPhase(GamePhase.Loading); UI.Loading();
            State.SetPause(PauseReason.Manual | PauseReason.Transition | PauseReason.Respawn, false);
            yield return SceneManager.LoadSceneAsync(Config.levels[area].sceneName);
            Level = Object.FindFirstObjectByType<LevelRoot>();
            if (Level == null) { Debug.LogError("Shorok: missing LevelRoot in " + Config.levels[area].sceneName); loading = false; yield break; }
            Level.Initialize(checkpoint); Data.area = area; Data.checkpoint = Level.CurrentRoom; Data.started = true;
            Audio.SetMusic(area + 1); UI.Hud(); State.SetPhase(GamePhase.Playing); loading = false;
            UI.Story(Level.Room.Definition.storyRu, Level.Room.Definition.storyEn); Save();
        }
        public void Save() { SaveSystem.Write(Data); pendingSave = Time.unscaledTime + 3; }
        public void SettingsChanged() { Audio.Apply(Data.settings, State.Reasons != PauseReason.None); Save(); }
        public void ResetRoom() { if (Level != null) { Level.ResetCurrent(); UI.Toast("room.reset"); } }
        public void Respawn()
        {
            if (respawning || loading || !State.Playing) return; StartCoroutine(RespawnRoutine());
        }
        IEnumerator RespawnRoutine()
        {
            respawning = true; State.SetPause(PauseReason.Respawn, true); UI.Fade(true);
            yield return new WaitForSecondsRealtime(.35f);
            Level.Ghost.Possession.ForceExit(Level.Room.Spawn); Level.cameraRig.Snap();
            yield return new WaitForSecondsRealtime(.2f);
            UI.Fade(false); State.SetPause(PauseReason.Respawn, false); respawning = false;
        }
        public void CompleteArea() { if (!loading) StartCoroutine(AreaComplete()); }
        IEnumerator AreaComplete()
        {
            loading = true; State.SetPause(PauseReason.Transition, true); Save(); UI.Fade(true);
            yield return new WaitForSecondsRealtime(.6f);
            if (Data.area == 4)
            {
                Data.completed = true; Save(); Level = null;
                yield return SceneManager.LoadSceneAsync("Ending"); State.SetPause(PauseReason.Transition, false);
                State.SetPhase(GamePhase.Ending); Audio.SetMusic(5); UI.Cinematic();
                yield return new WaitForSeconds(12);
                if (Data.memories.Count == 20)
                {
                    UI.Story(Text.Get("ending.extra"), Text.Get("ending.extra"), 15);
                    yield return new WaitForSeconds(15);
                }
                UI.Ending(); loading = false; yield break;
            }
            Data.area++; Data.checkpoint = 0; Data.unlockedAreas = Mathf.Max(Data.unlockedAreas, Data.area + 1); Save();
            if (adPolicy.CanShow(sessionPlayTime))
            {
                bool done = false; State.SetPause(PauseReason.Advertisement, true); adPolicy.Record(sessionPlayTime);
                Platform.Advertisement(false, _ => done = true);
                while (!done) yield return null;
                State.SetPause(PauseReason.Advertisement, false);
            }
            loading = false; yield return LoadArea(Data.area, 0); UI.Fade(false);
        }
        public void RewardHint()
        {
            if ((State.Reasons & PauseReason.Advertisement) != 0) return;
            State.SetPause(PauseReason.Advertisement, true);
            Platform.Advertisement(true, rewarded => { State.SetPause(PauseReason.Advertisement, false); if (rewarded && Level != null) Level.Room.Hint(); else UI.Toast("ad.unavailable"); });
        }
        public void MainMenu() { if (!loading) StartCoroutine(MenuRoutine()); }
        IEnumerator MenuRoutine()
        {
            Save(); State.SetPhase(GamePhase.Loading); State.SetPause(PauseReason.Manual, false);
            Level = null; yield return SceneManager.LoadSceneAsync("MainMenu"); State.SetPhase(GamePhase.Menu); Audio.SetMusic(0); UI.MainMenu();
        }
        void OnApplicationFocus(bool focus) { if (Input != null) State.SetPause(PauseReason.Focus, !focus); }
        void OnApplicationPause(bool pause) { if (Input != null) { State.SetPause(PauseReason.Focus, pause); if (pause) Save(); } }
        void OnApplicationQuit() { if (Data != null) SaveSystem.Write(Data); }
        void OnDestroy() { if (Instance != this) return; State.Changed -= ApplyState; Input.Dispose(); Time.timeScale = 1; AudioListener.pause = false; Instance = null; }
    }
}
