using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ComboSystem : MonoBehaviour
{
	public static event Action<Vector3, List<Fruitdata>> OnCombo;

	private readonly List<Fruitdata> fruitsInCombo = new List<Fruitdata>();
	private Vector3 position;
	private float lastSliceTime = 0;
	private int currentCombo = 0;

	private const float maxTimeBetweenSlices = 0.1f; 

	private void OnEnable()
	{
		Fruit.OnFruitSliced += Fruit_OnSliced;
		Fruit.OnFruitCritical += Fruit_OnSliced;
	}

	private void OnDisable()
	{
		Fruit.OnFruitSliced -= Fruit_OnSliced;
		Fruit.OnFruitCritical -= Fruit_OnSliced;
	}

	private void Fruit_OnSliced(Fruitdata fruitData, SlicedFruit sliced, float velocity, float rotation)
	{
		if(Time.time <= maxTimeBetweenSlices + lastSliceTime)
		{
			currentCombo++;
			if(currentCombo >= 3)
			{
				float maxShake = Mathf.Min(0.1f + (currentCombo - 3) * 0.025f, 0.1f + 10 * 0.025f);
				ObjectShaker.Add(Utilities.MainCamera.transform, maxShake, .1f);
			}
		}
		else
		{
			fruitsInCombo.Clear();
			currentCombo = 1;
		}
		fruitsInCombo.Add(fruitData);
		position = sliced.transform.position;
		lastSliceTime = Time.time;
	}

	private void Update()
	{
		if(currentCombo >= 3 && Time.time > lastSliceTime + maxTimeBetweenSlices)
		{
			Score.Instance.AddPoints(currentCombo);
			ComboText.Create(position, currentCombo);
			OnCombo?.Invoke(position, fruitsInCombo);
			currentCombo = 0;
		}
	}
}
