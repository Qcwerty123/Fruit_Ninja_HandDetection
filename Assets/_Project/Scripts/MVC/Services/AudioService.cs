using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Audio;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;

// Chuyển thành MonoBehaviour để kéo thả AudioMixer vào Inspector
public class AudioService : MonoBehaviour 
{
    [Header("Mixer & Routing")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup; // Dây cắm cho các loa trong Pool
    
    [Header("Music Source")]
    [SerializeField] private AudioSource musicSource;

    private ObjectPool<AudioSource> _audioPool;

    public bool IsMusicMuted { get; private set; }
    public bool IsSfxMuted { get; private set; }

    private void Awake()
    {
        // Khởi tạo Pool
        _audioPool = new ObjectPool<AudioSource>(
            createFunc: () => {
                GameObject go = new GameObject("[AUDIO]_PooledSource");
                go.transform.SetParent(this.transform); // Gom gọn vào AudioService cho đỡ rác Hierarchy
                
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = sfxGroup; // QUAN TRỌNG: Cắm dây loa vào SFX Mixer
                
                return source;
            },
            actionOnGet: source => source.gameObject.SetActive(true),
            actionOnRelease: source => {
                source.Stop();
                source.gameObject.SetActive(false);
            },
            actionOnDestroy: source => Destroy(source.gameObject),
            collectionCheck: false,
            defaultCapacity: 5,
            maxSize: 20
        );
    }

    private void Start()
    {
        // Khôi phục cài đặt từ lần chơi trước
        IsMusicMuted = PlayerPrefs.GetInt("Settings_MusicMuted", 0) == 1;
        IsSfxMuted = PlayerPrefs.GetInt("Settings_SfxMuted", 0) == 1;

        ApplyMusicSetting();
        ApplySfxSetting();
    }

    // ================= CHƠI NHẠC NỀN (BGM) =================
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // ================= CHƠI HIỆU ỨNG (SFX bằng POOL) =================
    public void PlaySFX(AudioClip clip, float volume = 1f, bool randomizePitch = true)
    {
        if (clip == null) return;

        // Bốc 1 cái loa từ trong kho ra
        AudioSource source = _audioPool.Get();
        
        source.clip = clip;
        source.volume = volume;
        source.pitch = randomizePitch ? Random.Range(0.85f, 1.15f) : 1f; // Pitch hoàn toàn độc lập
        
        source.Play();

        // Thu hồi loa sau khi phát xong
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

    // ================= CÀI ĐẶT MIXER & UI =================
    public void ToggleMusic()
    {
        IsMusicMuted = !IsMusicMuted;
        PlayerPrefs.SetInt("Settings_MusicMuted", IsMusicMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicSetting();
    }

    public void ToggleSFX()
    {
        IsSfxMuted = !IsSfxMuted;
        PlayerPrefs.SetInt("Settings_SfxMuted", IsSfxMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplySfxSetting();
    }

    private void ApplyMusicSetting()
    {
        mainMixer.SetFloat("MusicVol", IsMusicMuted ? -80f : 0f);
    }

    private void ApplySfxSetting()
    {
        mainMixer.SetFloat("SFXVol", IsSfxMuted ? -80f : 0f);
    }

    public void SetMusicVolume(float normalizedVolume)
    {
        // Công thức chuyển đổi Linear (0-1) sang Decibel (-80 đến 0)
        // Mathf.Max(0.0001f, ...) để tránh lỗi Toán học Log10(0) gây crash game
        float decibel = Mathf.Log10(Mathf.Max(0.0001f, normalizedVolume)) * 20f;
        
        mainMixer.SetFloat("MusicVol", decibel);

        // Lưu lại mức âm lượng
        PlayerPrefs.SetFloat("Settings_MusicVolValue", normalizedVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float normalizedVolume)
    {
        float decibel = Mathf.Log10(Mathf.Max(0.0001f, normalizedVolume)) * 20f;
        mainMixer.SetFloat("SFXVol", decibel);

        PlayerPrefs.SetFloat("Settings_SfxVolValue", normalizedVolume);
        PlayerPrefs.Save();
    }
}