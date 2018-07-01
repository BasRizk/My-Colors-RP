using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour {

	public Text answerText;
	private AnswerData answerData;
	private GameController gameController;

	// Use this for initialization
	void Start () {
		gameController = FindObjectOfType<GameController>();
	}

	public void Setup(AnswerData data) {
		
		answerData = data;
		answerText.text = answerData.colorName;

		/*
		this.GetComponent<Image>().color = new Color32(answerData.colorRGB[0],
														answerData.colorRGB[1],
														answerData.colorRGB[2]);
														*/
	}
	
	public void HandleClick() {
		gameController.AnswerButtonClicked(answerData);
	}
}
