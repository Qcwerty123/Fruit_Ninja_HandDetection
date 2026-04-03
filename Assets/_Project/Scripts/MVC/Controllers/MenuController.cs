using UnityEngine;
using Reflex.Attributes;

public class MenuController : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;

    // Gán hàm này vào sự kiện OnClick() của Button trong Inspector
    public void OnPlayButtonClicked()
    {
        _gameModel.StartGame();
    }
}