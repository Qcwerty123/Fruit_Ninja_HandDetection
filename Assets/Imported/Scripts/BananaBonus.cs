using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bonus
{
	public static readonly Bonus Frenzy = new Bonus(
	() =>
	{
		Spawner.Instance["Frenzy"].enabled = true;
	},
	() =>
	{
		Spawner.Instance["Frenzy"].enabled = false;
	});

	public static readonly Bonus Freeze = new Bonus(
	() =>
	{
		Time.timeScale = 0.5f;
		GameTimer.Instance.enabled = false;
	},
	() =>
	{
		Time.timeScale = 1;
		GameTimer.Instance.enabled = true;
	});

	public static readonly Bonus Double = new Bonus(
	() =>
	{
		Score.Instance.OnScoreChanged += Score_OnScoreChanged;
	},
	() =>
	{
		Score.Instance.OnScoreChanged -= Score_OnScoreChanged;
	});

	private static bool added = false;
	private static void Score_OnScoreChanged(int obj)
	{
		if (!added)
		{
			added = true;
			Score.Instance.AddPoints(obj);
		}
		else
		{
			added = false;
		}
	}

	protected Bonus(Action activate, Action deactivate)
	{
		this.activate = activate;
		this.deactivate = deactivate;
	}

	private bool active = false;
	private readonly Action activate;
	private readonly Action deactivate;

	public void Activate()
	{
		if(!active)
		{
			active = true;
			activate?.Invoke();
		}
	}

	public void Deactivate()
	{
		if(active)
		{
			active = false;
			deactivate?.Invoke();
		}
	}
}

public class BananaBonus : MonoBehaviour
{
	public static event Action<Bonus> OnBonusActivated;
	public static BananaBonus Instance;

	private void Awake()
	{
		Instance = this;
	}

	public void RemoveAll()
	{
		Bonus.Double.Deactivate();
		Bonus.Frenzy.Deactivate();
		Bonus.Freeze.Deactivate();
	}

	public void Add(Bonus bonus, float time)
	{
		bonus.Activate();
		FunctionTimer.Create(bonus.Deactivate, time);
		OnBonusActivated?.Invoke(bonus);
	}
}
