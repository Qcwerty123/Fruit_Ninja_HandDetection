using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
	private static Camera mainCamera;
	public static Camera MainCamera
	{
		get
		{
			if(mainCamera == null)
			{
				mainCamera = Camera.main;
			}
			return mainCamera;
		}
	}

	private static Canvas mainCanvas;
	public static Canvas MainCanvas
	{
		get
		{
			if (mainCanvas == null)
			{
				mainCanvas = Object.FindObjectOfType<Canvas>();
			}
			return mainCanvas;
		}
	}

	public static Vector3 GetWorldPosition(Vector3 screenPosition, float z = 0)
	{
		Vector3 position = MainCamera.ScreenToWorldPoint(screenPosition);
		position.z = z;
		return position;
	}

	public static float GetAngleFromVector(Vector3 dir)
	{
		float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		if (n < 0) n += 360;

		return n;
	}

	public static Vector3 GetVectorFromAngle(float angle)
	{
		float angleRad = angle * (Mathf.PI / 180f);
		return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
	}

	public static void SetParticlesRotation(ParticleSystem particles, float rotationMin, float rotationMax)
	{
		var main = particles.main;
		var startRotation = main.startRotation;
		if (rotationMin == rotationMax)
		{
			startRotation.mode = ParticleSystemCurveMode.Constant;
			startRotation.constant = rotationMin;
		}
		else
		{
			startRotation.mode = ParticleSystemCurveMode.TwoConstants;
			startRotation.constantMin = rotationMin;
			startRotation.constantMax = rotationMax;
		}
		main.startRotation = startRotation;
	}

	public static void SetParticlesColor(ParticleSystem particles, Color color)
	{
		var main = particles.main;
		var startColor = main.startColor;
		startColor.color = color;
		main.startColor = startColor;
	}

	public static void SetParticlesSize(ParticleSystem particles, float sizeMin, float sizeMax)
	{
		var main = particles.main;
		var startSize = main.startSize;
		if (sizeMin == sizeMax)
		{
			startSize.mode = ParticleSystemCurveMode.Constant;
			startSize.constant = sizeMin;
		}
		else
		{
			startSize.mode = ParticleSystemCurveMode.TwoConstants;
			startSize.constantMin = sizeMin;
			startSize.constantMax = sizeMax;
		}
		main.startSize = startSize;
	}

	public static void SetParticleCircleShape(ParticleSystem particles, float radius, float radiusThickness)
	{
		var shape = particles.shape;
		shape.radius = radius;
		shape.radiusThickness = radiusThickness;
	}

	public static void SetParticlesEmissionBurst(ParticleSystem particles, float time, short count, short cycles, float interval)
	{
		var emission = particles.emission;
		emission.burstCount = 1;
		emission.SetBurst(0, new ParticleSystem.Burst(time, count, count, cycles, interval));
	}

	public static float SignClamp(float value, float sign, float min, float max)
	{
		if (sign > 0)
		{
			return Mathf.Min(value, max);
		}
		return Mathf.Max(value, min);
	}
}
