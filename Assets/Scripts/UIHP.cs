﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIHP : MonoBehaviour {
	public static List<Transform> targets = new List<Transform>();

	void Update () 
	{
		if(targets == null )
			return;
		for (int i = 0; i < targets.Count; i++) {
				targets [i] = GameObject.Find (targets [i].name).transform;
		}
		foreach (Transform tgt in targets) {
			GameObject hp = GameObject.Find (tgt.name +"hp(Clone)");
			Vector3 setPos = new Vector3 (tgt.position.x, tgt.position.y + tgt.localScale.y+5, tgt.position.z);
			hp.transform.position = Camera.main.WorldToScreenPoint (setPos);
		}
	}
}