using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SO_GameSettings", menuName = "Fruit Ninja/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Gameplay Rules")]
    [Tooltip("Số mạng tối đa của người chơi khi bắt đầu game")]
    [SerializeField] private int _startingLives = 3;

    [Header("Global Content")]
    [Tooltip("Danh sách toàn bộ trái cây và bom có thể xuất hiện trong game")]
    [SerializeField] private List<FruitData> _availableFruits = new List<FruitData>();

    [Header("Spawn Settings (Độ khó)")]
    [SerializeField] private float _minDelay = 0.4f;
    [SerializeField] private float _maxDelay = 1.2f;

    [Header("Spawn Rates")]
    [Range(0f, 1f)] 
    [Tooltip("Tỷ lệ ra Bom: 0 = Không bao giờ, 1 = 100% ra Bom, 0.15 = 15%")]
    [SerializeField] private float bombSpawnChance = 0.15f; // Mặc định 15%

    [Header("Launch Physics (Vật lý ném)")]
    [SerializeField] private float _minForce = 13f;
    [SerializeField] private float _maxForce = 17f;
    [SerializeField] private float _minTorque = -20f;
    [SerializeField] private float _maxTorque = 20f;
    [SerializeField] private float _maxAngle = 15f;

    // --- PUBLIC PROPERTIES (Read-only) ---
    public int StartingLives => _startingLives;
    public IReadOnlyList<FruitData> AvailableFruits => _availableFruits;

    // Properties cho Spawner
    public float MinDelay => _minDelay;
    public float MaxDelay => _maxDelay;
    public float MinForce => _minForce;
    public float MaxForce => _maxForce;
    public float MinTorque => _minTorque;
    public float MaxTorque => _maxTorque;
    public float MaxAngle => _maxAngle;
    public float BombSpawnChance => bombSpawnChance;
}