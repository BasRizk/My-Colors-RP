using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionData {

	public string colorName;
	public byte[] colorRGB;
	public AnswerData[] answers;

	public void print() {
		UnityEngine.Debug.Log(
			"Color name : " + colorName + "\n" +
			"Color RGB: " + colorRGB[0] + " " + colorRGB[1] + " " + colorRGB[2] + "\n" +
			"Answers: " + answers[0] + "\n" + answers[1] + "\n" + answers[2] + "\n");
	}	
}
