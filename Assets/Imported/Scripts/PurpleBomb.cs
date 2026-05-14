using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurpleBomb : MonoBehaviour, ISliceable
{
	public static event Action<PurpleBomb> OnBombSpawned;

	private void Awake()
	{
		OnBombSpawned?.Invoke(this);
	}

	public void Slice(float velocity, float cutRotation)
	{
		BananaBonus.Instance.RemoveAll();
		int points = Score.Instance.score;
		points = Mathf.Min(points, 10);
		Score.Instance.AddPoints(-points);
		foreach(var fruit in Fruit.Instances)
		{
			Destroy(fruit.gameObject);
		}

		foreach (var sliced in SlicedFruit.Instances)
		{
			Destroy(sliced.gameObject);
		}
		Image flash = Utilities.MainCanvas.transform.Find("Flash").GetComponent<Image>();
		flash.rectTransform.SetAsLastSibling();
		Color color = flash.color;
		color.a = 1;
		flash.color = color;

		Spawner.Instance["Main"].enabled = false;

		FunctionTimer.Create(() =>
		FunctionUpdater.Create(() =>
		{
			color.a -= Time.deltaTime * 3;
			flash.color = color;
			if(color.a <= 0)
			{
				Spawner.Instance["Main"].enabled = true;
				return true;
			}
			return false;
		}), .5f);
		Destroy(gameObject);
	}
}
