using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Fruit Ninja/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Spawner Settings")]
    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 2.0f;
    public float minSpawnForce = 12f;
    public float maxSpawnForce = 17f;
    
    [Header("Game Rules")]
    public int startingLives = 3;
    public float minimumSwipeDistance = 0.5f; // Khoảng cách tối thiểu để ghi nhận 1 nét chém
}