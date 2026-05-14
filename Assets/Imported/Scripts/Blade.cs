using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blade : MonoBehaviour
{
	public static readonly List<Blade> Instances = new List<Blade>();

	[SerializeField] private LayerMask layerMask = new LayerMask();
	[SerializeField] private float minimumDistance = 0.05f;
	[SerializeField] private TrailRenderer trail = null;

	private void Awake()
	{
		Instances.Add(this);
		trail = GetComponent<TrailRenderer>();
	}

	private void OnDestroy()
	{
		Instances.Remove(this);
	}

	public TrailRenderer Trail()
	{
		return trail;
	}

	public void SetPosition(Vector3 position)
	{
		float distance = (position - transform.position).magnitude;
		if (distance >= minimumDistance)
		{
			var hits = Physics2D.RaycastAll(transform.position, position - transform.position, distance, layerMask);
			float angle = Utilities.GetAngleFromVector((position - transform.position).normalized);
			float velocity = distance / Time.deltaTime;
			for (int i = 0; i < hits.Length; i++)
			{
				ISliceable sliceable = hits[i].transform.GetComponent<ISliceable>();
				if(sliceable != null)
				{
					sliceable.Slice(velocity, angle);
				}
			}
		}
		transform.position = position;
	}
}
