using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeSkin : MonoBehaviour, ISliceable
{
	[SerializeField] private GameObject bonus = null;
	[SerializeField] private Gradient bladeGradient = null;

	public void Slice(float _, float __)
	{
		foreach (var blade in Blade.Instances)
		{
			blade.Trail().colorGradient = bladeGradient;
		}
		GameAssets.i.blade.Trail().colorGradient = bladeGradient;
	}
}
