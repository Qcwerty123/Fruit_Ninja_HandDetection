using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeBanana : BaseFruit
{
	public override void Slice(float velocity, float cutRotation)
	{
		BananaBonus.Instance.Add(Bonus.Freeze, 3);
		base.Slice(velocity, cutRotation);
	}
}
