using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SplashEffects
{
	public static void CreateSplash(Vector3 position, short count, short cycles, float interval, float sizeMin, float sizeMax, Color color, Transform parent = null, float radius = 0.00015f, float radiusThiccness = 0)
	{
		ParticleSystem splashParticles = Object.Instantiate(GameAssets.i.splash, position, Quaternion.identity);
		Utilities.SetParticleCircleShape(splashParticles, radius, radiusThiccness);
		Utilities.SetParticlesEmissionBurst(splashParticles, 0, count, cycles, interval);
		Utilities.SetParticlesColor(splashParticles, color);
		Utilities.SetParticlesSize(splashParticles, sizeMin, sizeMax);
		if(parent != null)
		{
			ObjectFollower.Add(splashParticles.transform, parent);
		}
		splashParticles.Play();
	}

	public static void CreateForwardSplash(Vector3 spawnPosition, float rotation, float sizeMin, float sizeMax, Color color)
	{
		ParticleSystem forwardSplash = Object.Instantiate(GameAssets.i.forwardSplash, spawnPosition, Quaternion.Euler(0, 0, rotation));
		Utilities.SetParticlesSize(forwardSplash, sizeMin, sizeMax);
		Utilities.SetParticlesColor(forwardSplash, color);
		forwardSplash.Play();
	}
}
