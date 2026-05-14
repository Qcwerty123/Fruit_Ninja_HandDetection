using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sliceable : MonoBehaviour, ISliceable
{
	[SerializeField] protected UnityEvent onSliced;

	public virtual void Slice(float velocity, float cutRotation)
	{
		onSliced?.Invoke();
		Destroy(gameObject);
	}
}
