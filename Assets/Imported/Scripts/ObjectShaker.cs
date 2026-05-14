using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectShaker : MonoBehaviour
{
	private class Data
	{
		public Transform target;
		public Vector3 basePosition;
		public float magnitude;
		public float duration;
		public float elapsed;
		public Action onComplete;
		public bool finished = false;


		public Data(Transform target, float magnitude, float duration, Action onComplete)
		{
			this.target = target;
			basePosition = target.localPosition;
			this.duration = duration;
			this.magnitude = magnitude;
			this.onComplete = onComplete;
			elapsed = 0;
			finished = false;
		}
	}

	private static ObjectShaker instance = null;
	private static List<Data> objectList = null;

	public static void Add(Transform target, float magnitude, float duration, Action onComplete = null)
	{
		if(instance == null)
		{
			instance = new GameObject("ObjectShaker", typeof(ObjectShaker)).GetComponent<ObjectShaker>();
			objectList = new List<Data>();
		}
		for(int i = 0; i < objectList.Count; i++)
		{
			if(objectList[i].target == target)
			{
				Data found = objectList[i];
				found.magnitude = magnitude;
				found.duration = duration;
				found.onComplete = onComplete;
				found.finished = false;
				found.elapsed = 0;
				return;
			}
		}
		objectList.Add(new Data(target, magnitude, duration, onComplete));
	}
	public static void Remove(Transform target)
	{
		for(int i = 0; i < objectList.Count; i++)
		{
			if(objectList[i].target == target)
			{
				objectList.RemoveAt(i);
				instance.iterator--;
				return;
			}
		}
	}

	private int iterator;

	void Update()
	{
		for (iterator = 0; iterator < objectList.Count; iterator++)
		{
			Data current = objectList[iterator];
			if (!current.finished)
			{
				if (current.target != null)
				{
					current.elapsed += Time.deltaTime;
					if (current.elapsed >= current.duration)
					{
						current.finished = true;
						current.target.localPosition = current.basePosition;
						current.onComplete?.Invoke();
					}
					else
					{
						current.target.localPosition = current.basePosition + new Vector3(Random.Range(-current.magnitude, current.magnitude), Random.Range(-current.magnitude, current.magnitude));
					}
				}
				else
				{
					objectList.RemoveAt(iterator);
					iterator--;
				}
			}
		}
	}
}
