using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	
	//アニメーション制御用
	private Animator animator;
	private int stateIdle, stateAttack, stateDamaged;
	
	//物理法則制御用
	private Rigidbody rgbody;

	//効果音再生用
	private AudioSource audiosrc;

	//攻撃判定タイマー
	//攻撃発生時にATTACK_TIMEを代入し、
	//毎フレーム経過時間を差し引いていく。
	//このタイマーが0未満になったら攻撃判定を消滅させる。
	private float attackTimer = 0.0f;
	
	//攻撃判定が継続する時間(sec)
	public float ATTACK_TIME = 0.3f;

	//プレイヤーの状態を表す列挙体データ
	public enum STEP {
		NONE = -1,
		IDLE = 0,
		JUMP,
		ATTACK,
		DAMAGED,
		NUM,
	};
	
	//現ステップ：このフレームでのプレイヤーの状態
	public STEP step = STEP.NONE;
	//次ステップ：次のフレームでのプレイヤーの状態
	public STEP nextStep = STEP.NONE;

	//----------------------------------------------
	//サウンド

	//攻撃ボイス
	public AudioClip VCattack01;
	public AudioClip VCattack02;
	//ダメージボイス
	public AudioClip VCdamaged01;
	public AudioClip VCdamaged02;
	//ランダムでボイスを出すための一時変数
	private AudioClip vc;

	//#################
	//	Start
	//#################
	void Start () {

		//アニメーション制御の準備
		animator = this.GetComponent<Animator> ();
		//アニメーションステート名のハッシュ値を取得
		stateIdle = Animator.StringToHash("Base Layer.Idle");
		stateAttack = Animator.StringToHash("Base Layer.Attack");
		stateDamaged = Animator.StringToHash("Base Layer.Damaged");

		//物理法則制御の準備
		rgbody = this.GetComponent<Rigidbody>();

		//効果音再生の準備
		audiosrc = GetComponent<AudioSource>();
		
		//攻撃判定オブジェクトを非アクティブにする
		transform.Find ("AttackRange").gameObject.SetActive(false);
		
		//次ステップを「待機」に設定
		this.nextStep = STEP.IDLE;

	}
	
	//#################
	//	Update
	//#################
	void Update () {

		//現在のアニメーション名を取得
		AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

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
				//攻撃ボタンが押されたら、次のステップ->ATTACK
				if(Input.GetMouseButtonDown(0)){
					this.nextStep = STEP.ATTACK;
					GameRoot.log("playTime : " + GameRoot.gameTimer);
					GameRoot.log("nextStep -> ATTACK");
				}
				break;
			case STEP.ATTACK:
				//攻撃判定有効時間が終わっていたら、次のステップ->IDLE
				if(attackTimer < 0.0f){
					this.nextStep = STEP.IDLE;
					GameRoot.log("playTime : " + GameRoot.gameTimer);
					GameRoot.log("ATTACK END");
				}
				break;
			case STEP.DAMAGED:
				//現在のアニメーションが「待機」になっていたら、次のステップ->IDLE
				if(animStateInfo.fullPathHash == stateIdle){
					this.nextStep = STEP.IDLE;
					GameRoot.log("playTime : " + GameRoot.gameTimer);
					GameRoot.log("nextStep -> IDLE");
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
			//一旦すべてのアニメーションフラグをfalseにする
			animator.SetBool("Idle", false);
			animator.SetBool("Attack", false);
			animator.SetBool("Damaged", false);
			
			switch(this.step){
			case STEP.IDLE:
				//攻撃判定オブジェクトを非アクティブにする
				//DAMAGEDから遷移してくる時は必要ない処理なのだが…
				transform.Find ("AttackRange").gameObject.SetActive(false);
				animator.SetBool ("Idle", true);
				break;
			case STEP.ATTACK:
				animator.SetBool("Attack", true);
				//攻撃判定オブジェクトをアクティブにする
				transform.Find ("AttackRange").gameObject.SetActive(true);
				//攻撃判定発生タイマーを起動
				attackTimer = ATTACK_TIME;

				//ボイス
				vc = (Random.value < 0.5)? VCattack01 : VCattack02;
				audiosrc.PlayOneShot(vc);
				
				GameRoot.log("ATTACK START!");
				break;
			case STEP.DAMAGED:
				animator.SetBool ("Damaged", true);
				//ボイス
				vc = (Random.value < 0.5)? VCdamaged01 : VCdamaged02;
				audiosrc.PlayOneShot(vc);
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
		case STEP.ATTACK:
			//攻撃判定発生タイマーをカウントダウン
			attackTimer -= Time.deltaTime;
			break;
		case STEP.DAMAGED:
			break;
		default:
			break;
		}

	}

	//#################
	//	敵との衝突処理
	//#################
	//OncollisionEnter()はFixedUpdate()より後、Update()より前に実行される
	void OnCollisionEnter(Collision collis){
		if(collis.gameObject.CompareTag("Enemy")){
			this.nextStep = STEP.DAMAGED;
			GameRoot.log("playTime : " + GameRoot.gameTimer);
			GameRoot.log("nextStep -> DAMAGED");
		}
	}

}
