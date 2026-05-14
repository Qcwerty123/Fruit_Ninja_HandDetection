using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LivesVisual : MonoBehaviour
{
	TextMeshProUGUI[] lives = new TextMeshProUGUI[3];

	private void Awake()
	{
		for(int i = 0; i < 3; i++)
		{
			lives[i] = transform.Find("live" + (i + 1)).GetComponent<TextMeshProUGUI>();
		}
	}

	private void Start()
	{
		Player.Instance.OnMiss += Instance_OnMiss;
	}

	private void Instance_OnMiss(int missCount)
	{
		if (missCount <= 3)
		{
			lives[missCount - 1].font = GameAssets.i.xRed;
			ObjectShaker.Add(lives[missCount - 1].transform, 10, .2f);
		}
	}
}
