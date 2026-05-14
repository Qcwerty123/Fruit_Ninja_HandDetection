using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScaler : MonoBehaviour
{
	private class Data
	{
		public Transform target;
		public Vector3 baseScale;
		public float currentScale = 1;
		public float targetScale = 1;
		public float speed = 1;
		public Action onComplete;
		public bool finished = false;


		public Data(Transform target, float targetScale, float speed, Action onComplete)
		{
			this.target = target;
			baseScale = target.localScale;
			currentScale = 1;
			this.targetScale = targetScale;
			this.speed = speed;
			this.onComplete = onComplete;
			finished = false;
		}
	}

	private static ObjectScaler instance = null;
	private static List<Data> objectList = null;

	public static void Add(Transform target, float targetScale, float speed = 1, Action onComplete = null)
	{
		if(instance == null)
		{
			instance = new GameObject("ObjectScaler", typeof(ObjectScaler)).GetComponent<ObjectScaler>();
			objectList = new List<Data>();
		}
		for(int i = 0; i < objectList.Count; i++)
		{
			if(objectList[i].target == target)
			{
				Data found = objectList[i];
				found.targetScale = targetScale;
				found.speed = speed;
				found.onComplete = onComplete;
				found.finished = false;
				return;
			}
		}
		objectList.Add(new Data(target, targetScale, speed, onComplete));
	}
	public static void Remove(Transform target)
	{
		for(int i = 0; i < objectList.Count; i++)
		{
			if(objectList[i].target == target)
			{
				objectList.RemoveAt(i);
				if (instance != null)
				{
					instance.iterator--;
				}
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
					float sign = Mathf.Sign(current.targetScale - current.currentScale);
					current.currentScale = Utilities.SignClamp(current.currentScale + sign * Time.deltaTime * current.speed, sign, current.targetScale, current.targetScale);
					current.target.localScale = current.baseScale * current.currentScale;
					if (current.currentScale == current.targetScale)
					{
						current.finished = true;
						current.onComplete?.Invoke();
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
