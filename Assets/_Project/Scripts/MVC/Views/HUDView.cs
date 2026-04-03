using UnityEngine;
using TMPro;
using R3;
using Reflex.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class HUDView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText; 

    [Inject] private readonly GameModel _gameModel;

    private void Start()
    {
        _gameModel.Score
            .Subscribe(newScore => 
            {
                scoreText.text = newScore.ToString();
                // .Forget() giúp UniTask chạy ngầm mà không làm treo luồng chính
                PunchScoreEffectAsync().Forget(); 
            })
            .RegisterTo(destroyCancellationToken);

        _gameModel.Lives
            .Subscribe(newLives => 
            {
                livesText.text = $"LIVES: {newLives}";
                livesText.color = newLives <= 1 ? Color.red : Color.white;
            })
            .RegisterTo(destroyCancellationToken);
    }

    // Dùng UniTaskVoid cho các hàm chỉ chạy mà không cần ai await nó
    private async UniTaskVoid PunchScoreEffectAsync()
    {
        // Lấy token tự động từ vòng đời của GameObject
        CancellationToken ct = this.GetCancellationTokenOnDestroy();

        try
        {
            scoreText.transform.localScale = Vector3.one * 1.5f;
            
            // Chờ 0.1 giây
            await UniTask.WaitForSeconds(0.1f, cancellationToken: ct);
            
            scoreText.transform.localScale = Vector3.one;
        }
        catch (OperationCanceledException)
        {
            // Bỏ qua lỗi nếu Object bị hủy giữa chừng lúc đang giật Text
        }
    }
}