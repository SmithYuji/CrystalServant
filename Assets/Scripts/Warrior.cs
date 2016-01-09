﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior: MonoBehaviour
{
	public GameObject tgt;
	//攻撃先s
	public Vector3 tgtDis;
	//攻撃先までの距離
	public int HP;
	//自分のHP
	public string state;
	//自分の状態
	public GameObject frontAlly = null;
	//前方の味方
	public static List<GameObject> allys = new List<GameObject> ();
	//味方のリスト
	public List<GameObject> atEnemys = new List<GameObject> (), behindAlly = new List<GameObject> ();
	//自分に攻撃してる敵のリスト
	private bool right;
	//迂回時の方向
	public LineRenderer linerende;
	//ラインレンダラー
	public bool lightup = false;
	public GameObject attackObj;
	float radian = 1f, i = 0;


	void Start ()
	{
		linerende = GetComponent<LineRenderer> ();//LineRendererコンポーネントを変数に
		tgt = GameObject.Find (otherName ("tgtName", false));//移動先!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		if (transform.localScale == new Vector3 (6, 6, 6))
			gameObject.name = (otherName ("myName", false)); //名前に味方召喚数の変数を付随させる
		 else if (transform.localScale == new Vector3 (10, 10, 10))
			gameObject.name = (otherName ("myName", false)); //名前に味方召喚数の変数を付随させる
		else if (transform.localScale == new Vector3 (8, 8, 8))
			gameObject.name = (otherName ("myName", false)); //名前に味方召喚数の変数を付随させる
		tgtDis = distance (tgt.transform.position, gameObject.transform.position);//移動先の座標と自分の座標の差分を図り、変数にいれる
		tgtDis = tgtDis.normalized;
		state = "move";//初期状態を移動にする
		HP = 200;//初期体力は200
		right = Random.value > 0.5f ? true : false;//迂回時の左右方向を50%で分けてる
		UIHP.targets.Add (gameObject.transform);//召喚したオブジェクトをHP表示するオブジェクトのリストに入れる

	}

	// Update is called once per frame
	void Update ()
	{
		switch (state) {//自分の状態を要素としswitch文
		case "move"://移動中であれば
			try {
				transform.LookAt (tgt.transform);//移動先に注目
			} catch {
				tgt = GameObject.Find (otherName ("tgtName", false));
			}
			transform.position = new Vector3 (transform.position.x + (tgtDis.x * 10 * Time.deltaTime),
				transform.position.y,
				transform.position.z + (tgtDis.z * 50 * Time.deltaTime));
			break;
		case "detour"://迂回であれば
			detour (10, radian);
			break;
		case "fight"://戦闘中であれば
			try {
				transform.LookAt (tgt.transform);//敵に注目
			} catch {
				tgt = GameObject.Find (otherName ("tgtName", false));
				state = "move";
			}
			break;
		default :
			break;
		}
		if (HP < 0){
			Death ();//HPがゼロになっていたら死亡
		}
		
		if (lightup) {
			linerende.SetPosition (0, transform.position);
			linerende.SetPosition (1, tgt.transform.position);
			try {
				if (tgt.GetComponent<Light> ().enabled == false)
					tgt.GetComponent<Light> ().enabled = true;
				if (attackObj != tgt && tgt.GetComponent<Light> ().color != Color.yellow) {
					tgt.GetComponent<Light> ().color = Color.yellow;
				} else if (attackObj == tgt && tgt.GetComponent<Light> ().color == Color.red && atEnemys.Count == 0) {
					tgt.GetComponent<Light> ().color = Color.blue;
				}
			} catch {
			}
			if (gameObject.GetComponent<Light> ().color != Color.blue)
				gameObject.GetComponent<Light> ().color = Color.blue;
			for (int i = 0; i < atEnemys.Count; i++) {
				if (atEnemys [i].GetComponent<Light> ().enabled == false) {
					atEnemys [i].GetComponent<Light> ().enabled = true;
					atEnemys [i].GetComponent<Light> ().color = Color.red;
					if (atEnemys [i].name == tgt.name)
						atEnemys [i].GetComponent<Light> ().color = Color.blue;
				}
				linerende.SetVertexCount (2 + ((i + 1) * 2));
				linerende.SetPosition (2 + ((i + 1) * 2 - 2), transform.position);
				linerende.SetPosition (2 + ((i + 1) * 2 - 1), atEnemys [i].transform.position);
			}
		}
	}

	public string otherName (string need, bool not)
	{
		
		int mask = LayerMask.NameToLayer ("Enemy");
		string s = "";
		string team = "";
		int teamCount;

		if (gameObject.layer == mask) {
			team = "Red";
			teamCount = EnemyControl.servantCount;
		} else { 
			team = "Blue";
			teamCount = summonsServant.servantCount;
		}

		switch (need) {
		case"tgtName":
			s = team + "FirstCrystal";
			break;
		case"myName":
			if (transform.localScale == new Vector3 (6, 6, 6)) {
				s = team + "Soldier" + teamCount;
			} else if (transform.localScale == new Vector3 (10, 10, 10)) {
				s = team + "Witch" + teamCount;
			} else if (transform.localScale == new Vector3 (8, 8, 8)) {
				s = team + "Guard" + teamCount;
			}
			break;
		case"myTag":
			if (team == "Red") {
				if (!not) {
					s = "Enemy";
				} else {
					s = "Player";
				}
			} else {
				if(!not) {
					s = "Player";
				} else {
					s = "Enemy";
				}
			}
			break;
		case "myStopTag":
			if (team == "Red") {
				s = "StopEnemy";
			} else {
				s = "StopPlayer";
			}
			break;
		case "tgtLayer":
			if (team == "Red")
				s = "BlueCrystal";
			else
				s = "RedCrystal";
			break;
		}
		return s;
	}

	void OnTriggerEnter (Collider col)
	{
		try {
			if (tgt.layer == LayerMask.NameToLayer (otherName ("myTag", true)) && tgt != col.gameObject) {
				return;
			}
		} catch {
			return;
		}
		if (col.gameObject.layer == LayerMask.NameToLayer (otherName ("myTag", true)) && state != "fight") {
			Warrior script = col.gameObject.GetComponent<Warrior> ();
			if (script.atEnemys.Count < 3) {
				script.atEnemys.Add (gameObject);
				tgt = col.gameObject;
				attackObj = tgt;
				state = "fight";
				try {
					behindAlly.ForEach (i => i.GetComponent<Warrior> ().detourReady ());
				} catch {
				}					
				tag = otherName ("myStopTag", false);
				StartCoroutine (attack (30)); //攻撃
			}
		} else if (col.gameObject.layer == LayerMask.NameToLayer (otherName ("tgtLayer", false)) || col.gameObject.layer == 1 << 2 + LayerMask.NameToLayer (otherName ("tgtLayer", false))) {

			if (gameObject.CompareTag (otherName ("myTag", false)) && state != "fight") {
				tgt = col.gameObject;
				attackObj = tgt;
				state = "fight";
				try {
					behindAlly.ForEach (i => i.GetComponent<Warrior> ().detourReady ());
				} catch {
				}				
				tag = otherName ("myStopTag", false);
				StartCoroutine (attack (30)); //攻撃
			}
		}
		if (col.gameObject == tgt) {
			col.gameObject.GetComponent<Light> ().color = Color.blue;
	
		}
	}

	public void detourReady ()
	{
		float detourDis = 0f;	
		try {
			if (right) {
				if (frontAlly.transform.position.x <= gameObject.transform.position.x) {
					detourDis = transform.localScale.x / 2 + (frontAlly.transform.localScale.x / 2 - (transform.position.x - frontAlly.transform.position.x));
				} else {
					detourDis = transform.localScale.x / 2 + (frontAlly.transform.localScale.x / 2 + (transform.position.x - frontAlly.transform.position.x));
				}
			} else {
				if (frontAlly.transform.position.x <= gameObject.transform.position.x) {
					detourDis = -transform.localScale.x / 2 + (-frontAlly.transform.localScale.x / 2 + (transform.position.x - frontAlly.transform.position.x));
				} else {
					detourDis = -transform.localScale.x / 2 + (-frontAlly.transform.localScale.x / 2 - (transform.position.x - frontAlly.transform.position.x));

				}
			}
		} catch {
			return;
		}
		state = "detour";
		var dy = (frontAlly.transform.position.x + detourDis) - gameObject.transform.position.x;
		var dx = frontAlly.transform.position.z - gameObject.transform.position.z;
		radian = Mathf.Atan2 (dy, dx);
		detour (10, radian);//迂回を開始する
	}

	private void detour (byte speed, float radian)
	{
		i += Time.deltaTime;
		var radi = (radian * Mathf.Rad2Deg) * i;
		gameObject.transform.eulerAngles = new Vector3 (0, radi, 0);
		transform.Translate (transform.forward * (tgtDis.z * speed * Time.deltaTime));
		if (i >= 1f) {
			i = 0;
			state = "move";
			gameObject.tag = otherName ("myTag", false);
		}
	}

	public static Vector3 distance (Vector3 target, Vector3 me)
	{
		Vector3 dis = target - me;

		return dis;
	}

	private IEnumerator attack (int power)
	{
		while (state == "fight") {
			try {
				tgt.GetComponent<Warrior> ().HP -= power;
			} catch {
				tgt.GetComponent<crystal> ().HP -= power;
			}
			GameObject myHPBar = GameObject.Find (tgt.name + ("hp(Clone)"));
			myHPBar.transform.localScale -= new Vector3 (0.15f, 0, 0);
			yield return new WaitForSeconds (3);
		}
	}

	private void changeAttack (GameObject obj)
	{
		try {
			if (tgt.GetComponent<Light> ().color == Color.yellow)
				tgt.GetComponent<Light> ().enabled = false;
		} catch {
		}
		if (tgt.layer == 1<<LayerMask.NameToLayer(otherName("myTag",true)))
			tgt.GetComponent<Warrior
> ().atEnemys.Remove (gameObject);
			tgt = obj;
		if (atEnemys.Find (delegate(GameObject ob) {
			return ob.name == tgt.name;
		}))
			attackObj = tgt;
		else {
			state = "move";
			tag = otherName ("myTag", false);
		}
		tgtDis = distance (tgt.transform.position, gameObject.transform.position);
		tgtDis = tgtDis.normalized;
	}

	private void Death ()
	{
		lightup = false;
		if (gameObject == Instruction.rayobj)
			Instruction.rayobj = null;
		if (tgt.layer == 1<<LayerMask.NameToLayer(otherName("myTag",true)))
			tgt.GetComponent<Warrior> ().atEnemys.Remove (gameObject);
		try {
			tgt.GetComponent<LineRenderer > ().SetVertexCount (tgt.GetComponent<Warrior> ().atEnemys.Count + 2);
		} catch {

		}
		Instruction.charaDestroy (gameObject);
		foreach (GameObject enemy in atEnemys) {//自分を狙っている敵
			Warrior script = enemy.GetComponent<Warrior> ();//敵のスクリプトを入手
			if (script.atEnemys.Count == 0) {//敵を狙っている味方がいなければ
				script.state = "move";//敵の状態をmove
				script.tgt = GameObject.Find ("summonBlue");//敵のターゲットを自陣に
				enemy.tag = otherName("myTag",true);//敵のタグを一般に
			} else
				script.tgt = script.atEnemys [0];
		}
		if (Instruction.saveChara == gameObject)
			Instruction.saveChara = null;
		UIHP.targets.Remove (gameObject.transform);
		Destroy (GameObject.Find (gameObject.name + "hp(Clone)"));
		summonsServant.sp += 10;
		Destroy (gameObject);

	}
}