using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CriticalText : MonoBehaviour
{
	public static CriticalText Create(Vector3 position, int amount)
	{
		position.z = -5f;
		CriticalText criticalText = Instantiate(GameAssets.i.criticalText, position, Quaternion.identity, Utilities.MainCanvas.transform);

		criticalText.numberText.text = amount.ToString();

		ObjectScaler.Add(criticalText.transform, 10, 100,
		() => FunctionTimer.Create(() =>
		{
			if (criticalText != null)
			{
				ObjectScaler.Add(criticalText.transform, 0, 90, () => Destroy(criticalText.gameObject));
			}
		}, 0.5f));

		return criticalText;
	}

	private void OnDestroy()
	{
		ObjectScaler.Remove(transform);
	}

	[SerializeField] private TextMeshProUGUI numberText = null;
}
