using UnityEngine;
using TMPro;

public class ComboTexts : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private TextMeshProUGUI[] numberTexts;

    // Hàm này được ComboPopupView gọi để nạp dữ liệu
    public void Setup(int amount, TMP_FontAsset font)
    {
        // 1. Đổi font cho chữ "COMBO"
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null) texts[i].font = font;
        }

        // 2. Đổi font và cập nhật số cho con số
        for (int i = 0; i < numberTexts.Length; i++)
        {
            if (numberTexts[i] != null)
            {
                numberTexts[i].font = font;
                numberTexts[i].text = amount.ToString();
            }
        }
    }

    // Hàm hỗ trợ reset Alpha của tất cả text về 1 (dùng cho vòng lặp Pool)
    public void ResetAlpha()
    {
        SetAlpha(1f);
    }

    // Hàm hỗ trợ chỉnh độ mờ (Fade)
    public void SetAlpha(float alpha)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null)
            {
                Color c = texts[i].color;
                c.a = alpha;
                texts[i].color = c;
            }
        }
        for (int i = 0; i < numberTexts.Length; i++)
        {
            if (numberTexts[i] != null)
            {
                Color c = numberTexts[i].color;
                c.a = alpha;
                numberTexts[i].color = c;
            }
        }
    }
}