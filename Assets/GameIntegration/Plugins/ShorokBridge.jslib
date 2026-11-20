mergeInto(LibraryManager.library, {
  ShorokInit: function(targetPtr) {
    var target = UTF8ToString(targetPtr);
    window.shorokBridge = { target: target, player: null, ad: false };
    var sdk = typeof ysdk !== 'undefined' ? ysdk : null;
    var language = sdk && sdk.environment ? sdk.environment.i18n.lang : navigator.language.split('-')[0];
    var send = function(save) {
      if (!ygGameInstance) { setTimeout(function(){send(save);},50); return; }
      ygGameInstance.SendMessage(target, 'OnPlatformInitialized', JSON.stringify({language:language,save:save || '',online:!!sdk,touch:navigator.maxTouchPoints>0 || /Android|iPhone|iPad|iPod/.test(navigator.userAgent)}));
    };
    if (!sdk) { send(''); return; }
    var settled = false;
    var finish = function(save) { if (!settled) { settled = true; send(save); } };
    setTimeout(function() { finish(''); }, 7000);
    var loadedPlayer;
    sdk.getPlayer().then(function(player) {
      loadedPlayer = player;
      return player.getData(['shorok']);
    }).then(function(data) {
      if (settled) return;
      var save = typeof data.shorok === 'string' ? data.shorok : '';
      var future = false;
      try { future = save && JSON.parse(save).schemaVersion > 1; } catch(e) {}
      if (!future) window.shorokBridge.player = loadedPlayer;
      finish(save);
    })
      .catch(function() { finish(''); });
  },
  ShorokSave: function(jsonPtr) {
    var json = UTF8ToString(jsonPtr), bridge = window.shorokBridge;
    if (!bridge || !bridge.player || json.length > 65536) return;
    bridge.player.setData({shorok:json}, true).catch(function() { console.warn('Shorok: cloud unavailable; local progress retained.'); });
  },
  ShorokAd: function(rewarded) {
    var bridge = window.shorokBridge, sdk = typeof ysdk !== 'undefined' ? ysdk : null;
    var closed = false, earned = false;
    var finish = function() {
      if (closed) return;
      closed = true;
      if (bridge) bridge.ad = false;
      ygGameInstance.SendMessage('ShorokPlatform', 'OnAdClosed', earned ? 'true' : 'false');
    };
    if (!bridge || !sdk || bridge.ad) { finish(); return; }
    bridge.ad = true;
    try {
      if (rewarded) sdk.adv.showRewardedVideo({callbacks:{onOpen:function(){},onRewarded:function(){earned=true;},onClose:finish,onError:finish}});
      else sdk.adv.showFullscreenAdv({callbacks:{onOpen:function(){},onClose:finish,onError:finish,onOffline:finish}});
    } catch(error) { finish(); }
  }
});
