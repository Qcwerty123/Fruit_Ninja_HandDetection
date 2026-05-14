using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	public static Player Instance { get; private set; }

	public event Action<int> OnMiss; 

	private void Awake()
	{
		Instance = this;
	}

	private int misses = 0;
	




	public void Miss()
	{
		misses++;
		OnMiss?.Invoke(misses);
		if(misses == 3)
		{
			SceneLoader.Instance.LoadScene("MainMenu");
		}
	}
}
