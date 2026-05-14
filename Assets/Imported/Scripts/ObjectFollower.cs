using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectFollower : MonoBehaviour
{
	private class Data
	{
		public Transform objectWhichFollow;
		public Transform objectToFollow;
		public Vector3 offset;


		public Data(Transform objectWhichFollow, Transform objectToFollow, Vector3 offset)
		{
			this.objectWhichFollow = objectWhichFollow;
			this.objectToFollow = objectToFollow;
			this.offset = offset;
		}
	}

	private static ObjectFollower instance = null;
	private static List<Data> objectList = null;

	public static void Add(Transform objectWhichFollow, Transform objectToFollow, Vector3? offset = null)
	{
		if(offset == null)
		{
			offset = Vector3.zero;
		}
		if(instance == null)
		{
			instance = new GameObject("ObjectFollower", typeof(ObjectFollower)).GetComponent<ObjectFollower>();
			objectList = new List<Data>();
		}
		for(int i = 0; i < objectList.Count; i++)
		{
			if(objectList[i].objectWhichFollow == objectWhichFollow)
			{
				objectList[i].objectToFollow = objectToFollow;
				objectList[i].offset = (Vector3)offset;
			}
		}
		objectList.Add(new Data(objectWhichFollow, objectToFollow, (Vector3)offset));
	}
	public static void Remove(Transform objectWhichFollow)
	{
		for(int i = 0; i < objectList.Count; i++)
		{
			if(objectList[i].objectWhichFollow == objectWhichFollow)
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
			if (current.objectWhichFollow != null)
			{
				if (current.objectToFollow != null)
				{
					current.objectWhichFollow.position = current.objectToFollow.position + current.offset;
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
