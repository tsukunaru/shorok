using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Shorok;

public sealed class GameplayTests
{
    GameApp app;
    string previous, backup;
    [UnitySetUp] public IEnumerator Boot()
    {
        previous = PlayerPrefs.GetString(SaveSystem.Key, ""); backup = PlayerPrefs.GetString(SaveSystem.Key + ".backup", "");
        SaveSystem.Clear(); yield return SceneManager.LoadSceneAsync("Bootstrap");
        for (int i = 0; i < 600 && (GameApp.Instance == null || GameApp.Instance.State.Phase != GamePhase.Menu); i++) yield return null;
        app = GameApp.Instance; Assert.NotNull(app); Assert.AreEqual(GamePhase.Menu, app.State.Phase);
        app.State.SetPause(PauseReason.Focus, false); app.NewGame();
        for (int i = 0; i < 600 && app.Level == null; i++) yield return null;
        Assert.NotNull(app.Level); app.State.SetPause(PauseReason.Focus, false); yield return new WaitForFixedUpdate();
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        if (app != null) app.State.SetPhase(GamePhase.Menu);
        var cleanup = SceneManager.CreateScene("TestCleanup");
        var oldScene = SceneManager.GetActiveScene(); SceneManager.SetActiveScene(cleanup);
        if (oldScene.IsValid()) yield return SceneManager.UnloadSceneAsync(oldScene);
        if (app != null) Object.Destroy(app.gameObject);
        var yg = GameObject.Find("YG2Instance"); if (yg != null) Object.Destroy(yg);
        yield return null;
        PlayerPrefs.SetString(SaveSystem.Key, previous); PlayerPrefs.SetString(SaveSystem.Key + ".backup", backup); PlayerPrefs.Save();
    }
    [UnityTest] public IEnumerator MenuLoadsGhostAndJumpHasHeight()
    {
        var ghost = app.Level.Ghost; yield return new WaitForSeconds(.4f);
        var camera = app.Level.cameraRig.GetComponent<Camera>();
        Assert.GreaterOrEqual(camera.transform.position.x - camera.orthographicSize * camera.aspect, 0, "Camera must stay inside background at the left edge");
        var ray = Physics2D.Raycast(ghost.transform.position, Vector2.down, 4, 1 << 6);
        Assert.IsTrue(ghost.Grounded, "position=" + ghost.transform.position + " velocity=" + ghost.Body.linearVelocity + " floor=" + (ray.collider == null ? "none" : ray.point.ToString()));
        float y = ghost.transform.position.y; app.Input.TouchPress(2); yield return new WaitForSeconds(.2f); Assert.Greater(ghost.transform.position.y, y + .7f);
        app.Input.TouchRelease(2); yield return new WaitForSeconds(.8f); Assert.Less(ghost.transform.position.y, y + .3f);
    }
    [UnityTest] public IEnumerator LampPossessionRevealsBridgeAndExitsSafely()
    {
        yield return app.LoadArea(0, 1);
        var lamp = app.Level.Room.GetComponentInChildren<LampPossessable>(); var ghost = app.Level.Ghost;
        ghost.Teleport((Vector2)lamp.transform.position + Vector2.left); yield return null;
        yield return ghost.Possession.Enter(lamp); Assert.AreSame(lamp, ghost.Possession.Current); Assert.IsFalse(ghost.Body.simulated);
        lamp.SetActive(true); yield return new WaitForSeconds(.3f);
        var bridge = app.Level.Room.GetComponentInChildren<RevealablePlatform>(); Assert.IsTrue(bridge.solid.enabled);
        Assert.IsTrue(ghost.Possession.TryExit()); Assert.IsTrue(ghost.Body.simulated); Assert.IsNull(ghost.Possession.Current);
    }
    [UnityTest] public IEnumerator CheckpointSurvivesRespawnAndRoomReset()
    {
        app.Level.Ghost.Teleport(new Vector2(25.4f, 1.2f)); yield return null; yield return null;
        Assert.AreEqual(1, app.Level.CurrentRoom); Assert.AreEqual(1, SaveSystem.Load().checkpoint);
        var lamp = app.Level.Room.GetComponentInChildren<LampPossessable>(); lamp.SetActive(true);
        app.Respawn(); yield return new WaitForSecondsRealtime(.7f);
        Assert.Less(Vector2.Distance(app.Level.Ghost.transform.position, app.Level.Room.Spawn), .5f);
        app.ResetRoom(); Assert.IsFalse(lamp.Active);
    }
    [UnityTest] public IEnumerator ManualPauseSurvivesPlatformResume()
    {
        app.TogglePause(); app.State.SetPause(PauseReason.Platform, true); app.State.SetPause(PauseReason.Platform, false);
        Assert.IsFalse(app.State.Playing); Assert.AreEqual(0, Time.timeScale); yield return null;
        app.TogglePause(); Assert.IsTrue(app.State.Playing); Assert.AreEqual(1, Time.timeScale);
    }
    [UnityTest] public IEnumerator SafeExitRejectsSolidAndHazardOverlap()
    {
        var lamp = Object.FindFirstObjectByType<LampPossessable>(); var blocker = new GameObject("Exit blocked"); blocker.layer = 6; blocker.transform.position = lamp.transform.position;
        blocker.AddComponent<BoxCollider2D>().size = new Vector2(10, 10); Physics2D.SyncTransforms();
        Assert.IsFalse(app.Level.Ghost.Possession.FindSafeExit(lamp, out _)); Object.Destroy(blocker); yield return null;
    }
    [UnityTest] public IEnumerator AllSixAbilitiesWorkInAuthoredAreas()
    {
        if (!Application.CanStreamedLevelBeLoaded("ChildRoom")) Assert.Ignore("Full content not generated yet.");
        var seen = new System.Collections.Generic.HashSet<string>();
        for (int area = 0; area < 4; area++)
        {
            yield return app.LoadArea(area, 0);
            foreach (var obj in app.Level.Possessables)
            {
                if (!seen.Add(obj.key)) continue;
                var ghost = app.Level.Ghost; ghost.Teleport((Vector2)obj.transform.position + Vector2.left);
                yield return ghost.Possession.Enter(obj); Assert.AreSame(obj, ghost.Possession.Current);
                if (obj is ToyCarPossessable || obj is HeavyPossessable)
                {
                    float x = obj.transform.position.x; app.Input.TouchPress(1); yield return new WaitForSeconds(.4f); app.Input.TouchRelease(1);
                    Assert.Greater(obj.transform.position.x, x + .15f, obj.key + " must physically move");
                }
                else
                {
                    app.Input.TouchPress(4); yield return null; yield return new WaitForSeconds(.1f);
                    Assert.IsTrue(obj.Active, obj.key + " ability must activate");
                }
                Assert.IsTrue(ghost.Possession.TryExit(), obj.key + " safe exit");
                if (obj is FanPossessable fan)
                {
                    ghost.Teleport((Vector2)fan.transform.position + fan.windOffset); yield return new WaitForSeconds(.3f);
                    Assert.Greater(ghost.Body.linearVelocity.y, 1, "Fan must lift the ghost");
                }
                if (obj is ClockPossessable) { yield return new WaitForFixedUpdate(); Assert.AreEqual(1, Time.timeScale); }
            }
        }
        Assert.AreEqual(6, seen.Count);
    }
    [UnityTest] public IEnumerator CarFitsTunnelAndActivatesRemotePlate()
    {
        if (!Application.CanStreamedLevelBeLoaded("ChildRoom")) Assert.Ignore("Full content not generated yet.");
        yield return app.LoadArea(1, 0);
        var car = app.Level.Possessables[0]; Assert.IsInstanceOf<ToyCarPossessable>(car);
        yield return app.Level.Ghost.Possession.Enter(car); app.Input.TouchPress(1);
        yield return new WaitForSeconds(2.45f); app.Input.TouchRelease(1);
        Assert.Greater(car.transform.position.x, 15);
        var plate = app.Level.Room.GetComponentInChildren<PressurePlate>(); Assert.IsTrue(plate.Latched);
        Assert.IsTrue(app.Level.Ghost.Possession.TryExit());
    }
    float routeApex;
    IEnumerator WalkTo(float x, float timeout = 8)
    {
        var ghost = app.Level.Ghost; int direction = x > ghost.transform.position.x ? 1 : 0;
        app.Input.TouchPress(direction); float until = Time.time + timeout;
        while (Time.time < until && (direction == 1 ? ghost.transform.position.x < x : ghost.transform.position.x > x)) { routeApex = Mathf.Max(routeApex, ghost.transform.position.y); yield return null; }
        app.Input.TouchRelease(direction);
        Assert.Less(Mathf.Abs(ghost.transform.position.x - x), .5f, "Cannot reach x=" + x + "; ghost=" + ghost.transform.position);
    }
    IEnumerator JumpTo(float x)
    {
        var ghost = app.Level.Ghost; float until = Time.time + 3;
        while (!ghost.Grounded && Time.time < until) yield return new WaitForFixedUpdate();
        Assert.IsTrue(ghost.Grounded, "Jump starts on a reachable surface at " + ghost.transform.position);
        Vector2 start = ghost.transform.position; routeApex = start.y;
        app.Input.TouchPress(2); yield return WalkTo(x, 1.5f);
        yield return new WaitForSeconds(.8f); app.Input.TouchRelease(2);
        Assert.IsTrue(ghost.Grounded, "No landing surface at " + ghost.transform.position + "; start=" + start + "; apex=" + routeApex);
    }
    [UnityTest] public IEnumerator LampBridgeAndMemoryAreReachableWithNormalJumps()
    {
        yield return app.LoadArea(0, 1);
        var lamp = app.Level.Room.GetComponentInChildren<LampPossessable>();
        yield return app.Level.Ghost.Possession.Enter(lamp); lamp.SetActive(true); Assert.IsTrue(app.Level.Ghost.Possession.TryExit());
        yield return WalkTo(32.2f); yield return new WaitForSeconds(.35f);
        yield return JumpTo(34); yield return JumpTo(37); yield return JumpTo(40); yield return JumpTo(42);
        Assert.IsTrue(app.Data.memories.Contains("Attic.memory.1"));
        yield return JumpTo(46); yield return WalkTo(51.3f);
        yield return null; Assert.AreEqual(2, app.Level.CurrentRoom);
        yield return WalkTo(50); yield return null; Assert.AreEqual(1, app.Level.CurrentRoom, "Backtracking must select the room being visited");
    }
    [UnityTest] public IEnumerator FanMovesCrateOntoPlateAndLiftsGhostToLedge()
    {
        yield return app.LoadArea(2, 1);
        var fan = app.Level.Room.GetComponentInChildren<FanPossessable>();
        yield return app.Level.Ghost.Possession.Enter(fan); fan.SetActive(true); yield return new WaitForSeconds(2.3f);
        Assert.IsTrue(app.Level.Room.GetComponentInChildren<PressurePlate>().Latched, "crate=" + fan.lightCrate.position + "; velocity=" + fan.lightCrate.linearVelocity);
        Assert.IsTrue(app.Level.Ghost.Possession.TryExit()); yield return WalkTo(33);
        yield return new WaitForSeconds(1); Assert.Greater(app.Level.Ghost.transform.position.y, 5);
        yield return WalkTo(35); yield return new WaitForSeconds(1.5f);
        yield return JumpTo(37.7f); yield return JumpTo(40.5f);
        Assert.Greater(app.Level.Ghost.transform.position.y, 4.8f);
    }
    [UnityTest] public IEnumerator HeavyFigureOpensGateAndReachesHighMemory()
    {
        yield return app.LoadArea(3, 2);
        var heavy = app.Level.Room.GetComponentInChildren<HeavyPossessable>();
        yield return app.Level.Ghost.Possession.Enter(heavy); app.Input.TouchPress(1);
        float until = Time.time + 10;
        while (heavy.transform.position.x < 66 && Time.time < until) yield return null;
        app.Input.TouchRelease(1); Assert.GreaterOrEqual(heavy.transform.position.x, 66);
        Assert.IsTrue(app.Level.Room.GetComponentInChildren<PressurePlate>().Latched);
        Assert.IsTrue(app.Level.Ghost.Possession.TryExit()); yield return new WaitForSeconds(.5f);
        yield return JumpTo(64); yield return JumpTo(66.3f);
        Assert.IsTrue(app.Data.memories.Contains("Workshop.memory.2"));
    }
    [UnityTest] public IEnumerator FinalExitSavesCompletionAndShowsEnding()
    {
        yield return app.LoadArea(4, 5);
        app.Level.Ghost.Teleport(new Vector2(155.5f, 6.2f));
        yield return new WaitForSecondsRealtime(13.5f);
        Assert.AreEqual(GamePhase.Ending, app.State.Phase); Assert.IsTrue(SaveSystem.Load().completed);
        Assert.AreEqual("Ending", SceneManager.GetActiveScene().name);
        Assert.NotNull(Object.FindFirstObjectByType<EndingDawn>());
        Assert.IsTrue(System.Array.Exists(app.UI.GetComponentsInChildren<UnityEngine.UI.Text>(), t => t.text == app.Text.Get("ending.title")));
    }
}
