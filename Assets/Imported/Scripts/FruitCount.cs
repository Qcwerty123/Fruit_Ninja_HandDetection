using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FruitCount
{
	public static int sliceCount { get; private set; } = PlayerPrefs.GetInt("SliceCount", 0);

	public static event Action<int> OnCountIncrease;

	[RuntimeInitializeOnLoadMethod]
	private static void Setup()
	{
		Fruit.OnFruitSliced += Fruit_OnFruitSliced;
		Fruit.OnFruitCritical += Fruit_OnFruitSliced;
	}

	private static void Fruit_OnFruitSliced(Fruitdata arg1, SlicedFruit arg2, float arg3, float arg4)
	{
		sliceCount++;
		PlayerPrefs.SetInt("SliceCount", sliceCount);
		OnCountIncrease?.Invoke(sliceCount);
	}
}
