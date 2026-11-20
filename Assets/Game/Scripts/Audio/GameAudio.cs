using UnityEngine;
namespace Shorok
{
    public sealed class GameAudio : MonoBehaviour
    {
        AudioSource music, sfx;
        GameConfig config;
        public void Initialize(GameConfig c)
        {
            config = c; music = gameObject.AddComponent<AudioSource>(); music.loop = true; music.playOnAwake = false;
            sfx = gameObject.AddComponent<AudioSource>(); sfx.playOnAwake = false;
        }
        public void SetMusic(int index) { if (config.music.Length == 0) return; var clip = config.music[Mathf.Abs(index) % config.music.Length]; if (music.clip == clip) return; music.clip = clip; music.Play(); }
        public void Play(int index) { if (index >= 0 && index < config.effects.Length) sfx.PlayOneShot(config.effects[index]); }
        public void Apply(UserSettings settings, bool paused)
        {
            AudioListener.volume = settings.mute ? 0 : settings.master; AudioListener.pause = paused;
            music.volume = settings.music; sfx.volume = settings.sfx;
        }
    }
}
