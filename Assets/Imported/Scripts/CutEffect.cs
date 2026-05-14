using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CutEffect
{
	public static void Create(Vector3 spawnPosition, float rotation)
	{
		Object.Instantiate(GameAssets.i.cut, spawnPosition, Quaternion.Euler(0, 0, rotation));
	}
}
