using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace Shorok
{
    public sealed class GameUI : MonoBehaviour
    {
        GameApp app;
        CanvasScaler scaler;
        RectTransform canvas, page, overlays;
        GameObject orientation, fade, touch;
        Image fadeImage;
        float fadeTarget;
        Text hud, context, toast, touchInteract, touchAction;
        GameObject touchInteractRoot, touchActionRoot, touchReverseRoot;
        float toastUntil;
        bool hudVisible;
        string currentPage;
        readonly Color ink = new Color(.055f, .055f, .065f, .95f), pale = new Color(.91f, .91f, .88f);
        public void Initialize(GameApp owner)
        {
            app = owner;
            var go = new GameObject("Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform); canvas = (RectTransform)go.transform;
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(960, 540);
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); events.transform.SetParent(transform);
            page = Rect("Page", canvas, Vector2.zero, Vector2.zero); Stretch(page);
            overlays = Rect("Overlays", canvas, Vector2.zero, Vector2.zero); Stretch(overlays);
            toast = Label(overlays, "", new Vector2(0, 174), new Vector2(800, 68), 12, TextAnchor.MiddleCenter);
            fade = Panel(overlays, "Fade", Vector2.zero, new Vector2(3000, 2000), Color.clear).gameObject;
            fadeImage = fade.GetComponent<Image>(); fade.SetActive(false);
            orientation = Panel(overlays, "Orientation", Vector2.zero, new Vector2(2000, 2000), ink).gameObject;
            Label((RectTransform)orientation.transform, "", Vector2.zero, new Vector2(700, 120), 20, TextAnchor.MiddleCenter); orientation.SetActive(false);
        }
        string T(string key) => app.Text.Get(key);
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
            r.anchorMin = r.anchorMax = new Vector2(.5f, .5f); r.sizeDelta = size; r.anchoredPosition = position; return r;
        }
        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.sizeDelta = Vector2.zero; r.anchoredPosition = Vector2.zero; }
        RectTransform Panel(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var r = Rect(name, parent, pos, size); var image = r.gameObject.AddComponent<Image>(); image.color = color; return r;
        }
        Text Label(Transform parent, string text, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var r = Rect("Text", parent, pos, size); var t = r.gameObject.AddComponent<Text>();
            t.font = app.Config.font; t.fontSize = fontSize; t.color = pale; t.text = text; t.alignment = anchor;
            t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.lineSpacing = 1.5f; return t;
        }
        Button Button(string key, Vector2 pos, Action action, float width = 336)
        {
            var r = Panel(page, key, pos, new Vector2(width, 42), new Color(.15f, .15f, .16f, .96f));
            var outline = r.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(.45f, .45f, .45f); outline.effectDistance = new Vector2(1, -1);
            var b = r.gameObject.AddComponent<Button>(); b.onClick.AddListener(() => { app.Audio.Play(4); action(); });
            var colors = b.colors; colors.highlightedColor = new Color(.7f, .7f, .7f); colors.pressedColor = Color.gray; b.colors = colors;
            Label(r, T(key), Vector2.zero, new Vector2(width - 16, 40), 12, TextAnchor.MiddleCenter); return b;
        }
        void Clear(string pageName)
        {
            foreach (Transform child in page) Destroy(child.gameObject);
            hudVisible = false; hud = context = null; touch = null; currentPage = pageName;
            toast.text = ""; toastUntil = 0;
            Fade(false);
        }
        void Card(string title)
        {
            Panel(page, "Shade", Vector2.zero, new Vector2(3000, 2000), new Color(0, 0, 0, .68f));
            Panel(page, "Card", Vector2.zero, new Vector2(584, 470), ink);
            Label(page, title, new Vector2(0, 191), new Vector2(550, 45), 20, TextAnchor.MiddleCenter);
            Panel(page, "Rule", new Vector2(0, 155), new Vector2(480, 2), Color.gray);
        }
        public void MainMenu()
        {
            Clear("menu");
            Panel(page, "Title shadow", new Vector2(-220, 0), new Vector2(475, 540), new Color(.035f, .035f, .04f, .88f));
            Label(page, (app.Data.area + 1).ToString("00") + " / 05     " + T("menu.chapter"), new Vector2(-213, 198), new Vector2(370, 38), 9);
            Label(page, app.Text.Language == "ru" ? app.Config.title : app.Config.englishTitle, new Vector2(-215, 120), new Vector2(380, 70), 44);
            Label(page, T("menu.subtitle"), new Vector2(-215, 57), new Vector2(380, 40), 13);
            float y = -18;
            if (app.Data.started) { Button("menu.continue", new Vector2(-220, y), app.Continue); y -= 52; }
            if (app.Data.completed) { Button("ending.revisit", new Vector2(-220, y), ChooseArea); y -= 52; }
            Button("menu.new", new Vector2(-220, y), () => { if (app.Data.started) ConfirmNew(); else app.NewGame(); }); y -= 52;
            Button("menu.settings", new Vector2(-220, y), Settings); y -= 52;
            Button("menu.how", new Vector2(-220, y), How);
            Label(page, T("menu.tagline"), new Vector2(245, -220), new Vector2(390, 60), 11, TextAnchor.MiddleCenter);
        }
        void ConfirmNew() { Clear("confirm"); Card(T("menu.new")); Label(page, T("menu.confirm"), new Vector2(0, 60), new Vector2(490, 130), 14, TextAnchor.MiddleCenter); Button("yes", new Vector2(0, -60), app.NewGame); Button("no", new Vector2(0, -116), MainMenu); }
        public void Hud()
        {
            Clear("hud"); hudVisible = true;
            hud = Label(page, "", new Vector2(-160, 234), new Vector2(600, 44), 11);
            context = Label(page, "", new Vector2(0, -139), new Vector2(800, 65), 11, TextAnchor.MiddleCenter);
            Button("II", new Vector2(420, 228), app.TogglePause, 54);
            touch = new GameObject("Touch", typeof(RectTransform)); touch.transform.SetParent(page, false);
            Touch(0, "<", new Vector2(-405, -212)); Touch(1, ">", new Vector2(-313, -212));
            Touch(2, T("touch.jump"), new Vector2(405, -212));
            touchInteract = Touch(3, T("touch.possess"), new Vector2(295, -212)); touchInteractRoot = touchInteract.transform.parent.gameObject;
            touchAction = Touch(4, T("touch.action"), new Vector2(185, -212)); touchActionRoot = touchAction.transform.parent.gameObject;
            touchReverseRoot = Touch(5, T("touch.reverse"), new Vector2(405, -115)).transform.parent.gameObject;
        }
        Text Touch(int kind, string caption, Vector2 position)
        {
            var r = Panel(touch.transform, "Touch " + kind, position, new Vector2(kind < 2 ? 78 : 96, 74), new Color(.2f, .2f, .2f, .62f));
            var o = r.gameObject.AddComponent<Outline>(); o.effectColor = new Color(.8f, .8f, .8f, .65f); o.effectDistance = new Vector2(2, -2);
            r.gameObject.AddComponent<TouchButton>().kind = kind;
            return Label(r, caption, Vector2.zero, new Vector2(92, 60), kind < 2 ? 26 : 10, TextAnchor.MiddleCenter);
        }
        public void PauseMenu()
        {
            Clear("pause"); Card(T("pause.title"));
            Button("menu.continue", new Vector2(0, 112), app.TogglePause);
            Button("pause.restart", new Vector2(0, 62), () => { app.ResetRoom(); app.TogglePause(); });
            Button("pause.checkpoint", new Vector2(0, 12), () => { app.Level.Ghost.Possession.ForceExit(app.Level.Room.Spawn); app.TogglePause(); });
            Button("menu.settings", new Vector2(0, -38), Settings);
            Button("ad.hint", new Vector2(0, -88), app.RewardHint);
            Label(page, T("ad.detail"), new Vector2(0, -128), new Vector2(510, 30), 9, TextAnchor.MiddleCenter);
            Button("pause.menu", new Vector2(0, -183), app.MainMenu);
        }
        void Back() { if (app.State.Phase == GamePhase.Playing) PauseMenu(); else MainMenu(); }
        public void Settings()
        {
            Clear("settings"); Card(T("menu.settings")); var s = app.Data.settings;
            Slider("settings.master", 112, s.master, v => s.master = v);
            Slider("settings.music", 62, s.music, v => s.music = v);
            Slider("settings.sfx", 12, s.sfx, v => s.sfx = v);
            Toggle("settings.mute", -46, s.mute, v => s.mute = v, -135);
            Toggle("settings.shake", -46, s.shake, v => s.shake = v, 135);
            Toggle("settings.touch", -99, s.touchAlways, v => s.touchAlways = v, -135);
            Button("settings.language", new Vector2(135, -99), () => { s.language = app.Text.Language == "ru" ? "en" : "ru"; app.Text.Language = s.language; app.SettingsChanged(); Settings(); }, 250);
            Button("no", new Vector2(0, -180), Back);
        }
        void Toggle(string key, float y, bool value, Action<bool> change, float x)
        {
            var button = Button(key, new Vector2(x, y), () => { change(!value); app.SettingsChanged(); Settings(); }, 250);
            button.GetComponentInChildren<Text>().text = T(key) + ": " + T(value ? "on" : "off");
            button.GetComponentInChildren<Text>().fontSize = 10;
        }
        void Slider(string key, float y, float value, Action<float> change)
        {
            Label(page, T(key), new Vector2(-113, y), new Vector2(270, 34), 11);
            var r = Panel(page, key, new Vector2(140, y), new Vector2(208, 24), new Color(.23f, .23f, .23f));
            var slider = r.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1;
            var handle = Panel(r, "Handle", Vector2.zero, new Vector2(16, 28), pale); slider.handleRect = handle; slider.targetGraphic = handle.GetComponent<Image>();
            slider.value = value; slider.onValueChanged.AddListener(v => { change(v); app.SettingsChanged(); });
        }
        void How() { Clear("how"); Card(T("menu.how")); Label(page, T("how.body"), new Vector2(0, 0), new Vector2(510, 290), 11, TextAnchor.UpperLeft); Button("no", new Vector2(0, -184), Back); }
        public void Loading() { Clear("loading"); Card(T("loading")); }
        public void Cinematic() { Clear("cinematic"); Story(T("ending.body"), T("ending.body"), 12); }
        public void Ending()
        {
            Clear("ending"); Card(T("ending.title"));
            Label(page, T("ending.body"), new Vector2(0, 80), new Vector2(510, 90), 15, TextAnchor.MiddleCenter);
            Label(page, T("hud.memory") + "  " + app.Data.memories.Count + " / 20", new Vector2(0, -3), new Vector2(500, 45), 12, TextAnchor.MiddleCenter);
            if (app.Data.memories.Count == 20) Label(page, T("ending.extra"), new Vector2(0, -67), new Vector2(510, 90), 11, TextAnchor.MiddleCenter);
            Button("ending.revisit", new Vector2(0, -148), ChooseArea, 460); Button("pause.menu", new Vector2(0, -202), app.MainMenu);
        }
        void ChooseArea()
        {
            Clear("areas"); Card(T("ending.choose"));
            for (int i = 0; i < app.Config.levels.Length; i++)
            {
                int index = i; var b = Button("", new Vector2(0, 112 - i * 53), () => StartCoroutine(app.LoadArea(index, 0)), 460);
                b.GetComponentInChildren<Text>().text = app.Text.Language == "ru" ? app.Config.levels[i].nameRu : app.Config.levels[i].nameEn;
            }
            Button("no", new Vector2(0, -180), () => { if (app.State.Phase == GamePhase.Ending) Ending(); else MainMenu(); });
        }
        public void Orientation(bool show)
        {
            if (orientation == null) return;
            orientation.SetActive(show); if (show) orientation.GetComponentInChildren<Text>().text = T("orientation") + "\n\n" + T("orientation.sub");
        }
        public void Fade(bool on) { fadeTarget = on ? 1 : 0; if (on && fade != null) fade.SetActive(true); }
        public void Toast(string key, float duration = 3) { toast.rectTransform.anchoredPosition = new Vector2(0, currentPage == "hud" ? 174 : -249); toast.text = T(key); toastUntil = Time.unscaledTime + duration; }
        public void Story(string ru, string en, float duration = 5) { toast.rectTransform.anchoredPosition = new Vector2(0, currentPage == "cinematic" ? -180 : 174); toast.text = app.Text.Language == "ru" ? ru : en; toastUntil = Time.unscaledTime + duration; }
        void Update()
        {
            if (app == null) return;
            float alpha = Mathf.MoveTowards(fadeImage.color.a, fadeTarget, Time.unscaledDeltaTime * 5);
            fadeImage.color = new Color(0, 0, 0, alpha); fade.SetActive(alpha > 0 || fadeTarget > 0);
            scaler.matchWidthOrHeight = Screen.width * 540 >= Screen.height * 960 ? 1 : 0;
            if (Time.unscaledTime > toastUntil) toast.text = "";
            if (!hudVisible || hud == null || app.Level == null) return;
            var level = app.Level; var p = level.Ghost.Possession;
            hud.text = (app.Text.Language == "ru" ? level.definition.nameRu : level.definition.nameEn) + "   /   " + T("hud.memory") + " " + app.Data.memories.Count + "/20";
            bool mobile = app.UsesTouch;
            touch.SetActive(mobile && app.State.Playing);
            context.text = p.Current != null ? T("object." + p.Current.key) + "   " + T(p.Current.ActionKey) + "\n" + T("hint.exit") : p.Nearest != null ? T("hint.possess") + " : " + T("object." + p.Nearest.key) : "";
            if (mobile && p.Current != null) context.text = T("object." + p.Current.key);
            int flags = app.Data.tutorialFlags;
            if (level.definition.index == 0 && level.CurrentRoom == 0 && !mobile)
            {
                if ((flags & 1) == 0) { context.text = T("hint.awakening"); if (Mathf.Abs(app.Input.Move) > .1f) app.Data.tutorialFlags |= 1; }
                else if ((flags & 2) == 0) { context.text = T("hint.jump"); if (app.Input.JumpPressed) app.Data.tutorialFlags |= 2; }
            }
            if (p.Current != null) app.Data.tutorialFlags |= 4;
            if (flags != app.Data.tutorialFlags) app.Save();
            if (mobile)
            {
                touchInteractRoot.SetActive(p.Current != null || p.Nearest != null); touchActionRoot.SetActive(p.Current != null && !(p.Current is ToyCarPossessable)); touchReverseRoot.SetActive(p.Current is FanPossessable);
                touchInteract.text = T(p.Current != null ? "touch.leave" : "touch.possess");
            }
        }
    }
}
