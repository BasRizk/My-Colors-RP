﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LearningController : MonoBehaviour {

	public Text colorName;
	public Text timeRemainingDisplay;
	private float timeRemaining;
	public BlackFader blackFader;
	public ArrayList learningQueue;
	private const int LEARNING_DURATION = 30;
	private const int OBAQUE_VALUE = 255;

	private DataController dataController;

	// Use this for initialization
	void Start () {

		dataController = FindObjectOfType<DataController>();
	
		InitLearningData();
		
	}

	private void InitLearningData() {

		while(!dataController.isLearningDataLoaded());
		learningQueue = new ArrayList();
		learningQueue.AddRange(dataController.GetCurrentLearningData().colorsToLearn);
		dataController.setLearning(true);
	}
	
	// Update is called once per frame
	void Update () {
		timeRemaining -= Time.deltaTime;
		UpdateTimeRemainingDisplay();
		
		if(timeRemaining <= 0f) {
			if(learningQueue.Count > 0) {
				ColorToLearnData colorToLearnData = (ColorToLearnData) learningQueue[0];
				learningQueue.RemoveAt(0);
				timeRemaining = colorToLearnData.timeToLearn;
				UpdateScreenForNewColor(colorToLearnData);
			} else {
				//blackFader.FadeToScreen("Game");
				dataController.setLearning(false);
				SceneManager.LoadScene("Game");
			}
		}
	}

	private void UpdateScreenForNewColor(ColorToLearnData LearningData) {
		
		colorName.text = LearningData.colorName;

		Color32 backgroundColor = new Color32(
				LearningData.colorRGB[0],
				LearningData.colorRGB[1],
				LearningData.colorRGB[2],
				OBAQUE_VALUE);

		GameObject.Find("ColorPanel").GetComponent<RawImage>().color = backgroundColor;

	}

	private void UpdateTimeRemainingDisplay() {
		timeRemainingDisplay.text = Mathf.Round(timeRemaining).ToString();
	}
}
