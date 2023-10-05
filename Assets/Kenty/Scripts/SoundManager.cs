using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //必要です！！

public class SoundManager : MonoBehaviour
{

	//ヒエラルキーからD&Dしておく
	public AudioSource BGM_title;
	public AudioSource BGM_main;

	//１つ前のシーン名
	private string beforeScene = "Title";

	// Use this for initialization
	void Start()
	{
		//自分と各BGMオブジェクトをシーン切り替え時も破棄しないようにする
		DontDestroyOnLoad(this.gameObject);
		DontDestroyOnLoad(BGM_title.gameObject);
		DontDestroyOnLoad(BGM_main.gameObject);

		//シーンが切り替わった時に呼ばれるメソッドを登録
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}

	//シーンが切り替わった時に呼ばれるメソッド
	void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
	{
		//シーンがどう変わったかで判定

		//DescriptionからFaceBlendShapesGame
		if (beforeScene == "Description" && nextScene.name == "FaceBlendShapesGame")
		{
			BGM_title.Stop();
			BGM_main.Play();
		}

		//FaceBlendShapesGameからTitleへ
		if ((beforeScene == "FaceBlendShapesGame" && nextScene.name == "Description") ||
			(beforeScene == "FaceBlendShapesGame" && nextScene.name == "Title"))
		{
			BGM_main.Stop();
			BGM_title.Play();
		}

		//遷移後のシーン名を「１つ前のシーン名」として保持
		beforeScene = nextScene.name;
	}
}