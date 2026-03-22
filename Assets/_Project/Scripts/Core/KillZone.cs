using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    [Inject] private readonly GameModel _gameModel;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out FruitController fruit))
        {
            fruit.Despawn();
            
            // Xử lý mất mạng
            if (_gameModel != null && _gameModel.IsPlaying.Value && !_gameModel.IsGameOver.Value)
            {
                _gameModel.LoseLife();
                Debug.Log($"<color=orange>HỤT! CÒN LẠI: {_gameModel.Lives.Value} MẠNG</color>");
            }
        }
    }
}