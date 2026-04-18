using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;

public class AudioSettingsView : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Inject] private readonly AudioService _audioService;

    private void Start()
    {
        // 1. Lấy giá trị đã lưu cài đặt cho Slider (Mặc định là 1.0 - Max volume)
        musicSlider.value = PlayerPrefs.GetFloat("Settings_MusicVolValue", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("Settings_SfxVolValue", 1f);

        // 2. Lắng nghe sự kiện người chơi kéo thanh trượt
        // Dùng AddListener để mỗi khi kéo, giá trị sẽ bắn thẳng về AudioService
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
    }

    private void OnMusicSliderChanged(float value)
    {
        _audioService.SetMusicVolume(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        _audioService.SetSFXVolume(value);
    }

    private void OnDestroy()
    {
        // Dọn dẹp sự kiện khi UI bị hủy
        if (musicSlider != null) musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveAllListeners();
    }
}