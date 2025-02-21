﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Threading;

public class MenuScreenController : MonoBehaviour {
	
	
	public Button startGameBtn;
	public Text nameField;
	public Text gender;
	public Text age;

	private DataController dataController;

	void Start() {
		dataController = FindObjectOfType<DataController>();
	}

	public void StartGame() {

		string playerName = nameField.text;
		//bool genderMale = gender.text == "Male";
		string playerGender = gender.text;
		int playerAge = 0;
		Int32.TryParse(age.text, out playerAge);

		dataController.SetPlayerPersonalData(playerName, playerAge, playerGender);
		dataController.IntializeNewGameDataFiles();
		dataController.StartRecordingSignals();

		UnityEngine.Debug.Log("Game Started at " + DateTime.Now.ToString("h:mm:ss tt"));
		SceneManager.LoadScene("Learning");
	}

}
