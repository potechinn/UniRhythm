﻿using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour {
	//アニメーション制御用
	private Animation anime;

	//物理法則制御用
	private Rigidbody rgbody;

	//プレイヤーの状態を表す列挙体データ
	public enum STEP {
		NONE = -1,
		IDLE = 0,
		RUN,
		JUMP,
		ATTACK,	//プレイヤーに接触した時(プレイヤーの攻撃間に合わず)
		ATTACK_END,	//ATTACKの後消える時
		DAMAGED,
		NUM,
	};

	//現ステップ：このフレームでのプレイヤーの状態
	public STEP step = STEP.NONE;
	//次ステップ：次のフレームでのプレイヤーの状態
	public STEP nextStep = STEP.NONE;

	//移動速度
	public float SPEED = 1.0f;

	//プレイヤーの攻撃を食らった時
	//ふっとぶ勢い(X方向)
	public float BLOWOFF_SPEED = 8.5f;
	//ふっとぶ高さ(Y方向)
	public float BLOWOFF_HEIGHT = 2.0f;

	//プレイヤーと接触した時
	//接触してから消えるまで(ATTACKからATTACK_ENDまで)の長さ
	public float ATTACK_END_TIME = 3.0f;

	//接触してから消えるまでのタイマー
	//攻撃発生時にATTACK_END_TIMEを代入し、
	//毎フレーム経過時間を差し引いていく。
	//このタイマーが0未満になったら自分を消滅させる。
	private float attackEndTimer = 0.0f;


	//#################
	//	Start
	//#################
	void Start () {
		//アニメーション制御の準備
		anime = this.GetComponent<Animation> ();
		//物理法則制御の準備
		rgbody = this.GetComponent<Rigidbody>();

		//次ステップを「走る」に設定
		this.nextStep = STEP.RUN;
	}
	

	//#################
	//	Update
	//#################
	void Update () {

		Vector3 velocity = this.rgbody.velocity;

		//-------------------------//
		//	状態の切り替えチェック
		//-------------------------//
		//このUpdateまでに次ステップが決まっていなければ
		//次ステップの状態を現ステップと変えるべきかどうかチェックする
		//ジャンプが押されたか、着地しているか、などなど
		//特に状態を変えなくてよい場合、nextStepはNONEのままになる
		if(this.nextStep == STEP.NONE){
			switch(this.step){
			case STEP.IDLE:
				break;
			case STEP.RUN:
				break;
			case STEP.ATTACK:	//Playerと接触した時<-OnCollisionEnter
				if(attackEndTimer < 0.0f){
					this.nextStep = STEP.ATTACK_END;
				}
				break;
			case STEP.DAMAGED:	//AttackRangeと接触した時<-OnTriggerEnter
				break;
			default:
				break;
			}
		}

		//=========================//
		//	状態変化時の初期処理
		//=========================//
		//次ステップが決まっている場合、それは状態が変化するということなので
		//その際に必要な処理を実行
		//状態が変化しない場合はこの中には入らない
		//whileを使っているので、nextStepがNONEにならない限り、繰り返し処理し続ける
		while(this.nextStep != STEP.NONE){
			this.step = this.nextStep;
			this.nextStep = STEP.NONE;

			switch(this.step){
			case STEP.IDLE:
				break;
			case STEP.RUN:
				anime.Play ("idle");
				break;
			case STEP.ATTACK:
				//ATTACK_ENDまでのカウントダウンタイマー
				attackEndTimer = ATTACK_END_TIME;

				velocity.x = 0.0f;
				anime.Play ("special");
				break;
			case STEP.ATTACK_END:
				break;
			case STEP.DAMAGED:
				//ふっとび
				velocity.x = BLOWOFF_SPEED;	//ふっとばされる勢い
				velocity.y = Mathf.Sqrt (2.0f*9.8f*BLOWOFF_HEIGHT);
				velocity.z = 0.0f;

				//やられモーション
				anime.Play ("wound");
				break;
			default:
				break;
			}
		}

		//=========================//
		//	毎フレームの処理
		//=========================//
		//現ステップの状態に応じた各種処理
		//移動速度の調整など
		switch(this.step){
		case STEP.IDLE:
			break;
		case STEP.RUN:
			velocity.x = -1 * SPEED;
			break;
		case STEP.ATTACK:
			//ATTACK_ENDまでのタイマーをカウントダウン
			attackEndTimer -= Time.deltaTime;
			break;
		case STEP.ATTACK_END:
			//消える
			GameRoot.log("playTime : " + GameRoot.gameTimer);
			GameRoot.log("Enemy -> Destroy");

			Destroy(this.gameObject);
			break;
		case STEP.DAMAGED:
			break;
		default:
			break;
		}

		this.rgbody.velocity = velocity;
	}

	//#################
	//	プレイヤーの攻撃にあたった時
	//#################
	void OnTriggerEnter(Collider collis){
		if(collis.CompareTag("AttackRange")){
			this.nextStep = STEP.DAMAGED;
			GameRoot.log("playTime : " + GameRoot.gameTimer);
			GameRoot.log("Enemy -> DAMAGED");
		}
	}

	//#################
	//	プレイヤーにあたった時
	//#################
	void OnCollisionEnter(Collision collis){
		if(collis.gameObject.CompareTag("Player")){
			this.nextStep = STEP.ATTACK;
			GameRoot.log("playTime : " + GameRoot.gameTimer);
			GameRoot.log("Enemy -> ATTACK");
		}
	}

}
