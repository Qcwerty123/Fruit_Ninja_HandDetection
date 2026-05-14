using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleEffects : MonoBehaviour
{
	public static void CreateSmall(Vector3 spawnPosition, float rotation, Color color)
	{
		BubbleEffects bubbles = Instantiate(GameAssets.i.smallBubbles, spawnPosition, Quaternion.Euler(0, 0, rotation));
		Utilities.SetParticlesColor(bubbles.bubbles, color);
		bubbles.Play();
		Destroy(bubbles.gameObject, 1.1f);
	}

	public static void CreateBig(Vector3 spawnPosition, float rotation, Color color)
	{
		BubbleEffects bubbles = Instantiate(GameAssets.i.bigBubbles, spawnPosition, Quaternion.Euler(0, 0, rotation));
		Utilities.SetParticlesColor(bubbles.bubbles, color);
		bubbles.Play();
		Destroy(bubbles.gameObject, 1.1f);
	}
	[SerializeField] private ParticleSystem bubbles = null;
	[SerializeField] private ParticleSystem glow = null;

	public void Play()
	{
		bubbles.Play();
		glow.Play();
	}
}
