using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCollider : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Destroy(collision.gameObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		Destroy(other.gameObject);
	}
}
