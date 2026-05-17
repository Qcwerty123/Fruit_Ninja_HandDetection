using UnityEngine;
using R3;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ComboService
{
    private readonly GameModel _gameModel;
    private int _currentCombo;
    private CancellationTokenSource _debounceCts;

    // Cập nhật Tuple để truyền thêm cờ isCritical sang cho các View lắng nghe
    public Subject<(int comboCount, Vector2 position, bool isCritical, int criticalBonus)> ComboAchieved { get; } = new();

    public ComboService(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    // Đổi tên hàm thành AddFruitToCombo để khớp cấu trúc gọi từ BladeController
    public void AddFruitToCombo(FruitController fruit, bool isCritical)
    {
        Vector2 slicePosition = fruit.transform.position;
        _currentCombo++;

        if (isCritical)
        {
            // Truyền thẳng số điểm bonus của quả này vào sự kiện
            ComboAchieved.OnNext((0, slicePosition, true, fruit.Data.criticalBonusScore));
        }

        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        ResolveComboAsync(slicePosition, _debounceCts.Token).Forget();
    }

    private async UniTaskVoid ResolveComboAsync(Vector2 lastSlicePos, CancellationToken ct)
    {
        try
        {
            // Cửa sổ thời gian tích lũy Combo (0.15 giây)
            await UniTask.WaitForSeconds(0.15f, cancellationToken: ct);

            // Nếu qua 0.15s người chơi không chém thêm quả nào -> Chốt sổ Combo
            if (_currentCombo >= 3)
            {
                // Thưởng điểm bằng đúng số combo (Chém 4 quả = Thưởng 4 điểm)
                _gameModel.AddScore(_currentCombo);
                
                // Kích hoạt sự kiện để UI hiển thị Combo (isCritical = false vì đây là luồng Combo chuẩn)
                ComboAchieved.OnNext((_currentCombo, lastSlicePos, false, 0));
            }

            // Reset đếm lại từ đầu cho lượt vung kiếm kế tiếp
            _currentCombo = 0;
        }
        catch (System.OperationCanceledException)
        {
            // Bị hủy do có quả khác vừa bị chém đè lên trong vòng 0.15s -> Tiếp tục tích lũy Combo!
        }
    }
}