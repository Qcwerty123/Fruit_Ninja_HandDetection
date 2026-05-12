using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;

public class MainMenuView : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;
    [SerializeField] private Button playButton; // Kéo thả cục Button Play vào đây
    [SerializeField] private DwellButton dwellPlayButton; // Kéo thả cục Button Play vào đây

    private void Start()
    {
        // Tìm nút Play trong MainMenuPanel và gọi hàm StartGame
        if (playButton == null) playButton = GetComponentInChildren<Button>();
        if (dwellPlayButton == null) dwellPlayButton = GetComponentInChildren<DwellButton>();
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => _gameModel.StartGame());
            dwellPlayButton.onDwellClick.AddListener(() => _gameModel.StartGame());
        }
    }
}