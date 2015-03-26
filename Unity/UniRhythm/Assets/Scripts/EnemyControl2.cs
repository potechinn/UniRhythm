using UnityEngine;
using System.Collections;

public class EnemyControl2 : MonoBehaviour {

	//GameRoot制御用
	private GameRoot gameroot;

	//アニメーション制御用
	private Animator animator;
	
	//物理法則制御用
	private Rigidbody rgbody;
	
	//エネミーの状態を表す列挙体データ
	public enum STEP {
		NONE = -1,
		IDLE = 0,	//画面外で待機
		FRAMEIN,	//画面内に入る
		STANDBY,	//画面内でジャンピング待機
		JUMPTO,		//プレイヤーに飛びかかる
		ATTACK,		//プレイヤーに接触した時(プレイヤーの攻撃間に合わず)
		ATTACK_END,	//ATTACKの後消える時
		DAMAGED,	//プレイヤーの攻撃を受けた
		NUM,
	};
	
	//現ステップ：このフレームでのエネミーの状態
	public STEP step = STEP.NONE;
	//次ステップ：次のフレームでのエネミーの状態
	public STEP nextStep = STEP.NONE;
	
	//移動速度
	public float SPEED = 1.0f;

	//-----------------
	//リズムデータ
	//-----------------
	//何泊目で画面に入ってくるか
	public int TIME_FRAMEIN = 1;
	//何拍目でとびかかるか
	public int TIME_JUMPTO = 3;

	//-----------------
	//画面に入ってくるアニメーション(STEP.FRAMEIN)
	//-----------------
	//入ってくる勢い(X方向)	正の数で指定
	public float FRAMEIN_SPEED = 4.0f;
	//入ってくる高さ(Y方向)
	public float FRAMEIN_HEIGHT = 2.5f;

	//-----------------
	//待機アニメーション(STEP.STANDBY)
	//-----------------
	//ジャンプの高さ(Y方向)
	public float STANDBY_HEIGHT = 2.0f;

	//-----------------
	//とびかかるアニメーション(STEP.JUMPTO)
	//-----------------
	//とびかかる勢い(X方向)	正の数で指定
	public float JUMPTO_SPEED = 5.0f;
	//とびかかる高さ(Y方向)
	public float JUMPTO_HEIGHT = 5.0f;

	//-------------------------
	//プレイヤーの攻撃を食らった時
	//-------------------------
	//ふっとぶ勢い(X方向)	正の数で指定
	public float BLOWOFF_SPEED = 15.0f;
	//ふっとぶ高さ(Y方向)
	public float BLOWOFF_HEIGHT = 8.0f;
	
	//-------------------------
	//プレイヤーと接触した時
	//-------------------------
	//接触してから消えるまで(ATTACKからATTACK_ENDまで)の長さ
	public float ATTACK_END_TIME = 3.0f;
	
	//接触してから消えるまでのタイマー
	//攻撃発生時にATTACK_END_TIMEを代入し、
	//毎フレーム経過時間を差し引いていく。
	//このタイマーが0未満になったら自分を消滅させる。
	private float attackEndTimer = 0.0f;

	private float animSpeed = 1.0f;
	
	
	//#################
	//	Start
	//#################
	void Start () {
		//GameRoot制御用
		gameroot = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameRoot>();

		//アニメーション制御の準備
		animator = this.GetComponent<Animator> ();
		//物理法則制御の準備
		rgbody = this.GetComponent<Rigidbody>();

		/*
		//アニメーション再生速度の設定
		//BGMのBPMが「120」の時、再生速度は「1」
		animSpeed = 1.0f * gameroot.soundBPM / 120.0f;
		//アニメーターのスピードを設定
		//このGameObjectのアニメーションがすべてこのスピードになるので注意
		animator.speed = animSpeed;

		GameRoot.log("playTime : " + GameRoot.gameTimer);
		GameRoot.log("EnemyAnimationSpeed : " + animSpeed);

		*/

		//次ステップを「待機」に設定
		this.nextStep = STEP.IDLE;
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
			case STEP.ATTACK:	//Playerと接触した時<-OnCollisionEnter
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
			
			//一旦すべてのアニメーションフラグをfalseにする
			animator.SetBool("StandBy", false);

			switch(this.step){
			case STEP.DAMAGED:
				rgbody.AddForce(
					1.0f * BLOWOFF_SPEED,
					BLOWOFF_HEIGHT,
					0,
					ForceMode.VelocityChange
					);
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
		default:
			break;
		}
		
		this.rgbody.velocity = velocity;
	}
	
	//#################
	//	FixedUpdate
	//#################
	//BGMのテンポと合わせたい動作はここへ書く
	//1フレーム内で複数回呼ばれることもあるので、
	//インプット探知など、物理演算以外の処理は書かないこと。
	void FixedUpdate(){
		if(GameRoot.soundBeat == true){	//拍を叩くタイミングのみ実行
			//-------------------------//
			//	状態の切り替えチェック
			//-------------------------//
			//このFixedUpdateまでに次ステップが決まっていなければ
			//次ステップの状態を現ステップと変えるべきかどうかチェックする
			//特に状態を変えなくてよい場合、nextStepはNONEのままになる
			if(this.nextStep == STEP.NONE){
				switch(this.step){
				case STEP.IDLE:
					if(GameRoot.soundBeatCounter == TIME_FRAMEIN){
						this.nextStep = STEP.FRAMEIN;
					}
					break;
				case STEP.FRAMEIN:
					this.nextStep = STEP.STANDBY;
					break;
				case STEP.STANDBY:
					if(GameRoot.soundBeatCounter == TIME_JUMPTO){
						this.nextStep = STEP.JUMPTO;
					}
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
				case STEP.FRAMEIN:
					rgbody.AddForce(
						-1.0f * FRAMEIN_SPEED,
						FRAMEIN_HEIGHT,
						0,
						ForceMode.VelocityChange
						);
					GameRoot.log("playTime : " + GameRoot.gameTimer);
					GameRoot.log("Enemy -> FRAME IN");
					GameRoot.log ("Enemy Position : " + this.transform.position);
					break;
				case STEP.JUMPTO:	//プレイヤーへ飛びかかる
					rgbody.AddForce(
						-1.0f * JUMPTO_SPEED,
						JUMPTO_HEIGHT,
						0,
						ForceMode.VelocityChange
						);
					GameRoot.log("playTime : " + GameRoot.gameTimer);
					GameRoot.log("Enemy -> JUMP TO PLAYER");
					break;
				default:
					break;
				}
			}

			//=========================//
			//	毎フレームの処理
			//=========================//
			//現ステップの状態に応じた各種処理
			switch(this.step){
			case STEP.STANDBY:	//待機状態のその場ジャンプ
				rgbody.AddForce(
					Vector3.up * STANDBY_HEIGHT,
					ForceMode.VelocityChange
					);
				GameRoot.log("playTime : " + GameRoot.gameTimer);
				GameRoot.log("Enemy -> STAND BY JUMP");
				break;
			default:
				break;
			}

		}//拍を叩くタイミングのみ実行するifのおわり

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
