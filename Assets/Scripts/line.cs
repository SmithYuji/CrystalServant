﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;

public class line : MonoBehaviour {
	private GameObject lineImage = (GameObject)Resources.Load("lineImage");
	public List<GameObject> lineob = new List<GameObject> ();
	private Vector3 p1;
	public bool mouseLine;

	public bool enable{
		set{
			if (value == false) {
				line script = gameObject.GetComponent<line> ();
				lineob.ForEach (i => Destroy (i));
				line.Destroy (script);
			} else {

			}
		}
	}
	public Color color{
		set{
			for(int i = -1; i < lineob.Count;i++){
				lineob[i].GetComponent<SpriteRenderer> ().color = value;
			}
		}
		get{
			return lineob[0].GetComponent<SpriteRenderer> ().color;
		}
	}
	/// <summary>
	/// Lineパラメーター初期設定
	/// </summary>
	/// <param name="i">The index.</param>
	/// <param name="c">C.</param>
	private void lineCons(int i,Color c){
		p1 = transform.position;
		lineob.Add((GameObject)Instantiate (lineImage,new Vector3(p1.x,0.1f,p1.z),Quaternion.identity));
		lineob[i].GetComponent<SpriteRenderer> ().color = c;
		lineob[i].transform.Rotate (90,0,0);

	}
	/// <summary>
	/// Line初期設定　兵士用
	/// </summary>
	/// <param name="c">C.</param>
	/// <param name="obs">Obs.</param>
	public void setup(Color c,GameObject tgt,List<GameObject> obs){
		obs.Add (tgt);
		for(int a = 0;a<obs.Count;a++){
			lineCons (a,c);
		}
		StartCoroutine(ObLine (obs));
	}
	/// <summary>
	/// Line初期設定　マウス
	/// </summary>
	/// <param name="c">C.</param>
	/// <param name="mouse">If set to <c>true</c> mouse.</param>
	public void setup(Color c,bool area,GameObject ob){
		lineCons (0,c);
		StartCoroutine(RayLine (area,ob));
	}
	/// <summary>
	/// Line初期設定　範囲指定後用
	/// </summary>
	/// <param name="c">C.</param>
	/// <param name="pos">Position.</param>
	public void setup(Color c,Vector3 pos){
		lineCons (0,c);
		StartCoroutine(dropLine (pos));

	}
	/// <summary>
	/// マウスの座標にLine表示する
	/// 引数はtrueで範囲移動時にライトアップさせない
	/// </summary>
	/// <param name="area">If set to <c>true</c> area.</param>
	private IEnumerator RayLine(bool area,GameObject ob){//引数は範囲移動時は攻撃対象をライトアップさせないため
		int terrain = 1 << LayerMask.NameToLayer ("Terrain");
		while (true) {
			p1 = ob.transform.position;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit,Mathf.Infinity,terrain)) {
				StartCoroutine(LineParameter (hit.point,p1,0));
			}
			int mask = (1 << LayerMask.NameToLayer ("touchChara"));
			if (Physics.Raycast (ray, out hit, Mathf.Infinity, mask) && area == false) {//rayがキャラクターに当たる
				try {
					if (Instruction.saveChara.layer == 10 && hit.collider.gameObject.transform.parent.gameObject.layer == 9
						|| Instruction.saveChara.layer == 9 && hit.collider.gameObject.transform.parent.gameObject.layer == 10) {
						//rayが当たったキャラと以前当たったキャラが敵対していたら
						GameObject Hetgt;//rayが当たっとキャラクターのターゲット
						Hetgt = hit.collider.gameObject.transform.parent.gameObject.GetComponent<Soldier> ().tgt;
						if (Hetgt != Instruction.saveChara) {
							hit.collider.gameObject.transform.parent.gameObject.GetComponent<Light> ().enabled = true;//当たったキャラのライトをON
							hit.collider.gameObject.transform.parent.gameObject.GetComponent<Light> ().color = Color.yellow;//黄色く光らせる
							Instruction.rayobj = hit.collider.gameObject.transform.parent.gameObject;//rayMouse内での判定用に保存
						}
					}
				} catch {
				}
			}
			yield return null;
		}
	}
	/// <summary>
	/// 複数のラインを生成
	/// </summary>
	/// <param name="obs">Obs.</param>
	private IEnumerator ObLine(List<GameObject> obs){//キャラクターへのLine 引数は対象リスト
		while (true) {
			p1 = transform.position;
			int i = 0;
			foreach(GameObject ob in obs){
				try{
					Vector3 p0 = ob.transform.position;
					StartCoroutine( LineParameter (p0,p1,i));
					//Debug.Log(p0);
				}catch{
					obs.Remove (ob);
					lineob.RemoveAt(i);
				}
				i++;
			}
			i = 0;
			yield return null;
		}
	}
	/// <summary>
	/// emptyから固定座標に向けてLineを出す。emptyのリストが空になるか固定座標にたどり着くかで消える。
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="p0">P0.</param>
	public IEnumerator dropLine(Vector3 p0){
		p1 = transform.position;
		try{
			foreach(GameObject ob in Empty.emptys){
				ob.GetComponent<Empty> ().listSerch (gameObject.GetComponent<Empty>().allys);
			}
		}catch{
		}
		name = "moveEmpty";
		Empty.emptys.Add (gameObject);
		while(Mathf.Abs(p1.x-p0.x) > 0.5f && Mathf.Abs(p1.z-p0.z) > 0.5f){
			p1 = transform.position;
			StartCoroutine(LineParameter (p0,p1,0));
			yield return null;
		}//目的地に着くとwhileを抜ける
		Empty.emptys.Remove (gameObject);//staticリストから自分を外す
		Destroy (gameObject);//自殺
		yield return null;
	}

	/// <summary>
	/// Lineの動的変化メソッド
	/// 引数は原点座標と対象座標
	/// </summary>
	/// <param name="p0">P0.</param>
	/// <param name="p1">P1.</param>
	public IEnumerator LineParameter(Vector3 p0,Vector3 p1,int i){
		float d = Vector3.Distance (p0,p1);
		float atan =  Mathf.Atan2 ((p0.x-p1.x),(p0.z - p1.z))*Mathf.Rad2Deg;
		lineob[i].transform.rotation = Quaternion.Euler (90, atan, 0);
		lineob[i].transform.localScale = new Vector3 (200f,d*100,0);
		lineob[i].transform.position = new Vector3 ((p0.x+p1.x)/2,0.1f,(p0.z+p1.z)/2);
		yield return null;
	}
	void OnDestroy(){

	}
}
