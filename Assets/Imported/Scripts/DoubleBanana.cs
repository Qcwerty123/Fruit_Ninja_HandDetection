using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleBanana : BaseFruit
{
	public override void Slice(float velocity, float cutRotation)
	{
		BananaBonus.Instance.Add(Bonus.Double, 6);
		base.Slice(velocity, cutRotation);
	}
}
