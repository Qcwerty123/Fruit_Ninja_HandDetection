using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboText : MonoBehaviour
{
	public static ComboText Create(Vector3 position, int amount)
	{
		position.z = -5f;
		ComboText combo = Instantiate(GameAssets.i.comboText, position, Quaternion.identity, Utilities.MainCanvas.transform);
		int index = (amount - 3) % GameAssets.i.comboFonts.Length;
		for(int i = 0; i < combo.texts.Length; i++)
		{
			combo.texts[i].font = GameAssets.i.comboFonts[index];
		}
		for(int i = 0; i < combo.numberTexts.Length; i++)
		{
			combo.numberTexts[i].font = GameAssets.i.comboFonts[index];
			combo.numberTexts[i].text = amount.ToString();
		}

		ObjectScaler.Add(combo.transform, 10, 75,
		() => ObjectScaler.Add(combo.transform, 11, 30, 
		() => FunctionTimer.Create(() =>
		{
			if (combo != null)
			{
				ObjectScaler.Add(combo.transform, 0, 100, () => Destroy(combo.gameObject));
			}
		}, 0.5f)));

		return combo;
	}

	private void OnDestroy()
	{
		ObjectScaler.Remove(transform);
	}

	[SerializeField] private TextMeshProUGUI[] texts = null;
	[SerializeField] private TextMeshProUGUI[] numberTexts = null;
}
