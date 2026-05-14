using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnData
{
	public Rigidbody2D gameObject;
	public Transform[] spawnPoints;
	public float minSpawnTime;
	public float maxSpawnTime;
	public float minSpeed;
	public float maxSpeed;
	[HideInInspector] public float timer;
	[HideInInspector] public float timerMax;

	public float RandomSpawnTime()
	{
		return Random.Range(minSpawnTime, maxSpawnTime);
	}

	public float RandomSpeed()
	{
		return Random.Range(minSpeed, maxSpeed);
	}
}

public class Spawner : MonoBehaviour
{
	public static Dictionary<string, Spawner> Instance = new Dictionary<string, Spawner>();
	[SerializeField] private string spawnerName;
	[SerializeField] private SpawnData[] spawners = null;

	private void Awake()
	{
		Instance.Add(spawnerName, this);
	}

	private void OnDestroy()
	{
		Instance.Remove(spawnerName);
	}

	private void Start()
	{
		for(int i = 0; i < spawners.Length; i++)
		{
			spawners[i].timerMax = spawners[i].RandomSpawnTime();
		}
	}

	void Update()
    {
		for (int i = 0; i < spawners.Length; i++)
		{
			spawners[i].timer += Time.deltaTime;
			while (spawners[i].timer >= spawners[i].timerMax)
			{
				spawners[i].timer -= spawners[i].timerMax;
				spawners[i].timerMax = spawners[i].RandomSpawnTime();
				int randomIndex = Random.Range(0, spawners[i].spawnPoints.Length);
				Rigidbody2D newObject = Instantiate(spawners[i].gameObject, spawners[i].spawnPoints[randomIndex].position, spawners[i].spawnPoints[randomIndex].rotation);
				newObject.AddForce(newObject.transform.up * spawners[i].RandomSpeed(), ForceMode2D.Impulse);
			}
		}
    }
}
