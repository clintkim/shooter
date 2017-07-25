using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

	public AudioClip mainTheme;
	public AudioClip menuTheme;

	string sceneName;

	void Start() {
		OnLevelWasLoaded (0);
	
	}

	void OnLevelWasLoaded(int SceneIndex) {
		string newSceneName = SceneManager.GetActiveScene ().name;
		if (newSceneName != sceneName) {
			sceneName = newSceneName;
			Invoke ("PlayMusic", 2f);
		}
	}

	void PlayMusic() {
		AudioClip clipToplay = null;

		if (sceneName == "Menu") {
			clipToplay = menuTheme;
		} else if (sceneName == "Game") {
			clipToplay = mainTheme;
		}

		if (clipToplay != null) {
			AudioManager.instance.PlayMusic (clipToplay, 2);
			Invoke ("PlayMusic", clipToplay.length);
		}
	}

	void Update () {
		if (Input.GetKey (KeyCode.Space)) {
			AudioManager.instance.PlayMusic (mainTheme, 3);
		}
	}
}
