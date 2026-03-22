using R3;
using UnityEngine;

public class GameModel
{
    // Reactive Properties
    public ReactiveProperty<int> Score { get; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> Lives { get; } = new ReactiveProperty<int>(3);
    
    // Quản lý luồng Game Flow
    public ReactiveProperty<bool> IsGameOver { get; } = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> IsPlaying { get; } = new ReactiveProperty<bool>(false);

    private readonly int _maxLives;

    public GameModel(int startingLives)
    {
        _maxLives = startingLives;
        Lives.Value = _maxLives;
    }

    public void StartGame()
    {
        Score.Value = 0;
        Lives.Value = _maxLives;
        IsGameOver.Value = false;
        IsPlaying.Value = true;
    }

    public void AddScore(int points)
    {
        if (IsGameOver.Value || !IsPlaying.Value) return;
        Score.Value += points;
    }

    public void LoseLife()
    {
        if (IsGameOver.Value || !IsPlaying.Value) return;
        
        Lives.Value--;
        if (Lives.Value <= 0)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (IsGameOver.Value) return;
        
        Lives.Value = 0;
        IsGameOver.Value = true;
        IsPlaying.Value = false;
        Debug.Log("<color=red>GAME OVER!</color>");
    }
}