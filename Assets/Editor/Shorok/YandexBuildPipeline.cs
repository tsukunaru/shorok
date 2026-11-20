using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shorok.EditorTools
{
    public static class YandexBuildPipeline
    {
        public static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../../Builds/Yandex/shorok"));
        public static string Archive => Path.GetFullPath(Path.Combine(Application.dataPath, "../../Release/yandex/shorok-yandex.zip"));
        [MenuItem("Tools/Shorok/Validate Project")]
        public static void Validate()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Game/Data/Config/GameConfig.asset"); Require(config != null, "Game config missing");
            Require(config.levels.Length == 5, "Five areas required"); Require(config.font != null && config.font.HasCharacter('Я'), "Cyrillic font missing");
            var memories = new HashSet<string>(); var checkpoints = new HashSet<string>(); int rooms = 0;
            var previous = EditorSceneManager.GetActiveScene().path;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                Require(File.Exists(scene.path), "Missing scene: " + scene.path); var opened = EditorSceneManager.OpenScene(scene.path);
                foreach (var root in opened.GetRootGameObjects())
                {
                    foreach (var t in root.GetComponentsInChildren<Transform>(true)) Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0, "Missing script: " + t.name);
                    foreach (var sprite in root.GetComponentsInChildren<SpriteRenderer>(true)) Require(sprite.sprite != null, "Missing sprite: " + sprite.name);
                    foreach (var shard in root.GetComponentsInChildren<MemoryShard>(true)) Require(memories.Add(shard.id), "Duplicate memory: " + shard.id);
                    foreach (var room in root.GetComponentsInChildren<RoomController>(true)) { rooms++; Require(checkpoints.Add(opened.name + "." + room.index), "Duplicate checkpoint"); }
                    var level = root.GetComponent<LevelRoot>();
                    if (level != null) Require(level.definition != null && level.Ghost != null && level.cameraRig != null && level.rooms.Length == level.definition.rooms.Length, "Incomplete area: " + opened.name);
                }
            }
            Require(memories.Count == 20, "Expected 20 scene memories, found " + memories.Count);
            foreach (var path in Directory.GetFiles("Assets/Game/Art/Generated", "*.png"))
            {
                var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
                Require(importer != null && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled, "Invalid pixel import: " + path);
            }
            foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind))) Require(Localization.Contains("hint." + kind.ToString().ToLowerInvariant()), "Missing hint: " + kind);
            if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
            Directory.CreateDirectory("Reports"); File.WriteAllText("Reports/content-validation.txt", $"PASS\nScenes: {EditorBuildSettings.scenes.Length}\nRooms: {rooms}\nUnique memories: {memories.Count}\nUnique checkpoints: {checkpoints.Count}\nMissing scripts/sprites: 0\nPixel texture settings: PASS\nRU/EN font and hints: PASS\n");
            Debug.Log("SHOROK_VALIDATION_OK");
        }
        [MenuItem("Tools/Shorok/Build/Yandex Release")]
        public static void BuildRelease()
        {
            Validate(); Directory.CreateDirectory(Output);
            PlayerSettings.WebGL.template = "PROJECT:Shorok";
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(), locationPathName = Output, target = BuildTarget.WebGL, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("Web build failed: " + report.summary.result);
            ValidateOutput();
        }
        [MenuItem("Tools/Shorok/Validate Yandex Build")]
        public static void ValidateOutput()
        {
            Require(File.Exists(Path.Combine(Output, "index.html")), "index.html must be at build root");
            var files = new DirectoryInfo(Output).GetFiles("*", SearchOption.AllDirectories);
            long size = files.Sum(f => f.Length); Require(size < 99_000_000, "Build exceeds 99 MB hard limit");
            foreach (var file in files) Require(Path.GetRelativePath(Output, file.FullName).All(c => c < 128 && c != ' '), "Non-ASCII or spaced release filename: " + file.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(Archive)); string pending = Archive + ".next"; if (File.Exists(pending)) File.Delete(pending);
            ZipFile.CreateFromDirectory(Output, pending, System.IO.Compression.CompressionLevel.Optimal, false);
            using (var zip = ZipFile.OpenRead(pending)) Require(zip.GetEntry("index.html") != null, "ZIP root index missing");
            string verify = Path.GetFullPath(Path.Combine("Reports", "zip-check-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")));
            Directory.CreateDirectory(verify); ZipFile.ExtractToDirectory(pending, verify);
            Require(File.Exists(Path.Combine(verify, "index.html")), "Extracted ZIP index missing");
            Require(new DirectoryInfo(verify).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length) == size, "Extracted byte size differs");
            File.Copy(pending, Archive, true); File.Delete(pending);
            var text = new StringBuilder(); text.AppendLine("Production Web build — Unity " + Application.unityVersion); text.AppendLine("Development flag: OFF"); text.AppendLine("Web: " + Output); text.AppendLine("ZIP: " + Archive);
            text.AppendLine($"Uncompressed: {size:N0} bytes ({size / 1_000_000.0:F2} MB)"); text.AppendLine($"ZIP: {new FileInfo(Archive).Length / 1_000_000.0:F2} MB"); text.AppendLine("Files: " + files.Length); text.AppendLine("ZIP extraction verification: PASS"); text.AppendLine("Largest files:");
            foreach (var file in files.OrderByDescending(f => f.Length).Take(12)) text.AppendLine($"{file.Length / 1_000_000.0:F2} MB  {Path.GetRelativePath(Output, file.FullName)}");
            Directory.CreateDirectory("Reports"); File.WriteAllText("Reports/BUILD_REPORT.txt", text.ToString()); Debug.Log(text.ToString());
            if (size > 70_000_000) Debug.LogWarning("Shorok exceeds the 70 MB optimization target.");
        }
        static void Require(bool condition, string message) { if (!condition) throw new BuildFailedException(message); }
        [MenuItem("Tools/Shorok/Clear Local Save")] public static void ClearLocalSave() => SaveSystem.Clear();
        [MenuItem("Tools/Shorok/Open Save Folder")] public static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
