using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
	public const int POINTS_PER_FRUIT = 1;
	public const int POINTS_PER_CRITICAL = 10;

	public int score { get; private set; } = 0;
	public int highscore { get; private set; } = 0;

	public event Action<int> OnScoreChanged;
	public event Action<int> OnHighscoreChanged;

	public static Score Instance { get; private set; }

	private TextMeshProUGUI scoreText;
	private TextMeshProUGUI highscoreText;
	private Image icon;

	private float originalIconSize;

	string modeName;
	string highscoreString;
	string scoreString;

	private void Awake()
	{
		Instance = this;
		modeName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		highscoreString = modeName + "Highscore";
		scoreString = modeName + "Score";
		highscore = PlayerPrefs.GetInt(highscoreString, 0);
		scoreText = transform.Find("score").GetComponent<TextMeshProUGUI>();
		highscoreText = transform.Find("highscore").GetComponent<TextMeshProUGUI>();
		icon = transform.Find("icon").GetComponent<Image>();
		originalIconSize = icon.rectTransform.localScale.x;
		scoreText.text = "0";
		highscoreText.text = highscore.ToString();
	}

	private void OnDestroy()
	{
		PlayerPrefs.SetInt(highscoreString, highscore);
	}

	private void OnEnable()
	{
		Fruit.OnFruitSliced += Fruit_OnSliced;
		Fruit.OnFruitCritical += Fruit_OnFruitCritical;
	}

	private void OnDisable()
	{
		Fruit.OnFruitSliced -= Fruit_OnSliced;
		Fruit.OnFruitCritical -= Fruit_OnFruitCritical;
	}

	private void Fruit_OnSliced(Fruitdata fruit, SlicedFruit sliced, float velocity, float rotation)
	{
		AddPoints(POINTS_PER_FRUIT);
	}

	private void Fruit_OnFruitCritical(Fruitdata fruit, SlicedFruit sliced, float velocity, float rotation)
	{
		AddPoints(POINTS_PER_CRITICAL);
	}

	public void AddPoints(int points)
	{
		score += points;
		if (score > highscore)
		{
			highscore = score;
			OnHighscoreChanged?.Invoke(highscore);
			ObjectScaler.Add(highscoreText.transform, 1.2f, 3, () => ObjectScaler.Add(highscoreText.transform.transform, 1f, 3, () => ObjectScaler.Remove(highscoreText.transform)));
		}
		OnScoreChanged?.Invoke(points);
		ObjectScaler.Add(icon.transform, 1.2f, 3, () => ObjectScaler.Add(icon.transform, 1f, 3, () => ObjectScaler.Remove(icon.transform)));
		ObjectScaler.Add(scoreText.transform, 1.2f, 3, () => ObjectScaler.Add(scoreText.transform.transform, 1f, 3, () => ObjectScaler.Remove(scoreText.transform)));
		Refresh();
	}

	public void Refresh()
	{
		highscoreText.text = highscore.ToString();
		scoreText.text = score.ToString();
	}
}
