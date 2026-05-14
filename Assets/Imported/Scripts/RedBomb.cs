using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class RedBomb : MonoBehaviour, ISliceable
{
	public static event Action<RedBomb> OnBombSpawned;

	private void Awake()
	{
		OnBombSpawned?.Invoke(this);
	}

	public void Slice(float velocity, float cutRotation)
	{
		Player.Instance.Miss();
		Player.Instance.Miss();
		Player.Instance.Miss();
	}
}
