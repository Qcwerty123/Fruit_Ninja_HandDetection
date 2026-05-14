using UnityEngine;

[CreateAssetMenu(fileName = "NewFruitData", menuName = "Fruit Ninja/Fruit Data")]
public class FruitData : ScriptableObject
{
    [Header("Basic Info")]
    public string fruitName;
    public int scoreValue = 1;
    public bool isBomb = false;

    [Header("Visuals - Classic Mode")]
    [Tooltip("Prefab chứa script ClassicFruit và CircleCollider2D")]
    public GameObject classicPrefab;      
    [Tooltip("Prefab chứa 2 nửa trái cây vẽ sẵn (Dùng cho Classic)")]
    public GameObject classicSlicedPrefab; 

    [Header("Visuals - Dynamic Mode")]
    [Tooltip("Prefab chứa script DynamicFruit và PolygonCollider2D")]
    public GameObject dynamicPrefab;      

//--- Old ---
    [Header("VFX & Audio")]
    public GameObject baseJuiceParticle;
    public Color splashColor = Color.white; 
    public GameObject specialJuiceParticle;
//-----------

    [Header("VFX Layers - Lớp hiệu ứng hình ảnh")]
    [Tooltip("Bụi mờ hoặc tia chớp nhỏ tại điểm chém (Khớp với tiếng Impact)")]
    public GameObject impactVFX;

    [Tooltip("Tia nước xịt mạnh có hướng (Khớp với tiếng Splatter)")]
    public GameObject splatterVFX;

    [Tooltip("Các giọt nước/bã trái cây nặng rơi xuống (Khớp với tiếng Drip/Splat)")]
    public GameObject pulpVFX;

    [Header("Audio Layers - Lớp va đập (Khô)")]
    [Tooltip("Tiếng vỏ bị đứt (Impact, Crack, Thud...)")]
    public AudioClip[] impactSounds;

    [Header("Audio Layers - Lớp xịt nước (Ướt)")]
    [Tooltip("Tiếng xịt nước ép (Splatter, Squish...)")]
    public AudioClip[] splatterSounds;

    [Header("Audio Layers - Lớp chi tiết (Tùy chọn)")]
    [Tooltip("Tiếng giọt nước, bã trái cây (Drip, Splat...)")]
    public AudioClip[] detailSounds;
}