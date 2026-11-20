// Runs the actual jslib in a mock SDK; no browser or platform calls.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../Assets/GameIntegration/Plugins/ShorokBridge.jslib'), 'utf8');
function harness(sdk = null) {
  const messages = [], timers = [], library = {};
  const context = { window: {}, ysdk: sdk, navigator: { language: 'ru-RU' }, console,
    ygGameInstance: { SendMessage: (...args) => messages.push(args) },
    LibraryManager: { library }, mergeInto: Object.assign, UTF8ToString: s => s,
    setTimeout: fn => timers.push(fn) };
  vm.runInNewContext(source, context);
  return { context, messages, timers, library };
}
(async () => {
  const local = harness(); local.library.ShorokInit('ShorokPlatform');
  assert.equal(JSON.parse(local.messages[0][2]).online, false);
  const callbacks = [];
  const sdk = { environment: { i18n: { lang: 'ru' } }, getPlayer: async () => ({ getData: async () => ({ shorok: '{"schemaVersion":1}' }), setData: async () => {} }),
    adv: { showRewardedVideo: request => callbacks.push(request.callbacks), showFullscreenAdv: request => callbacks.push(request.callbacks) } };
  const live = harness(sdk); live.library.ShorokInit('ShorokPlatform');
  await new Promise(resolve => setImmediate(resolve));
  assert.equal(JSON.parse(live.messages[0][2]).save, '{"schemaVersion":1}');
  live.library.ShorokAd(true); callbacks[0].onClose(); callbacks[0].onClose();
  assert.equal(live.messages.length, 2); assert.equal(live.messages[1][2], 'false');
  live.library.ShorokAd(true); callbacks[1].onRewarded(); callbacks[1].onClose(); assert.equal(live.messages[2][2], 'true');
  live.library.ShorokAd(false); callbacks[2].onError(); assert.equal(live.messages[3][2], 'false');
  const future = harness({ ...sdk, getPlayer: async () => ({ getData: async () => ({ shorok: '{"schemaVersion":9}' }) }) });
  future.library.ShorokInit('ShorokPlatform'); await new Promise(resolve => setImmediate(resolve));
  assert.equal(future.context.window.shorokBridge.player, null);
  console.log('PASS: local fallback, cloud load, future schema, reward cancel/success, duplicate close, ad error.');
})().catch(error => { console.error(error); process.exitCode = 1; });
