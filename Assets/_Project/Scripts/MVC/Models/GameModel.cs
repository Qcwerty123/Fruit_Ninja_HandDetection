using R3;
using UnityEngine;

// Định nghĩa các trạng thái độc tôn của game
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameModel
{
    // Reactive Properties để UI (UIManager, HUDView) tự động lắng nghe
    public ReactiveProperty<int> Score { get; } = new(0);
    public ReactiveProperty<int> Lives { get; }
    
    // TRẠNG THÁI DUY NHẤT (Single Source of Truth)
    public ReactiveProperty<GameState> State { get; } = new(GameState.MainMenu);

    private readonly int _maxLives;

    public GameModel(int startingLives)
    {
        _maxLives = startingLives;
        Lives = new ReactiveProperty<int>(startingLives);
        
        // Khởi đầu ở MainMenu, ta phải đóng băng thời gian để Spawner không ném trái cây
        Time.timeScale = 0f; 
    }

    // Hàm này sẽ được gọi bởi Nút "Play" ở MainMenuPanel
    public void StartGame()
    {
        Score.Value = 0;
        Lives.Value = _maxLives;
        ChangeState(GameState.Playing);
    }

    public void AddScore(int amount)
    {
        // Chỉ cho phép cộng điểm khi đang chơi (chống bug chém trúng trái cây lúc Game Over)
        if (State.Value != GameState.Playing) return;
        Score.Value += amount;
    }

    public void LoseLife()
    {
        if (State.Value != GameState.Playing) return;
        
        Lives.Value--;
        if (Lives.Value <= 0) 
        {
            ChangeState(GameState.GameOver);
        }
    }

    public void TogglePause()
    {
        if (State.Value == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
        else if (State.Value == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }

    // Hàm điều phối State trung tâm, đảm bảo tính đồng bộ của vật lý
    private void ChangeState(GameState newState)
    {
        if (State.Value == newState) return;

        State.Value = newState;

        // Quản lý vật lý tập trung: Chỉ đóng băng thời gian khi ở Menu hoặc Pause.
        // Khi Game Over, Time.timeScale vẫn = 1 để trái cây rớt nốt xuống đáy màn hình cho đẹp.
        if (newState == GameState.Paused || newState == GameState.MainMenu)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void TriggerGameOver()
    {
        // Chỉ cho phép kích hoạt Game Over nếu game đang ở trạng thái Playing
        if (State.Value == GameState.Playing)
        {
            ChangeState(GameState.GameOver);
        }
    }
}