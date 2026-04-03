using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;

public class AudioService
{
    private readonly IObjectPool<AudioSource> _audioPool;

    public AudioService()
    {
        // Khởi tạo Pool chứa sẵn các AudioSource
        _audioPool = new ObjectPool<AudioSource>(
            createFunc: () => {
                GameObject go = new GameObject("[AUDIO]_PooledSource");
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                Object.DontDestroyOnLoad(go); // Giữ qua các Scene
                return source;
            },
            actionOnGet: source => source.gameObject.SetActive(true),
            actionOnRelease: source => {
                source.Stop();
                source.gameObject.SetActive(false);
            },
            actionOnDestroy: source => Object.Destroy(source.gameObject),
            collectionCheck: false,
            defaultCapacity: 5,
            maxSize: 20
        );
    }

    public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        AudioSource source = _audioPool.Get();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();

        // Thu hồi AudioSource sau khi phát xong đoạn nhạc
        ReturnToPoolAsync(source, clip.length).Forget();
    }

    private async UniTaskVoid ReturnToPoolAsync(AudioSource source, float delay)
    {
        CancellationToken ct = source.GetCancellationTokenOnDestroy();
        try
        {
            await UniTask.WaitForSeconds(delay, cancellationToken: ct);
            if (source.gameObject.activeInHierarchy)
            {
                _audioPool.Release(source);
            }
        }
        catch (System.OperationCanceledException) { }
    }
}