using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeSpawner : MonoBehaviour
{
	Dictionary<int, Blade> blades = new Dictionary<int, Blade>();

	Blade mouseBlade;

	void Update()
    {
		if(Input.GetMouseButtonDown(0))
		{
			mouseBlade = Instantiate(GameAssets.i.blade, Utilities.GetWorldPosition(Input.mousePosition, -5), Quaternion.identity);
		}
		if(Input.GetMouseButton(0))
		{
			mouseBlade.SetPosition(Utilities.GetWorldPosition(Input.mousePosition, -5));
		}
		if(Input.GetMouseButtonUp(0))
		{
			Destroy(mouseBlade.gameObject, 2);
			mouseBlade = null;
		}
		for(int i = 0; i < Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			if (!blades.ContainsKey(touch.fingerId))
			{
				blades.Add(touch.fingerId, Instantiate(GameAssets.i.blade, Utilities.GetWorldPosition(touch.position), Quaternion.identity));
			}
			else
			{
				if(touch.phase == TouchPhase.Ended)
				{
					Destroy(blades[touch.fingerId].gameObject, 2);
					blades.Remove(touch.fingerId);
				}
				else
				{
					blades[touch.fingerId].transform.position = Utilities.GetWorldPosition(touch.position);
				}
			}
		}
    }
}
