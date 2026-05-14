using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusCriticalChance : MonoBehaviour
{
	[SerializeField] private float bonusChance = 5;

	private void Awake()
	{
		Fruit.criticalChance += bonusChance;
	}

	private void OnDestroy()
	{
		Fruit.criticalChance -= bonusChance;
	}
}
