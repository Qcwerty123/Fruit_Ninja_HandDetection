using UnityEngine;
using Reflex.Attributes;

// Script này chỉ dùng để test, nó đóng vai trò thay thế cho nút "Play" trên UI
public class TestBootstrapper : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;

    private void Start()
    {
        // Ép game bắt đầu ngay lập tức để Spawner hoạt động
        _gameModel.StartGame();
        Debug.Log("<color=yellow>[TEST] Đã ép GameModel khởi động vòng lặp chơi!</color>");
    }
}