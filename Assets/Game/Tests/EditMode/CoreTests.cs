using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Shorok;

public sealed class CoreTests
{
    [Test] public void SaveRoundTripRetainsProgressAndSettings()
    {
        var save = new SaveData { area = 3, checkpoint = 2, saveRevision = 7, started = true }; save.settings.language = "ru"; save.memories.Add("Attic.memory.1");
        var restored = SaveSystem.Parse(JsonUtility.ToJson(save)); Assert.AreEqual(3, restored.area); Assert.AreEqual(7, restored.saveRevision); Assert.AreEqual("ru", restored.settings.language); CollectionAssert.AreEqual(save.memories, restored.memories);
    }
    [Test] public void MigrationAndCorruptionAreSafe()
    {
        Assert.AreEqual(1, SaveSystem.Parse("{\"schemaVersion\":0,\"area\":2}").schemaVersion);
        Assert.IsNull(SaveSystem.Parse("{broken")); Assert.IsNull(SaveSystem.Parse("{\"schemaVersion\":99}")); Assert.IsNull(SaveSystem.Parse("[1]"));
        Assert.AreEqual(4, SaveSystem.Parse("{\"schemaVersion\":1,\"area\":99}").area);
    }
    [Test] public void RevisionWinsOverOldCloudProgress()
    {
        var local = new SaveData { saveRevision = 9, area = 0 }; var cloud = new SaveData { saveRevision = 8, area = 4, completed = true };
        Assert.AreSame(local, SaveSystem.Resolve(local, cloud)); cloud.saveRevision = 10; Assert.AreSame(cloud, SaveSystem.Resolve(local, cloud));
    }
    [Test] public void EqualRevisionUsesTimestampThenProgress()
    {
        var a = new SaveData { lastSaveTimestamp = 12 }; var b = new SaveData { lastSaveTimestamp = 13 };
        Assert.AreSame(b, SaveSystem.Resolve(a, b)); a.lastSaveTimestamp = 13; a.completed = true; Assert.AreSame(a, SaveSystem.Resolve(a, b));
    }
    [Test] public void LocalizationUsesEnglishFallback()
    {
        var text = new Localization { Language = "tr" }; Assert.AreEqual("Continue", text.Get("menu.continue")); text.Language = "ru"; Assert.AreEqual("Продолжить", text.Get("menu.continue"));
        foreach (PuzzleKind kind in System.Enum.GetValues(typeof(PuzzleKind))) Assert.IsTrue(Localization.Contains("hint." + kind.ToString().ToLowerInvariant()));
    }
    [Test] public void PauseReasonsDoNotResumeManualPause()
    {
        var state = new GameState(); state.SetPhase(GamePhase.Playing); Assert.IsTrue(state.Playing);
        state.SetPause(PauseReason.Manual, true); state.SetPause(PauseReason.Platform | PauseReason.Advertisement, true);
        state.SetPause(PauseReason.Platform, false); state.SetPause(PauseReason.Advertisement, false); Assert.IsFalse(state.Playing);
        state.SetPause(PauseReason.Manual, false); Assert.IsTrue(state.Playing); state.SetPhase(GamePhase.Menu); Assert.IsFalse(state.Playing);
    }
    [Test] public void AdPolicyWaitsForFirstDelayAndCooldown()
    {
        var policy = new AdPolicy(180, 180); Assert.IsFalse(policy.CanShow(179)); Assert.IsTrue(policy.CanShow(180)); policy.Record(180); Assert.IsFalse(policy.CanShow(359)); Assert.IsTrue(policy.CanShow(360));
    }
    [Test] public void DefinitionsContainFiveAreasAndTwentyUniqueMemories()
    {
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/Game/Data/Config/GameConfig.asset"); Assert.NotNull(config); Assert.AreEqual(5, config.levels.Length);
        var keys = new HashSet<string>(); int count = 0;
        foreach (var level in config.levels) { Assert.GreaterOrEqual(level.rooms.Length, 5); foreach (var room in level.rooms) { Assert.IsTrue(keys.Add(room.key)); if (room.memory) count++; } }
        Assert.AreEqual(20, count); Assert.NotNull(config.font); Assert.IsTrue(config.font.HasCharacter('Я')); Assert.IsTrue(config.font.HasCharacter('A'));
    }
    [Test] public void GhostHasTransparentBorderOpaqueBodyAndBlackEyes()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/Art/Ghost/Generated/ghost_clean.png"); Assert.NotNull(tex);
        int black = 0, white = 0; var pixels = tex.GetPixels32();
        for (int y = 0; y < tex.height; y++) for (int x = 0; x < tex.width; x++)
        {
            var c = pixels[y * tex.width + x]; if (x == 0 || y == 0 || x == tex.width - 1 || y == tex.height - 1) Assert.AreEqual(0, c.a);
            if (c.a > 0 && c.r < 30) black++; if (c.a == 255 && c.r > 240) white++;
        }
        Assert.Greater(black, 8); Assert.Greater(white, 150);
    }
    [Test] public void MobileButtonsPreserveSimultaneousMovementAndJump()
    {
        using var input = new InputRouter(); input.TouchPress(1); input.TouchPress(2); input.Sample(); Assert.AreEqual(1, input.Move); Assert.IsTrue(input.JumpHeld); Assert.IsTrue(input.JumpPressed);
        input.TouchRelease(2); Assert.AreEqual(1, input.Move); input.ClearTouch(); Assert.AreEqual(0, input.Move); Assert.IsFalse(input.JumpHeld);
    }
}
