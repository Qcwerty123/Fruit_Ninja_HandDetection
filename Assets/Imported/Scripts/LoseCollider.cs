using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseCollider : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Player.Instance.Miss();
		Destroy(collision.gameObject);
	}
}
