using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicedFruit : MonoBehaviour
{
	public static List<SlicedFruit> Instances = new List<SlicedFruit>();

	public static SlicedFruit Spawn(SlicedFruit prefab, Vector3 spawnPosition, float velocity, float rotation, bool critical = false)
	{
		float multiplier;
		SlicedFruit slicedFruit = Instantiate(prefab, spawnPosition, Quaternion.Euler(0, 0, rotation));
		Vector3 forwardOne = Utilities.GetVectorFromAngle(rotation - 90);
		Vector3 forwardTwo = Utilities.GetVectorFromAngle(rotation + 90);
		if (!critical)
		{
			multiplier = Mathf.Clamp(velocity / 100, 1f, 2f);
			slicedFruit.partOne.AddForce(-forwardOne * Random.Range(2f, 4f) * multiplier, ForceMode.Impulse);
			slicedFruit.partTwo.AddForce(-forwardTwo * Random.Range(2f, 4f) * multiplier, ForceMode.Impulse);
		}
		else
		{
			multiplier = 35;
			slicedFruit.partOne.AddForce(-forwardOne * multiplier, ForceMode.Impulse);
			slicedFruit.partTwo.AddForce(-forwardTwo * multiplier, ForceMode.Impulse);
		}
		slicedFruit.partOne.AddTorque(Random.Range(0.5f, 2f) * -Mathf.Sign(forwardOne.y), Random.Range(-1f, 1f), Random.Range(-1f, 1f), ForceMode.Impulse);
		slicedFruit.partTwo.AddTorque(Random.Range(0.5f, 2f) * -Mathf.Sign(forwardTwo.y), Random.Range(-1f, 1f), Random.Range(-1f, 1f), ForceMode.Impulse);
		return slicedFruit;
	}

	[SerializeField] Rigidbody partOne = null;
	[SerializeField] Rigidbody partTwo = null;

	private void Awake()
	{
		Instances.Add(this);
		Destroy(gameObject, 5);
	}

	private void OnDestroy()
	{
		Instances.Remove(this);
	}

	public Transform PartOne()
	{
		return partOne.transform;
	}

	public Transform PartTwo()
	{
		return partTwo.transform;
	}
}
