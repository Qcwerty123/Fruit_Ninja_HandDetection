using UnityEngine;
using TMPro;

public class CriticalTexts : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText = null;

    // Nạp con số hiển thị (Ví dụ: +3)
    public void Setup(int amount)
    {
        if (numberText != null)
        {
            numberText.text = amount.ToString(); 
        }
    }

    // Hàm hỗ trợ để tạo hiệu ứng mờ dần (Fade)
    public void SetAlpha(float alpha)
    {
        if (numberText != null)
        {
            Color c = numberText.color;
            c.a = alpha;
            numberText.color = c;
        }
    }
}