using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Audio;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;

public class AudioService : MonoBehaviour 
{
    [Header("Mixer & Routing")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup; 
    
    [Header("Music Source")]
    [SerializeField] private AudioSource musicSource;

    private ObjectPool<AudioSource> _audioPool;

    public bool IsMusicMuted { get; private set; }
    public bool IsSfxMuted { get; private set; }

    private void Awake()
    {
        _audioPool = new ObjectPool<AudioSource>(
            createFunc: () => {
                GameObject go = new GameObject("[AUDIO]_PooledSource");
                go.transform.SetParent(this.transform); 
                
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.outputAudioMixerGroup = sfxGroup; 
                
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
        // 1. Khôi phục trạng thái Mute
        IsMusicMuted = PlayerPrefs.GetInt("Settings_MusicMuted", 0) == 1;
        IsSfxMuted = PlayerPrefs.GetInt("Settings_SfxMuted", 0) == 1;

        // 2. Khôi phục âm lượng Slider (Mặc định là 1f - 100%)
        float savedBGMVol = GetBGMVolume();
        float savedSfxVol = GetSFXVolume();

        // 3. Áp dụng ngay khi khởi động
        SetBGMVolume(savedBGMVol);
        SetSFXVolume(savedSfxVol);
        
        ApplyMusicSetting();
        ApplySfxSetting();
    }

    // ================= CHƠI NHẠC NỀN & SFX =================
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, bool randomizePitch = true)
    {
        if (clip == null) return;

        AudioSource source = _audioPool.Get();
        source.clip = clip;
        source.volume = volume;
        source.pitch = randomizePitch ? Random.Range(0.85f, 1.15f) : 1f; 
        source.Play();

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

    // ================= GETTERS CHO UI SETTINGS =================
    public float GetBGMVolume()
    {
        return PlayerPrefs.GetFloat("Settings_MusicVolValue", 1f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("Settings_SfxVolValue", 1f);
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
        // Nếu Mute thì ép về -80dB. Nếu Unmute thì khôi phục lại mức Volume của Slider.
        if (IsMusicMuted)
            mainMixer.SetFloat("MusicVol", -80f);
        else
            SetBGMVolume(GetBGMVolume());
    }

    private void ApplySfxSetting()
    {
        if (IsSfxMuted)
            mainMixer.SetFloat("SFXVol", -80f);
        else
            SetSFXVolume(GetSFXVolume());
    }

    public void SetBGMVolume(float normalizedVolume)
    {
        float decibel = Mathf.Log10(Mathf.Max(0.0001f, normalizedVolume)) * 20f;
        
        // Chỉ thay đổi Mixer nếu KHÔNG bị Mute
        if (!IsMusicMuted)
        {
            mainMixer.SetFloat("MusicVol", decibel);
        }

        PlayerPrefs.SetFloat("Settings_MusicVolValue", normalizedVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float normalizedVolume)
    {
        float decibel = Mathf.Log10(Mathf.Max(0.0001f, normalizedVolume)) * 20f;
        
        if (!IsSfxMuted)
        {
            mainMixer.SetFloat("SFXVol", decibel);
        }

        PlayerPrefs.SetFloat("Settings_SfxVolValue", normalizedVolume);
        PlayerPrefs.Save();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null; // Giải phóng clip cũ
        }
    }
}