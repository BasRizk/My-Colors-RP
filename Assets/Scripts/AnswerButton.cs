using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour {

	public Text answerText;
	private AnswerData answerData;
	private QuestioningController questioningController;

	private bool answered = false;

	// Use this for initialization
	void Start () {
		questioningController = FindObjectOfType<QuestioningController>();
	}

	void Update() {
		if(answered) {
			if(answerData.isCorrect) {
				this.GetComponent<Image>().color = new Color32(answerData.colorRGB[0],
																	answerData.colorRGB[1],
																	answerData.colorRGB[2],
																	255);
			}
		}
		
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
		if(answerData.isCorrect) {
			answered = true;
		}
		questioningController.AnswerButtonClicked(answerData);
	}
}
