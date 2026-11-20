# Yandex release checklist

Authoritative references checked 2026-09-24:
- [Requirements](https://yandex.ru/dev/games/doc/ru/concepts/requirements)
- [Loading and gameplay](https://yandex.ru/dev/games/doc/ru/sdk/sdk-game-events)
- [Events](https://yandex.ru/dev/games/doc/ru/sdk/sdk-events)
- [Player data](https://yandex.ru/dev/games/doc/ru/sdk/sdk-player)
- [Ads](https://yandex.ru/dev/games/doc/ru/sdk/sdk-adv)

The uncompressed file limit is 100 MB. The build pipeline rejects 99 MB or above; the optimization target is at most 70 MB. ZIP root must contain `index.html`. Folder names and release files are ASCII without spaces.

## Automated/local evidence
Final results are recorded in `Reports/BUILD_REPORT.txt`, test XML and `Reports/content-validation.txt`.

- [x] Production Web build, no development flag
- [x] EditMode: 10 passed; PlayMode: 11 passed; focused camera regressions: 2 passed
- [x] Content/reference/import validation passes
- [x] Browser launches menu and gameplay over HTTP
- [x] RU and EN readable; no missing glyphs
- [x] Movement/jump/possession in Unity tests; browser movement, lamp possession/action/keyboard exit and pause observed
- [x] Simultaneous move+jump input tested; browser touch movement/release and portrait pause observed
- [x] Local save and language survive page refresh and browser restart
- [x] Pause reasons preserve manual pause in tests
- [ ] Audible sound suspension on real device/platform focus/ads/orientation (state handling implemented; no listening test claimed)
- [x] ASCII names, size and ZIP extraction checked: 68.52 MB extracted, 26.88 MB ZIP

See `Reports/browser-check.md` for exact browser evidence and limits.

## Yandex draft verification
These require the uploaded game in the developer console, with real platform callbacks.

- [ ] Loading ready only after interactive menu and save/platform initialization
- [ ] Gameplay indicator starts/stops at actual control transitions
- [ ] game_api_pause/resume and focus transitions
- [ ] Guest play and cloud save/failure fallback
- [ ] Interstitial close/error/offline restore prior state
- [ ] Reward only after onRewarded; cancellation does not give a hint
- [ ] Android and iOS Safari devices, resize, long press and no page scrolling
- [ ] Add store screenshots, localized description, supported platforms and submit for moderation
