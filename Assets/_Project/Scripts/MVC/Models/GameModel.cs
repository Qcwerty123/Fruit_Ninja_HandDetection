using UnityEngine;
using R3;

public class GameModel
{
    public ReactiveProperty<int> Score { get; } = new(0);
    public ReactiveProperty<int> Lives { get; } = new(3);
    public ReactiveProperty<bool> IsGameOver { get; } = new(false);

    public void AddScore(int amount)
    {
        if (IsGameOver.Value) return;
        Score.Value += amount;
    }

    public void LoseLife()
    {
        if (IsGameOver.Value) return;
        
        Lives.Value--;
        if (Lives.Value <= 0)
        {
            IsGameOver.Value = true;
        }
    }

    public void ResetGame(int startingLives)
    {
        Score.Value = 0;
        Lives.Value = startingLives;
        IsGameOver.Value = false;
    }
}