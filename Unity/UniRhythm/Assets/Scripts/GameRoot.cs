using UnityEngine;
using System.Collections;
using System.Collections.Generic;	//List型を扱うための宣言

public class GameRoot : MonoBehaviour {

	//プレイヤー制御用
	private PlayerControl player = null;

	//敵制御用
	private EnemyControl enemy = null;

	//敵プレハブ
	public GameObject enemyPF = null;

	//敵ウェーブの設定
	public Vector3 enemyPosition = new Vector3(5.0f,0.5f,0.0f);
	public int enemyCount = 10;	//1ウェーブで何匹出すか
	public float enemyWait = 1.0f;	//敵1匹ごとの間隔
	public float startWait = 2.0f;	//ゲームスタートから生成開始まで
	public float waveWait = 3.0f;	//敵ウェーブの間隔

	//BGM再生用
	private AudioSource audiosrc;
	
	//ゲーム全体の経過時間
	public static float gameTimer = 0.0f;

	//------------------------------------
	//デバッグ用

	//リズムカウントサウンド
	//1分間に何拍たたきたいか
	public float soundBPM = 60.0f;
	//何拍子か
	public int soundTime  = 4;
	//何秒ごとに拍子がたたかれるか(Startで初期化)
	private float soundCountInterval = 0.0f;
	
	//サウンドソース
	public AudioClip SEbeatNormal01;	//通常の拍子音
	public AudioClip SEbeatTime01;		//小節の区切り音

	//サウンド用の経過時間カウンタ
	//リセットすることがあるので、ゲーム全体のカウンタとは別に用意している
	private float soundTimer = 0.0f;
	//拍子を何回叩いたか
	//区切りの音を鳴らしたらリセットする
	private int soundBeatCounter = 0;


	//#################
	//	Start
	//#################
	void Start () {
		//プレイヤー制御の準備
		this.player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();

		//敵生成のコルーチン処理を開始する
		StartCoroutine ( enemyWaves () );

		//効果音再生の準備
		audiosrc = GetComponent<AudioSource>();
		//何秒間隔でリズム音を鳴らすか
		soundCountInterval = 60 / soundBPM;

	}
	
	//#################
	//	Update
	//#################
	void Update () {
		//ゲーム全体の経過時間を更新
		gameTimer += Time.deltaTime;

	}

	//#################
	//	FixedUpdate
	//#################
	void FixedUpdate(){
		//リズム再生用のゲーム全体の経過時間を更新
		soundTimer += Time.deltaTime;

		if(soundTimer > soundCountInterval){
			soundBeatCounter += 1;

			if(soundBeatCounter == 1){	//区切り音
				audiosrc.PlayOneShot(SEbeatTime01);
				//GameRoot.log("playTime : " + GameRoot.gameTimer);
				//GameRoot.log ("Play Time beat");
			} else if(soundBeatCounter >= soundTime){	//通常の拍子(末尾)
				soundBeatCounter = 0;
				audiosrc.PlayOneShot(SEbeatNormal01);
				//GameRoot.log("playTime : " + GameRoot.gameTimer);
				//GameRoot.log ("Play Last Normal beat");
			} else {	//通常の拍子
				audiosrc.PlayOneShot(SEbeatNormal01);
				//GameRoot.log("playTime : " + GameRoot.gameTimer);
				//GameRoot.log ("Play Normal beat");
			}

			soundTimer = 0.0f;
		}

	}

	//####################//
	//	敵を生成
	//####################//
	IEnumerator enemyWaves ()
	{

		//ゲーム開始から少しだけ処理を中断(いきなり始まるとプレイヤーが反応しきれない)
		yield return new WaitForSeconds (startWait);
		
		while(true)
		{
			for ( int i = 0; i < enemyCount; i++ )
			{
				//プレイヤーの位置を取得して、敵をその少し先に出す
				//enemyPosition.x = player.transform.position.x + Random.Range (10,20);

				Vector3 spawnPosition = enemyPosition;

				Quaternion spawnRotation = new Quaternion(0.0f, 0.7f, 0.0f, -0.7f);
				//Quaternion spawnRotation = Quaternion.Euler (0,270,0);
				//GameRoot.log ("spawnRoatation Quaternion : " + spawnRotation);
				Instantiate ( enemyPF, spawnPosition, spawnRotation );
				//ここで一旦処理を中断
				yield return new WaitForSeconds (enemyWait);
			}
			//ここで一旦処理を中断
			yield return new WaitForSeconds (waveWait);
		}
	}
	//####################//
	//ログを画面上に出力
	//####################//
	void OnGUI(){
		Rect rect = new Rect(5,5,400,50);
		
		GUIStyle style = new GUIStyle();
		style.fontSize = 10;
		style.fontStyle = FontStyle.Normal;
		style.normal.textColor = Color.black;
		
		string outMessage = "";
		foreach(string msg in logMsg){
			outMessage += msg + System.Environment.NewLine;
		}
		
		GUI.Label (rect, outMessage, style);
	}
	
	//====================//
	//	ログの記録
	//====================//
	private static List<string> logMsg = new List<string>();
	public static void log(string msg){
		logMsg.Add (msg);
		//直近5件だけ保存
		if(logMsg.Count > 5){
			logMsg.RemoveAt (0);
		}
		
		//コンソールにも表示
		print(msg);
	}
}
