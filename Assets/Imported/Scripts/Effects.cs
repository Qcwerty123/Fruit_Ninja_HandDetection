using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
	// private void OnEnable()
	// {
	// 	Fruit.OnFruitSliced += Fruit_OnFruitSliced;
	// 	Fruit.OnFruitCritical += Fruit_OnFruitCritical;
	// 	ComboSystem.OnCombo += ComboSystem_OnCombo;
	// }

	// private void OnDisable()
	// {
	// 	Fruit.OnFruitSliced -= Fruit_OnFruitSliced;
	// 	Fruit.OnFruitCritical -= Fruit_OnFruitCritical;
	// 	ComboSystem.OnCombo -= ComboSystem_OnCombo;
	// }

	// private void Fruit_OnFruitSliced(Fruitdata fruit, SlicedFruit sliced, float velocity, float rotation)
	// {
	// 	if (Random.Range(0, 100) <= 90)
	// 	{
	// 		BubbleEffects.CreateSmall(sliced.transform.position, rotation, fruit.Color());
	// 	}
	// 	if(Random.Range(0, 100) <= 60)
	// 	{
	// 		BubbleEffects.CreateBig(sliced.transform.position, rotation, fruit.Color());
	// 	}
	// 	CutEffect.Create(sliced.transform.position, rotation);
	// 	AudioManager.PlayRandomSplash();
	// 	if (Random.Range(0, 100) <= 90)
	// 	{
	// 		float max = Mathf.Min(1.6f, 1 + velocity / 300f);
	// 		float min = Mathf.Min(max, 1 + velocity / 320f);
	// 		SplashEffects.CreateSplash(sliced.transform.position, 1, 1, 0.1f, 1 + velocity / 320f, 1 + velocity / 300f, fruit.Color());
	// 		if (velocity >= 40 && Random.Range(1, 100) <= 50)
	// 		{
	// 			max = Mathf.Min(5, 2 + velocity / 49f);
	// 			min = Mathf.Min(max, 2 + velocity / 51f);
	// 			SplashEffects.CreateForwardSplash(sliced.transform.position, rotation, min, max, fruit.Color());
	// 		}
	// 	}
	// }

	// private void Fruit_OnFruitCritical(Fruitdata fruit, SlicedFruit sliced, float velocity, float rotation)
	// {
	// 	AudioManager.PlayRandomSplash();
	// 	SplashEffects.CreateSplash(sliced.transform.position, 1, 20, 0.02f, .8f, 1.2f, fruit.Color(), sliced.PartOne(), 0.5f, 1);
	// 	SplashEffects.CreateSplash(sliced.transform.position, 1, 20, 0.02f, .8f, 1.2f, fruit.Color(), sliced.PartTwo(), 0.5f, 1);
	// 	CriticalText.Create(sliced.transform.position, Score.POINTS_PER_CRITICAL);
	// }

	// private void ComboSystem_OnCombo(Vector3 comboPosition, List<Fruitdata> comboData)
	// {
	// 	SplashEffects.CreateSplash(comboPosition, (short)(comboData.Count + 4), 1, 0, .8f, 1.2f, comboData[comboData.Count - 1].Color(), null, 1f, 1);
	// }
}
