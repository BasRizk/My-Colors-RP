using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MenuScreenController : MonoBehaviour {
	
	
	public Button startGameBtn;
	public Text nameField;
	public Text gender;
	public Text age;

	private DataController dataController;

	void Start() {
		dataController = FindObjectOfType<DataController>();
	}
	void Update() {
		
	}

	public void StartGame() {
		string playerName = nameField.text;
		bool genderMale = gender.text == "Male";
		int playerAge = 0;
		Int32.TryParse(age.text, out playerAge);

		dataController.setPlayerProgressData(playerName, playerAge, genderMale);
		SceneManager.LoadScene("Learning");
	}

}
