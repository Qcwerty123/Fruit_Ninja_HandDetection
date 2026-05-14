using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	public static SceneLoader Instance;

	private void Awake()
	{
		Instance = this;
	}

	public void LoadScene(int index)
	{
		var sceneLoading = SceneManager.LoadSceneAsync(index);
		//sceneLoading.allowSceneActivation = true;
	}

	public void LoadScene(string scene)
	{
		var sceneLoading = SceneManager.LoadSceneAsync(scene);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
