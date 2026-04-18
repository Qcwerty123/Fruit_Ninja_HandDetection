using UnityEngine;

[CreateAssetMenu(fileName = "NewFruitData", menuName = "Fruit Ninja/Fruit Data")]
public class FruitData : ScriptableObject
{
    [Header("Basic Info")]
    public string fruitName;
    public int scoreValue = 1;
    public bool isBomb = false;

    [Header("Visuals")]
    public GameObject fruitPrefab;      // Prefab trái cây nguyên vẹn
    public GameObject slicedPrefab;     // Prefab chứa 2 nửa trái cây đã cắt
    public GameObject baseJuiceParticle;    // Prefab VFX Juice mặc định
    
    [Header("VFX Settings")]
    public Color splashColor = Color.white; 
    
    [Tooltip("If left empty, will use splashColor. If assigned, will use this prefab instead.")]
    public GameObject specialJuiceParticle;

    public AudioClip sliceSound;        // Âm thanh khi bị chém

}