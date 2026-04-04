using UnityEngine;
using UnityEngine.UI;
using Reflex.Attributes;

public class MainMenuView : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;

    private void Start()
    {
        // Tìm nút Play trong MainMenuPanel và gọi hàm StartGame
        Button playButton = GetComponentInChildren<Button>(true);
        if (playButton != null)
        {
            playButton.onClick.AddListener(() => _gameModel.StartGame());
        }
    }
}