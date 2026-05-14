using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Achievement
{
	public bool Done { get; protected set; } = false;
	
	public abstract string Name { get; }

	public event Action OnComplete;

	public abstract void OnEnable();
	public abstract void OnDisable();
	public abstract float Percentage();
}

public abstract class Achievement_FruitCount : Achievement
{
	protected abstract int CountRequired { get; }
	protected int currentCount;

	public sealed override void OnEnable()
	{
		currentCount = FruitCount.sliceCount;
		FruitCount.OnCountIncrease += FruitCount_OnCountIncrease;
	}

	private void FruitCount_OnCountIncrease(int obj)
	{
		currentCount = obj;
	}

	public override void OnDisable()
	{
		FruitCount.OnCountIncrease -= FruitCount_OnCountIncrease;
	}

	public override sealed float Percentage()
	{
		return currentCount * 1f / CountRequired;
	}
}

public class Achievemen_FruitFight : Achievement_FruitCount
{
	public override string Name => "Fruit Fight";

	protected override int CountRequired => 10;
}

public class Achievements : MonoBehaviour
{
	public static Achievements Instance { get; private set; }


}
