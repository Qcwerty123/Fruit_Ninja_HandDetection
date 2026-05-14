using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
	[Header("X")]
	[SerializeField] private float speedXMin = 0;
	[SerializeField] private float speedXMax = 0;
	[Header("Y")]
	[SerializeField] private float speedYMin = 0;
	[SerializeField] private float speedYMax = 0;
	[Header("Z")]
	[SerializeField] private float speedZMin = 0;
	[SerializeField] private float speedZMax = 0;

	private Vector3 speed;

	private void Awake()
	{
		speed = new Vector3(Random.Range(speedXMin, speedXMax), Random.Range(speedYMin, speedYMax), Random.Range(speedZMin, speedZMax));
	}

	void Update()
    {
		transform.Rotate(speed * Time.deltaTime);
    }
}
