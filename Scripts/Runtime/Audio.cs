using System;

using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public static class Audio
    {
        public static async void LoadAndPlay(Clip clip)
        {
            using (var request = UnityWebRequestMultimedia.GetAudioClip(clip.Path, AudioType.MPEG))
            {
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    PlayAudioAndDestroy.Play(DownloadHandlerAudioClip.GetContent(request), clip.Volume);
                else
                    Log.Error(request, $"Sound Alert was not loaded:\n{request.error}");
            }
        }
    }

    [Serializable]
    public class Clip : ILogTarget
    {
        [LogInfo] public string Path;

        public float Volume;

        public Clip(Clip clip)
        {
            Volume = clip.Volume;
            Path = clip.Path;
        }
    }

    /// <summary>
    /// Plays audio once and destroy itself and audio clip.
    /// </summary>
    public class PlayAudioAndDestroy : MonoBehaviour
    {
        AudioSource _source;

        void Update()
        {
            if (!_source)
            {
                Destroy(gameObject);

                return;
            }

            if (!_source.isPlaying)
            {
                if (_source.clip)
                    Destroy(_source.clip);

                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Play audio clip once and destroy it.
        /// </summary>
        public static void Play(AudioClip clip, float volume = 1f) => Play(clip, Vector3.zero, volume);
        /// <summary>
        /// Play audio clip once and destroy it.
        /// </summary>
        public static void Play(AudioClip clip, Vector3 position, float volume = 1f)
        {
            var go = new GameObject("One shot audio");
            go.transform.position = position;
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1.0f;
            source.volume = volume;
            source.Play();

            var comp = go.AddComponent<PlayAudioAndDestroy>();
            comp._source = source;
        }
    }
}