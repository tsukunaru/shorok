using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.Rendering.Universal;

namespace Shorok.EditorTools
{
    public static class ShorokContentBuilder
    {
        const string Game = "Assets/Game/";
        static GameConfig config;
        static Material spriteMaterial;
        static Tile floorTile;
        static readonly Sprite[] backgrounds = new Sprite[5];
        static readonly string[] Areas = { "Attic", "ChildRoom", "Kitchen", "Workshop", "ClockHall" };
        static readonly string[] NamesRu = { "Чердак", "Детская", "Кухня", "Мастерская", "Часовой зал" };
        static readonly string[] NamesEn = { "The attic", "The nursery", "The kitchen", "The workshop", "The clock hall" };
        static readonly PuzzleKind[][] Puzzles = {
            new[]{PuzzleKind.Awakening,PuzzleKind.Lamp,PuzzleKind.Lamp,PuzzleKind.Lamp,PuzzleKind.Lamp},
            new[]{PuzzleKind.Car,PuzzleKind.LampCar,PuzzleKind.Car,PuzzleKind.Music,PuzzleKind.Music},
            new[]{PuzzleKind.Fan,PuzzleKind.FanCrate,PuzzleKind.Lamp,PuzzleKind.FanCrate,PuzzleKind.Music},
            new[]{PuzzleKind.Clock,PuzzleKind.Clock,PuzzleKind.Heavy,PuzzleKind.ClockHeavy,PuzzleKind.Music},
            new[]{PuzzleKind.Lamp,PuzzleKind.Fan,PuzzleKind.Clock,PuzzleKind.Heavy,PuzzleKind.Music,PuzzleKind.Finale}
        };
        static readonly string[][] StoriesRu = {
            new[]{"Что-то шевельнулось среди старых писем.","Свет помнит невидимые ступени.","В темноте вещи кажутся чужими.","На балках ещё лежит лунная пыль.","Внизу кто-то забыл закончить мелодию."},
            new[]{"Крошечные колёса всё ещё хотят в путь.","Фары здесь ни при чём. Нужен свет побольше.","Когда-то этот шкаф был целым городом.","Достаточно одной ноты, чтобы вспомнить мост.","Мелодия короткая. Но её можно повторить."},
            new[]{"Здесь пахло утром, даже по ночам.","Бумага легче воздуха. Почти как ты.","Над раковиной спряталась лунная тропинка.","Одна коробка — и старый клапан проснётся.","Тихая музыка пробирается даже сквозь пар."},
            new[]{"Эти часы помнят другой ритм.","Когда мир спешит, дай ему чуть больше времени.","Иногда дому нужна маленькая тяжесть.","Не торопи фигурку. Замедли всё остальное.","За стеной молчит самое большое сердце дома."},
            new[]{"Дом узнаёт твой свет.","Ветер поднимается по огромной лестнице.","У каждого колеса есть своя пауза.","Пусть маленькая фигурка удержит большой механизм.","Последний мост соткан из знакомой мелодии.","Остался один оборот. И наступит утро."}
        };
        static readonly string[][] StoriesEn = {
            new[]{"Something stirred among the old letters.","Light remembers invisible steps.","Things feel unfamiliar in the dark.","Moon dust still rests on the rafters.","Someone downstairs left a melody unfinished."},
            new[]{"The little wheels still long to travel.","Headlights will not do. Find a larger light.","This cupboard was once an entire city.","One note is enough to remember a bridge.","The melody is brief. You can always play it again."},
            new[]{"It smelled like morning here, even at night.","Paper is lighter than air. Almost like you.","A moonlit path is hiding above the sink.","One little box can wake an old valve.","A quiet melody finds its way through steam."},
            new[]{"These clocks remember another rhythm.","When the world hurries, give it a little time.","Sometimes a house needs a little weight.","Do not rush the figure. Slow everything else.","Beyond the wall, the house's largest heart is silent."},
            new[]{"The house recognises your light.","The breeze rises up the great stairway.","Every wheel has its own pause.","Let a small figure hold a great mechanism.","The last bridge is woven from a familiar melody.","One last turn. Then morning."}
        };
        [MenuItem("Tools/Shorok/Build Game Content")]
        public static void BuildGameContent() => Build(true);
        public static void BuildSlice() => Build(false);
        static void Build(bool full)
        {
            foreach (string d in new[]{"Data/Config","Data/Levels","Prefabs/Player","Prefabs/Possessables","Prefabs/World","Scenes","Audio/SFX","Audio/Music"}) Directory.CreateDirectory(Game + d);
            PixelArtGenerator.ProcessGhost(); PixelArtGenerator.Generate(); Setup(); GenerateAudio();
            config = Asset<GameConfig>(Game + "Data/Config/GameConfig.asset");
            config.movement = Asset<GhostMovementConfig>(Game + "Data/Config/GhostMovement.asset");
            config.font = AssetDatabase.LoadAssetAtPath<Font>(Game + "Art/UI/PressStart2P-Regular.ttf");
            config.ghost = AssetDatabase.LoadAssetAtPath<Sprite>(Game + "Art/Ghost/Generated/ghost_clean.png");
            config.blink = AssetDatabase.LoadAssetAtPath<Sprite>(Game + "Art/Ghost/Generated/ghost_blink.png");
            config.pixel = Sprite("pixel"); config.memory = Sprite("memory");
            config.effects = new AudioClip[8]; for (int i = 0; i < 8; i++) config.effects[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(Game + "Audio/SFX/fx" + i + ".wav");
            config.music = new AudioClip[6]; for (int i = 0; i < 6; i++) config.music[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(Game + "Audio/Music/room" + i + ".wav");
            config.levels = new LevelDefinition[5];
            for (int a = 0; a < 5; a++)
            {
                var level = Asset<LevelDefinition>(Game + "Data/Levels/" + Areas[a] + ".asset");
                level.index = a; level.sceneName = Areas[a]; level.nameRu = NamesRu[a]; level.nameEn = NamesEn[a];
                level.rooms = new RoomDefinition[Puzzles[a].Length];
                for (int r = 0; r < level.rooms.Length; r++) level.rooms[r] = new RoomDefinition { key = Areas[a] + "." + r, puzzle = Puzzles[a][r], memory = a == 4 ? r < 3 || r == 4 : r > 0, storyRu = StoriesRu[a][r], storyEn = StoriesEn[a][r] };
                config.levels[a] = level; EditorUtility.SetDirty(level);
            }
            NewScene();
            var ghost = CreateGhost(); config.ghostPrefab = PrefabUtility.SaveAsPrefabAsset(ghost.gameObject, Game + "Prefabs/Player/Ghost.prefab"); UnityEngine.Object.DestroyImmediate(ghost.gameObject);
            var platform = new GameObject("ShorokPlatform"); platform.AddComponent<YandexPlatformService>(); config.platformPrefab = PrefabUtility.SaveAsPrefabAsset(platform, Game + "Prefabs/World/YandexPlatform.prefab"); UnityEngine.Object.DestroyImmediate(platform);
            foreach (string kind in new[]{"lamp","fan","car","clock","heavy","music"}) { var p = Possess(kind, Vector2.zero, null); PrefabUtility.SaveAsPrefabAsset(p.gameObject, Game + "Prefabs/Possessables/" + kind + ".prefab"); UnityEngine.Object.DestroyImmediate(p.gameObject); }
            var bootstrap = new GameObject("Shorok").AddComponent<GameApp>(); bootstrap.Config = config; SaveScene("Bootstrap");
            CreateMenu(false);
            for (int a = 0; a < (full ? 5 : 1); a++) CreateLevel(a, full ? Puzzles[a].Length : 2);
            CreateMenu(true);
            var scenes = new List<EditorBuildSettingsScene> { SceneEntry("Bootstrap"), SceneEntry("MainMenu") };
            for (int a = 0; a < (full ? 5 : 1); a++) scenes.Add(SceneEntry(Areas[a])); scenes.Add(SceneEntry("Ending")); EditorBuildSettings.scenes = scenes.ToArray();
            EditorUtility.SetDirty(config); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Game + "Scenes/Bootstrap.unity");
            Directory.CreateDirectory("Reports"); File.WriteAllText("Reports/content-generation.txt", DateTime.UtcNow.ToString("O") + "\n" + (full ? "Five areas generated." : "Attic vertical slice generated."));
            Debug.Log("SHOROK_CONTENT_OK " + (full ? "FULL" : "SLICE"));
        }
        static T Asset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        static void Setup()
        {
            string[] backgroundNames = { "attic-v2", "childroom-v2", "kitchen-v2", "workshop-v2", "clockhall-v2" };
            for (int i = 0; i < backgroundNames.Length; i++)
            {
                string path = Game + "Art/Backgrounds/Source/" + backgroundNames[i] + ".png";
                if (!File.Exists(path)) throw new FileNotFoundException("Approved background missing", path);
                PixelArtGenerator.Import(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                importer.spritePixelsPerUnit = width / 26f; importer.maxTextureSize = 2048;
                importer.isReadable = false; importer.SaveAndReimport();
                backgrounds[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]); var layers = tags.FindProperty("layers");
            string[] names = {"Ground","OneWayGround","Possessable","Player","Hazard","Interactive","GhostPassage","WindAffected"};
            for (int i = 0; i < names.Length; i++) layers.GetArrayElementAtIndex(i + 6).stringValue = names[i]; tags.ApplyModifiedProperties();
            Physics2D.IgnoreLayerCollision(9, 9); Physics2D.IgnoreLayerCollision(10, 10);
            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Game + "Data/Config/Pixel.mat");
            if (spriteMaterial == null) { spriteMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")); AssetDatabase.CreateAsset(spriteMaterial, Game + "Data/Config/Pixel.mat"); }
            floorTile = Asset<Tile>(Game + "Data/Config/WoodTile.asset"); floorTile.sprite = Sprite("plank"); floorTile.colliderType = Tile.ColliderType.Grid; EditorUtility.SetDirty(floorTile);
            var yg = YG.InfoYG.Inst(); yg.Basic.autoGRA = false; yg.Basic.autoPauseGame = false; yg.Basic.autoApplySettings = false; yg.Basic.archivingBuild = false; yg.Basic.syncInitSDK = false; yg.Basic.logInEditor = false;
            yg.Templates.customProgressBar = false; yg.Templates.fixedAspectRatio = false; yg.Templates.fillBackground = false; yg.Templates.pixelRatioEnable = false; EditorUtility.SetDirty(yg);
            PlayerSettings.companyName = "Shorok"; PlayerSettings.productName = "shorok"; PlayerSettings.runInBackground = true;
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.WebGL.template = "PROJECT:Shorok"; PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false; PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
        }
        static Sprite Sprite(string name)
        {
            if (name.StartsWith("background") && int.TryParse(name.Substring(10), out int index) && backgrounds[index] != null) return backgrounds[index];
            return AssetDatabase.LoadAssetAtPath<Sprite>(PixelArtGenerator.Root + name + ".png");
        }
        static GameObject Obj(string name, Vector2 position, Transform parent = null)
        {
            var go = new GameObject(name); go.transform.position = position; if (parent != null) go.transform.SetParent(parent, true); return go;
        }
        static SpriteRenderer Art(string sprite, Vector2 position, Transform parent, int order = 0, Vector2? size = null)
        {
            var go = Obj(sprite, position, parent); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = Sprite(sprite); renderer.sharedMaterial = spriteMaterial; renderer.sortingOrder = order;
            if (size.HasValue) { var bounds = renderer.sprite.bounds.size; renderer.transform.localScale = new Vector3(size.Value.x / bounds.x, size.Value.y / bounds.y, 1); }
            return renderer;
        }
        static BoxCollider2D Box(GameObject go, Vector2 size, int layer = 6, bool trigger = false)
        {
            go.layer = layer; var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(size.x / go.transform.localScale.x, size.y / go.transform.localScale.y); c.isTrigger = trigger; return c;
        }
        static SpriteRenderer Shelf(Vector2 p, float width, Transform parent, bool oneWay = false)
        {
            var art = Art("shelf", p, parent, 2, new Vector2(width, .375f)); var c = Box(art.gameObject, new Vector2(width, .32f), oneWay ? 7 : 6);
            if (oneWay) { c.usedByEffector = true; var e = art.gameObject.AddComponent<PlatformEffector2D>(); e.useOneWay = true; e.surfaceArc = 170; }
            return art;
        }
        static GhostMotor CreateGhost()
        {
            var root = Obj("Ghost", new Vector2(2, 1.25f)); root.layer = 9;
            var body = root.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.constraints = RigidbodyConstraints2D.FreezeRotation; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var col = root.AddComponent<CapsuleCollider2D>(); col.size = new Vector2(.7f, 1); col.direction = CapsuleDirection2D.Vertical;
            var noFriction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Game + "Data/Config/Ghost.physicsMaterial2D");
            if (noFriction == null) { noFriction = new PhysicsMaterial2D("Ghost") { friction = 0, bounciness = 0 }; AssetDatabase.CreateAsset(noFriction, Game + "Data/Config/Ghost.physicsMaterial2D"); } col.sharedMaterial = noFriction;
            var motor = root.AddComponent<GhostMotor>(); motor.config = config.movement; root.AddComponent<PossessionController>();
            var visual = Obj("Body", root.transform.position, root.transform).AddComponent<SpriteRenderer>(); visual.sprite = config.ghost; visual.sharedMaterial = spriteMaterial; visual.sortingOrder = 20;
            var anim = root.AddComponent<GhostAnimator>(); anim.body = visual; anim.normal = config.ghost; anim.blink = config.blink;
            return motor;
        }
        static Possessable Possess(string type, Vector2 p, Transform parent)
        {
            var art = Art(type, p, parent, 8); var obj = art.gameObject;
            Possessable possessable;
            switch (type)
            {
                case "lamp": possessable = obj.AddComponent<LampPossessable>(); ((LampPossessable)possessable).beam = Art("beam", p + new Vector2(1.1f, -.6f), obj.transform, -1, new Vector2(4, 5)); ((LampPossessable)possessable).beam.enabled = false; break;
                case "fan": possessable = obj.AddComponent<FanPossessable>(); possessable.detail = Art("blades", p + new Vector2(0, .15f), obj.transform, 9).transform; break;
                case "car": possessable = obj.AddComponent<ToyCarPossessable>(); break;
                case "clock": possessable = obj.AddComponent<ClockPossessable>(); possessable.detail = Art("hand", p + new Vector2(0, .375f), obj.transform, 9).transform; break;
                case "heavy": possessable = obj.AddComponent<HeavyPossessable>(); break;
                default: possessable = obj.AddComponent<MusicBoxPossessable>(); possessable.detail = Art("note", p + Vector2.up * .8f, obj.transform, 9).transform; break;
            }
            possessable.visual = art; possessable.key = type;
            Vector2 size = type == "car" ? new Vector2(1.35f, .55f) : type == "heavy" ? new Vector2(.85f, 1.15f) : new Vector2(.6f, .8f);
            Box(obj, size, 8);
            if (type == "car" || type == "heavy") { var body = obj.AddComponent<Rigidbody2D>(); body.mass = type == "heavy" ? 12 : 2; body.gravityScale = 3; body.constraints = RigidbodyConstraints2D.FreezeRotation; body.interpolation = RigidbodyInterpolation2D.Interpolate; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; }
            return possessable;
        }
        static void NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        static void SaveScene(string name) => EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Game + "Scenes/" + name + ".unity");
        static EditorBuildSettingsScene SceneEntry(string name) => new EditorBuildSettingsScene(Game + "Scenes/" + name + ".unity", true);
        static Camera Camera()
        {
            var camera = Obj("Main Camera", new Vector2(7.5f, 4)).AddComponent<Camera>(); camera.tag = "MainCamera"; camera.transform.position += Vector3.back * 10;
            camera.orthographic = true; camera.orthographicSize = 5.625f; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.07f, .07f, .07f);
            camera.gameObject.AddComponent<AudioListener>(); camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            var pixel = camera.gameObject.AddComponent<PixelPerfectCamera>(); pixel.assetsPPU = 32; pixel.refResolutionX = 640; pixel.refResolutionY = 360; pixel.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            return camera;
        }
        static void CreateMenu(bool ending)
        {
            NewScene(); var camera = Camera(); camera.transform.position = new Vector3(13, 4.9f, -10);
            var backdrop = Art("background" + (ending ? 4 : 0), new Vector2(13, 4.9f), null, -20);
            var ghost = Art("pixel", new Vector2(17, 1.25f), null, 20); ghost.sprite = config.ghost; ghost.gameObject.AddComponent<AmbientMotion>().sway = 2;
            if (ending) { var hand = Art("hand", new Vector2(19.95f, 8.83f), null, 10, new Vector2(1.4f, 1.4f)); var dawn = backdrop.gameObject.AddComponent<EndingDawn>(); dawn.background = backdrop; dawn.hand = hand.transform; }
            SaveScene(ending ? "Ending" : "MainMenu");
        }
        static void CreateLevel(int area, int roomCount)
        {
            NewScene(); var root = Obj(Areas[area], Vector2.zero).AddComponent<LevelRoot>(); root.definition = config.levels[area]; root.rooms = new RoomController[roomCount];
            root.Ghost = ((GameObject)PrefabUtility.InstantiatePrefab(config.ghostPrefab)).GetComponent<GhostMotor>();
            root.cameraRig = Camera().gameObject.AddComponent<PixelCamera>(); root.Particles = Obj("Pixel particles", Vector2.zero, root.transform).AddComponent<PixelParticles>();
            for (int x = 0; x < roomCount * 26; x += 26)
            {
                var background = Art("background" + area, new Vector2(x + 13, 4.9f), root.transform, -30);
                var ambient = background.gameObject.AddComponent<AmbientMotion>(); ambient.sway = 0; ambient.parallax = .012f;
            }
            var grid = Obj("Grid", Vector2.zero, root.transform).AddComponent<Grid>(); grid.cellSize = new Vector3(1, .5f, 1);
            var mapObj = Obj("Wooden architecture", Vector2.zero, grid.transform); var map = mapObj.AddComponent<Tilemap>(); mapObj.layer = 6;
            var render = mapObj.AddComponent<TilemapRenderer>(); render.sharedMaterial = spriteMaterial;
            var rb = mapObj.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Static;
            var tiles = mapObj.AddComponent<TilemapCollider2D>(); tiles.compositeOperation = Collider2D.CompositeOperation.Merge;
            var composite = mapObj.AddComponent<CompositeCollider2D>(); composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            for (int r = 0; r < roomCount; r++) CreateRoom(root, map, area, r);
            map.RefreshAllTiles(); tiles.ProcessTilemapChanges(); composite.GenerateGeometry(); Physics2D.SyncTransforms();
            // Boundaries keep objects inside authored space and make retry predictable.
            var left = Obj("Left wall", new Vector2(-.5f, 3), root.transform); Box(left, new Vector2(1, 10));
            SaveScene(Areas[area]);
        }
        static void Ground(Tilemap map, int start, int end, int height = 0) { for (int x = start; x <= end; x++) for (int y = -5; y <= height; y++) map.SetTile(new Vector3Int(x, y, 0), floorTile); }
        static RevealablePlatform Reveal(Vector2 p, float width, RoomController room, Possessable source)
        {
            var art = Shelf(p, width, room.transform); var reveal = art.gameObject.AddComponent<RevealablePlatform>(); reveal.source = source; reveal.room = room; reveal.visual = art; reveal.solid = art.GetComponent<Collider2D>(); reveal.solid.enabled = false; return reveal;
        }
        static PressurePlate Plate(Vector2 p, RoomController room, bool heavy = false)
        {
            var art = Art("plate", p, room.transform, 5); Box(art.gameObject, new Vector2(1, .3f), 11, true); var plate = art.gameObject.AddComponent<PressurePlate>(); plate.visual = art; plate.requireHeavy = heavy; return plate;
        }
        static PuzzleGate Gate(Vector2 p, RoomController room, PressurePlate plate)
        {
            var art = Art("gate", p, room.transform, 5, new Vector2(.8f, 4)); Box(art.gameObject, new Vector2(.8f, 4)); var gate = art.gameObject.AddComponent<PuzzleGate>(); gate.plate = plate; gate.room = room; return gate;
        }
        static WorldHazard Hazard(string sprite, Vector2 p, Vector2 size, RoomController room, HazardKind kind)
        {
            var art = Art(sprite, p, room.transform, 6, size); Box(art.gameObject, size * .78f, 10, true); var hazard = art.gameObject.AddComponent<WorldHazard>(); hazard.kind = kind; return hazard;
        }
        static void CreateRoom(LevelRoot root, Tilemap map, int area, int r)
        {
            int x = r * 26; var def = config.levels[area].rooms[r];
            var room = Obj(def.key + " " + def.puzzle, new Vector2(x, 0), root.transform).AddComponent<RoomController>();
            root.rooms[r] = room; room.index = r; room.Spawn = new Vector2(x + 2, 1.2f); room.Right = x + 26; room.ExitHeight = .5f;
            var parent = room.transform; Possessable memorySource = null; Vector2 memoryAt = new Vector2(x + 15, 5.4f);
            Ground(map, x, x + 6); Ground(map, x + 21, x + 25);
            Art("vent", new Vector2(x + 24.5f, .7f), parent, 1);
            var marker = Art("memory", new Vector2(x + 1, 1), parent, 4); marker.transform.localScale = Vector3.one * .4f;
            if (def.puzzle == PuzzleKind.Awakening)
            {
                Ground(map, x + 7, x + 25); Shelf(new Vector2(x + 8, 1), 3, parent); Shelf(new Vector2(x + 12, 1.65f), 2.5f, parent, true); Shelf(new Vector2(x + 16, 2.65f), 2.5f, parent, true);
                Art("book", new Vector2(x + 19, .8f), parent, 2, new Vector2(3, .6f));
            }
            else if (def.puzzle == PuzzleKind.Lamp || def.puzzle == PuzzleKind.Music)
            {
                bool music = def.puzzle == PuzzleKind.Music; var source = Possess(music ? "music" : "lamp", new Vector2(x + 4.5f, music ? 1 : 1.5f), parent); memorySource = source;
                Reveal(new Vector2(x + 8, 1.3f), 2.3f, room, source); Reveal(new Vector2(x + 11, 2.35f), 2.3f, room, source);
                Reveal(new Vector2(x + 14, 3.3f), 2.6f, room, source); Reveal(new Vector2(x + 17, 2.3f), 2.4f, room, source); Reveal(new Vector2(x + 20, 1.25f), 2.4f, room, source);
                Shelf(new Vector2(x + 16, 4.5f), 1.7f, parent, true); memoryAt = new Vector2(x + 16, 5.35f);
                if (r >= 2 || area > 0) Hazard("shadow", new Vector2(x + 12, .4f), new Vector2(3, 2), room, HazardKind.Shadow).calmSource = source;
                if (r == 3 && area == 0) { var moving = Shelf(new Vector2(x + 18, 3.6f), 1.7f, parent).gameObject.AddComponent<MovingPlatform>(); moving.travel = new Vector2(1.4f, .5f); }
            }
            else if (def.puzzle == PuzzleKind.Car || def.puzzle == PuzzleKind.LampCar || def.puzzle == PuzzleKind.Finale)
            {
                Ground(map, x + 7, x + 20);
                var car = Possess("car", new Vector2(x + 5, .85f), parent);
                var roof = Art("cupboard", new Vector2(x + 11.5f, 3.8f), parent, 3, new Vector2(6, 4.9f)); Box(roof.gameObject, new Vector2(6, 4.9f));
                var plate = Plate(new Vector2(x + 16.5f, .63f), room); Gate(new Vector2(x + 18.3f, 2.5f), room, plate);
                if (def.puzzle == PuzzleKind.LampCar)
                {
                    var lamp = Possess("lamp", new Vector2(x + 3, 1.5f), parent);
                    var blocker = Gate(new Vector2(x + 8, 2.5f), room, null); blocker.source = lamp;
                    memorySource = lamp;
                }
                Shelf(new Vector2(x + 20, 1.8f), 2, parent, true); Shelf(new Vector2(x + 22, 3), 2, parent, true); Shelf(new Vector2(x + 24, 4.2f), 2, parent, true); memoryAt = new Vector2(x + 24, 5);
                if (def.puzzle == PuzzleKind.Finale)
                {
                    Art("grandclock", new Vector2(x + 22.8f, 4.8f), parent, -5, new Vector2(6, 10));
                    Shelf(new Vector2(x + 25.3f, 5.3f), 2, parent, true); room.ExitHeight = 5.3f;
                }
            }
            else if (def.puzzle == PuzzleKind.Fan || def.puzzle == PuzzleKind.FanCrate)
            {
                var fan = (FanPossessable)Possess("fan", new Vector2(x + 5, 1.25f), parent);
                fan.windOffset = new Vector2(2, 3); fan.windSize = new Vector2(3.4f, 8);
                Shelf(new Vector2(x + 9, 3), 2.2f, parent, true); Shelf(new Vector2(x + 11.7f, 4.2f), 2.5f, parent, true);
                Ground(map, x + 14, x + 25, 8); room.ExitHeight = 4.5f;
                Shelf(new Vector2(x + 18, 5.8f), 2, parent, true); memoryAt = new Vector2(x + 18, 6.6f);
                if (def.puzzle == PuzzleKind.FanCrate)
                {
                    Ground(map, x + 7, x + 13);
                    var crate = Art("crate", new Vector2(x + 7.2f, 1.03f), parent, 4);
                    Box(crate.gameObject, new Vector2(.95f, .95f), 13).sharedMaterial = config.ghostPrefab.GetComponent<CapsuleCollider2D>().sharedMaterial;
                    var body = crate.gameObject.AddComponent<Rigidbody2D>(); body.mass = .3f; body.gravityScale = 3; body.constraints = RigidbodyConstraints2D.FreezeRotation; fan.lightCrate = body;
                    var plate = Plate(new Vector2(x + 11, .63f), room); Gate(new Vector2(x + 13, 4.5f), room, plate);
                }
                if (area == 2 && r > 0) Hazard("steam", new Vector2(x + 20, 5.7f), new Vector2(1, 2.2f), room, HazardKind.Steam);
            }
            else if (def.puzzle == PuzzleKind.Clock)
            {
                var clock = (ClockPossessable)Possess("clock", new Vector2(x + 4.7f, 1.4f), parent);
                var first = Shelf(new Vector2(x + 8, 1.35f), 2.5f, parent).gameObject.AddComponent<MovingPlatform>(); first.travel = new Vector2(2.8f, .6f); first.clock = clock;
                Shelf(new Vector2(x + 13, 2.5f), 2.5f, parent);
                var second = Shelf(new Vector2(x + 16, 2), 2.5f, parent).gameObject.AddComponent<MovingPlatform>(); second.travel = new Vector2(3, 0); second.clock = clock;
                Shelf(new Vector2(x + 14.5f, 3.8f), 2, parent, true); memoryAt = new Vector2(x + 14.5f, 4.7f);
                var hazard = Hazard("gear", new Vector2(x + 17.5f, 3.4f), new Vector2(1.3f, 1.3f), room, HazardKind.Gear); hazard.clock = clock; hazard.interval = 2.6f;
            }
            else if (def.puzzle == PuzzleKind.Heavy || def.puzzle == PuzzleKind.ClockHeavy)
            {
                Ground(map, x + 7, x + 20);
                var heavy = (HeavyPossessable)Possess("heavy", new Vector2(x + 5, 1.1f), parent);
                var plate = Plate(new Vector2(x + 14, .63f), room, true); Gate(new Vector2(x + 17, 2.5f), room, plate);
                if (def.puzzle == PuzzleKind.ClockHeavy)
                {
                    var clock = (ClockPossessable)Possess("clock", new Vector2(x + 3, 1.4f), parent);
                    var door = Art("gate", new Vector2(x + 10, 2.5f), parent, 4, new Vector2(.7f, 4)); Box(door.gameObject, new Vector2(.7f, 4));
                    var moving = door.gameObject.AddComponent<MovingPlatform>(); moving.travel = new Vector2(0, 4.7f); moving.speed = 1.5f; moving.clock = clock;
                }
                Shelf(new Vector2(x + 12, 2.7f), 2, parent, true); Shelf(new Vector2(x + 14.3f, 4), 2.1f, parent, true); memoryAt = new Vector2(x + 14.3f, 4.8f);
                var brittle = Art("shelf", new Vector2(x + 21, 1.5f), parent, 2, new Vector2(2, .4f)); Box(brittle.gameObject, new Vector2(2, .4f)); var gate = brittle.gameObject.AddComponent<PuzzleGate>(); gate.brittle = true; gate.heavy = heavy; gate.room = room;
            }
            if (def.memory)
            {
                var shard = Art("memory", memoryAt, parent, 16); Box(shard.gameObject, new Vector2(.55f, .7f), 11, true);
                var collect = shard.gameObject.AddComponent<MemoryShard>(); collect.id = Areas[area] + ".memory." + r; collect.revealSource = memorySource;
            }
            // Readable foreground props, without decorative collision.
            for (int i = 0; i < 3; i++) Art(area == 2 ? "vent" : "book", new Vector2(x + 1.5f + i * 1.1f, -.2f - i * .12f), parent, 1);
        }
        static void GenerateAudio()
        {
            for (int i = 0; i < 8; i++) Wave(Game + "Audio/SFX/fx" + i + ".wav", .28f + i * .035f, i, false);
            for (int i = 0; i < 6; i++) Wave(Game + "Audio/Music/room" + i + ".wav", 24, i, true);
        }
        static void Wave(string path, float seconds, int kind, bool music)
        {
            if (!File.Exists(path))
            {
                const int rate = 22050; int count = (int)(seconds * rate);
                using var writer = new BinaryWriter(File.Create(path)); writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16); writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                int[] notes = { 0, 7, 10, 14, 7, 3, 12, 10, 0, 7, 15, 14, 10, 7, 3, 7 };
                for (int n = 0; n < count; n++)
                {
                    double t = n / (double)rate, value;
                    if (music)
                    {
                        double noteTime = t % 1.5, frequency = 130.8128 * Math.Pow(2, (notes[(int)(t / 1.5) % notes.Length] + kind % 3 * 2) / 12.0);
                        double envelope = Math.Min(1, noteTime * 30) * Math.Exp(-noteTime * 2.5);
                        value = (Math.Sin(t * frequency * 2 * Math.PI) + .25 * Math.Sin(t * frequency * 4 * Math.PI)) * envelope * .14;
                        value += Math.Sin(t * 65.4064 * 2 * Math.PI) * .018 * Math.Pow(Math.Sin(t / 24 * Math.PI), 2);
                        value *= Math.Min(1, (seconds - t) * 3);
                    }
                    else
                    {
                        double frequency = (180 + kind * 87) * (kind == 0 ? 1 + t * 2 : 1 - t * .7);
                        value = Math.Sin(t * frequency * 2 * Math.PI) * Math.Sin(Math.PI * t / seconds) * Math.Exp(-t * 6) * .28;
                    }
                    writer.Write((short)(Math.Clamp(value, -1, 1) * short.MaxValue));
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path); var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = .35f; settings.loadType = music ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings; importer.forceToMono = true; importer.SaveAndReimport();
        }
    }
}
