using System;
using UnityEngine;
using Reflex.Attributes;
using R3;
using System.Threading;
using Cysharp.Threading.Tasks;

public class GameFlowController : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;

    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();
        
        // Gọi hàm UniTaskVoid (Fire and Forget) mà không bị cảnh báo (warning)
        RunGameLoopAsync(_cts.Token).Forget();
    }

    // Trả về UniTask thay vì Task
    private async UniTask RunGameLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // ==========================================
                // TRẠNG THÁI 1: MAIN MENU
                // ==========================================
                ShowPanel(mainMenuPanel);
                
                // Chờ IsPlaying == true (Người chơi bấm Play)
                // Dùng WaitUntil cực kỳ tối ưu, không tốn 1 byte rác nào
                await UniTask.WaitUntil(() => _gameModel.IsPlaying.Value == true, cancellationToken: ct);

                // ==========================================
                // TRẠNG THÁI 2: ĐANG CHƠI (IN-GAME)
                // ==========================================
                ShowPanel(hudPanel);

                // Chờ cho đến khi mạng = 0 (IsGameOver == true)
                await UniTask.WaitUntil(() => _gameModel.IsGameOver.Value == true, cancellationToken: ct);

                // ==========================================
                // TRẠNG THÁI 3: GAME OVER
                // ==========================================
                // UniTask có sẵn hàm WaitForSeconds xài bằng thời gian Game (Time.deltaTime)
                await UniTask.WaitForSeconds(0.5f, cancellationToken: ct); 
                
                ShowPanel(gameOverPanel);

                // Chờ người chơi bấm nút Replay
                await UniTask.WaitUntil(() => _gameModel.IsPlaying.Value == true, cancellationToken: ct);
            }
        }
        catch (OperationCanceledException)
        {
            // UniTask ném lỗi này khi ct bị Cancel. Rất an toàn, chỉ cần log ra.
            Debug.Log("<color=green>Game Flow (UniTask) đã dừng an toàn.</color>");
        }
    }

    private void ShowPanel(GameObject activePanel)
    {
        mainMenuPanel.SetActive(activePanel == mainMenuPanel);
        hudPanel.SetActive(activePanel == hudPanel);
        gameOverPanel.SetActive(activePanel == gameOverPanel);
    }

    private void OnDestroy()
    {
        // Dọn dẹp token
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}