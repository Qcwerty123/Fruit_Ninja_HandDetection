using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
	public static GameTimer Instance;

	[SerializeField] int totalTimeInSeconds = 90;

	float timeLeft;
	TextMeshProUGUI timerText;

	private void Awake()
	{
		Instance = this;
		timerText = GetComponent<TextMeshProUGUI>();
		timeLeft=totalTimeInSeconds;
		UpdateTime(Mathf.CeilToInt(timeLeft));
	}

	private float critChance;

	private void Start()
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Zen")
		{
			critChance = Fruit.criticalChance;
			Fruit.criticalChance = 0;
		}
	}

	private void OnDestroy()
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Zen")
		{
			Fruit.criticalChance = critChance;
		}
	}

	void Update()
    {
		timeLeft -= Time.deltaTime;
		UpdateTime(Mathf.Max(0,Mathf.CeilToInt(timeLeft)));
		if (timeLeft <= 0)
		{
			if(Fruit.Instances.Count == 0)
			{
				SceneLoader.Instance.LoadScene("MainMenu");
			}
		}
    }

	void UpdateTime(int _time)
	{
		int timeLeftInSeconds = _time % 60;
		timerText.text = (_time / 60) + ":" + (timeLeftInSeconds < 10 ? "0" + timeLeftInSeconds.ToString() : timeLeftInSeconds.ToString());
		if (_time <= 0)
		{
			timeLeftInSeconds = 0;
			timerText.text = (_time / 60) + ":" + (timeLeftInSeconds < 10 ? "0" + timeLeftInSeconds.ToString() : timeLeftInSeconds.ToString());
			foreach (var spawner in Spawner.Instance.Values)
			{
				spawner.enabled = false;
			}
		}
	}
}
