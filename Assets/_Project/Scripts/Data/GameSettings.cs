using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SO_GameSettings", menuName = "Fruit Ninja/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Gameplay Rules")]
    [SerializeField] private int _startingLives = 10;

    [Header("Fruit Libraries")]
    [Tooltip("Danh sách trái cây dùng cho chế độ Classic (Máy yếu)")]
    [SerializeField] private List<FruitData> _classicFruits = new List<FruitData>();
    
    [Tooltip("Danh sách trái cây dùng cho chế độ Dynamic (Toán học)")]
    [SerializeField] private List<FruitData> _dynamicFruits = new List<FruitData>();

    [Header("Spawn Settings")]
    [SerializeField] private float _minDelay = 0.4f;
    [SerializeField] private float _maxDelay = 1.2f;
    [Range(0f, 1f)] 
    [SerializeField] private float bombSpawnChance = 0.15f; 

    [Header("Launch Physics")]
    [SerializeField] private float _minForce = 15f;
    [SerializeField] private float _maxForce = 17f;
    [SerializeField] private float _minTorque = -5f;
    [SerializeField] private float _maxTorque = 5f;
    [SerializeField] private float _maxAngle = 15f;

    // --- PUBLIC PROPERTIES ---
    public int StartingLives => _startingLives;

    // Hàm thông minh: Tự động trả về danh sách quả dựa trên Mode đang chơi
    public IReadOnlyList<FruitData> GetAvailableFruits(GameMode mode)
    {
        return mode == GameMode.Dynamic ? _dynamicFruits : _classicFruits;
    }

    public float MinDelay => _minDelay;
    public float MaxDelay => _maxDelay;
    public float MinForce => _minForce;
    public float MaxForce => _maxForce;
    public float MinTorque => _minTorque;
    public float MaxTorque => _maxTorque;
    public float MaxAngle => _maxAngle;
    public float BombSpawnChance => bombSpawnChance;
}