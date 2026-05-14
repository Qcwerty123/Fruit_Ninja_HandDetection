using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeBonus : MonoBehaviour
{
	public static GameObject bonus = null;

	private void Awake()
	{
		if(bonus != null)
		{
			Instantiate(bonus);
		}
	}
}
