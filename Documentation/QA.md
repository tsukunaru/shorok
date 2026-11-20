# Verification evidence

Unity version: 6000.3.24f1. Reports are machine-generated under `Reports/`; timestamps in XML are UTC. Tests preserve and restore the Editor's prior local save.

## Automated checks

- EditMode: 10 passed, 0 failed. Save round-trip, migration/corruption handling, conflict resolution, RU/EN fallback, independent pause reasons, ad delay/cooldown, five area definitions, twenty memories, ghost transparency/body/eyes, simultaneous movement and jump input.
- PlayMode: 11 passed, 0 failed. Actual authored scenes and physics: movement/jump/landing, lamp bridge and safe exit, checkpoint/reset/respawn, manual pause preservation, obstructed exits, all six possession abilities, car tunnel and remote plate, jumping along the lamp bridge to a memory and backtracking, fan/crate/upper ledge, heavy plate/secret memory, final scene and saved completion.
- After the final camera framing change, two focused PlayMode regressions passed: movement/grounding/camera left boundary and checkpoint/reset/respawn (`Reports/camera-regression.xml`). These overlap the main suite and are not counted as additional unique tests.
- `node Tools/check-platform.cjs`: real bridge code tested against a deterministic SDK stub. Local fallback, cloud load, preservation of a future save schema, cancelled/successful reward, repeated ad-close callback and ad errors pass. These are simulated callbacks, not a claim of Yandex-hosted verification.
- Approved attic source and project copy have identical SHA-256: `D7DAA9735E177E74F9AF9C95BADC7321DCD694C2712B32B5E8FCB36F1822AD01`.

## Release evidence

`Reports/content-validation.txt` records scene, reference, localization and asset validation. `Reports/BUILD_REPORT.txt` records the actual production build and extracted ZIP sizes. Browser observations are recorded in `Reports/browser-check.md` after running the built player over HTTP.

The final production build succeeded on 2026-09-25: 68,517,188 bytes extracted (68.52 MB), 26.88 MB ZIP, extraction verification passed. Local browser checks on September 25–26 verified menu/gameplay launch, RU/EN, retained checkpoint/memories/settings, lamp possession/action/keyboard exit, movement/release and portrait/landscape layout. No browser errors or warnings were reported during the checked interactions.

## Scope of evidence

The route tests exercise representative authored puzzles with normal movement input; they do not constitute a complete human playthrough of every room. The 30–45 minute first-play duration is a design target and has not been measured with first-time players. Real Android/iOS performance and Yandex-hosted ads, cloud and SDK lifecycle require device/platform validation. Local or simulated results are not substitutes for those checks.
