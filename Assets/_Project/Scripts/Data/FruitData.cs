using UnityEngine;

[CreateAssetMenu(fileName = "NewFruitData", menuName = "Fruit Ninja/Fruit Data")]
public class FruitData : ScriptableObject
{
    [Header("Basic Info")]
    public string fruitName;
    public int scoreValue = 1;
    public bool isBomb = false;

    [Header("Visuals")]
    public GameObject wholePrefab;      // Prefab trái cây nguyên vẹn
    public GameObject slicedPrefab;     // Prefab chứa 2 nửa trái cây đã cắt
    public GameObject juiceParticle;    // Hiệu ứng nước ép văng ra
}