using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseFruit : Sliceable
{
	[SerializeField] protected SlicedFruit slicedPrefab = null;

	public override void Slice(float velocity, float cutRotation)
	{
		if (slicedPrefab != null)
		{
			SlicedFruit.Spawn(slicedPrefab, transform.position, velocity, cutRotation, false);
		}
		base.Slice(velocity, cutRotation);
	}
}
