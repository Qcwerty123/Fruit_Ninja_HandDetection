using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ComboService
{
    private readonly GameModel _gameModel;
    private int _currentCombo;
    private CancellationTokenSource _debounceCts;

    // Chuẩn Naming: Động từ quá khứ, không có chữ "On" ở đầu
    public Subject<(int comboCount, Vector2 position)> ComboAchieved { get; } = new();

    public ComboService(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    public void RecordSlice(Vector2 slicePosition)
    {
        _currentCombo++;

        // Hủy bộ đếm thời gian cũ (nếu có) để tạo bộ đếm mới (Debounce)
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        ResolveComboAsync(slicePosition, _debounceCts.Token).Forget();
    }

    private async UniTaskVoid ResolveComboAsync(Vector2 lastSlicePos, CancellationToken ct)
    {
        try
        {
            // Cửa sổ thời gian Combo (0.15 giây)
            await UniTask.WaitForSeconds(0.15f, cancellationToken: ct);

            // Nếu qua 0.15s người chơi không chém thêm quả nào -> Chốt sổ
            if (_currentCombo >= 3)
            {
                // Thưởng điểm bằng đúng số combo (Chém 4 quả = Thưởng 4 điểm)
                _gameModel.AddScore(_currentCombo);
                
                // Kích hoạt sự kiện để UI hiển thị
                ComboAchieved.OnNext((_currentCombo, lastSlicePos));
            }

            // Reset đếm lại từ đầu
            _currentCombo = 0;
        }
        catch (System.OperationCanceledException)
        {
            // Bị hủy do có quả táo khác vừa bị chém đè lên -> Nuốt lỗi để tiếp tục tích lũy!
        }
    }
}