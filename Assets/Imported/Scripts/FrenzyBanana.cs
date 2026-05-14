using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrenzyBanana : BaseFruit
{
	public override void Slice(float velocity, float cutRotation)
	{
		BananaBonus.Instance.Add(Bonus.Frenzy, 6);
		base.Slice(velocity, cutRotation);
	}
}
