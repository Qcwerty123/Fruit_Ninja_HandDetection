using UnityEngine;
using Cysharp.Threading.Tasks;

public interface ISliceable
{
    // Cờ báo hiệu để phân biệt Bom và Trái cây/Nút bấm
    bool IsBomb(); 

    // Nhận Vector3 và vận tốc để truyền lực vật lý 3D
    UniTaskVoid Slice(Vector2 cutDirection, Vector2 cutStart, Vector2 cutEnd, float velocity, bool isCritical);
}