﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
	private GameObject tgt = GameObject.Find ("summonBlue");
	private Vector3 tgtDis;

	public int HP = 200;
	private Vector3 myPos = transform.position;
	private Vector3 savePos;
	private float detourDis;
	private static int speed = 10;
	public string state = "move";
	private bool measureSpace = true;
	public GameObject frontAlly = null;
	public static List<GameObject> allys = new List<GameObject> ();
	public List<GameObject> atEnemys = new List<GameObject> ();
	private float frontdis = 7.0f;
	private GameObject saveFrontAlly;
	private bool  right = Random.value > 0.5f ? true : false;
	public enemyMove script;
	private bool attackSpace = true;
	private static int count = 0;

	void Start ()
	{
		count++;
		gameObject.name = "RedSoldier" + count;
		tgtDis = distance (tgt.transform.position, myPos);
	}
	
	// Update is called once per frame
	void Update ()
	{
		switch (state) {
		case "move":
			myPos.z = myPos.z + (tgtDis.z * Time.deltaTime) / speed;
			if (myPos.x >= 1 || myPos.x <= -1) {
				myPos.x = myPos.x - (tgtDis.x * Time.deltaTime) / (speed * (myPos.z / tgtDis.z));
			}
			transform.position = myPos;
			break;
		case "detour":
			detour ();
			break;
		case "fight":
			if (attackSpace) {
				StartCoroutine (attack ()); 
			}
			break;
		default :
			break;
		}
		if (HP < 0) {
			death ();
		}
		
	}

	void OnCollisionEnter (Collision col)
	{
		switch (col.gameObject.tag) {
		case "Enemy":
			if (gameObject.CompareTag ("Player") && state != "fight") {
				script = col.gameObject.GetComponent<enemyMove> ();
				if (script.atEnemys.Count < 3) {
					script.atEnemys.Add (gameObject);
					state = "fight";
					gameObject.tag = "StopPlayer";
				}
			}
			break;
		case "StopEnemy":
			if (gameObject.CompareTag ("Player") && state != "fight") {
				script = col.gameObject.GetComponent<enemyMove> ();
				if (script.atEnemys.Count < 3) {
					script.atEnemys.Add (gameObject);
					state = "fight";
					gameObject.tag = "StopPlayer";
				}
			}
			break;
		case "summonBlue":
			state = "fight";
			gameObject.tag = "StopPlayer";
			break;
		default :
			break;
		}
	}

	public void detourReady ()
	{
		if (saveFrontAlly != frontAlly) { 
			saveFrontAlly = frontAlly;
			detourDis = right ? frontAlly.transform.localScale.x : -frontAlly.transform.localScale.x;
			state = "detour";
			savePos = myPos;
		}
	}

	public static Vector3 distance (Vector3 target, Vector3 me)
	{
		Vector3 dis = target - me;
		return dis;
	}

	private IEnumerator attack ()
	{
		script.HP -= 30;
		attackSpace = false;
		yield return new WaitForSeconds (3);
		attackSpace = true;
	}

	private void death ()
	{
		foreach (GameObject enemy in atEnemys) {
			script = enemy.GetComponent<enemyMove> ();
			script.atEnemys.Remove (gameObject);
			script.state = "move";
			enemy.tag = "Enemy";
		}
		summonsServant.sp += 10;
		Destroy (gameObject);

	}
}
