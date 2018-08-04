using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class SummaryController : MonoBehaviour {

	public Text summary;
	public Text personalData;
	public Text generalErrors;
	public Text accumlativeErrors;
	private DataController dataController;
	SummaryData summaryData;
	/*
	GUIStyle guiStyle = new GUIStyle();
	void OnGUI() {
		guiStyle.fontSize = 25;
		guiStyle.normal.textColor = Color.white;
		GUI.BeginGroup(new Rect(10, 10, 250, 150));
		GUI.Box(new Rect(0,0,140,140), "Stats ", guiStyle);
		
		GUI.EndGroup();
	}
	*/

	// Use this for initialization
	void Start () {
		dataController = FindObjectOfType<DataController>();
		// Personal Data
		string playerName, playerAge, playerGender;
		
		if (dataController != null) {
			// TODO What if summary data not available yet
			summaryData = dataController.GetSummaryData();
			playerName = dataController.playerPersonalData.playerName;
			playerAge = dataController.playerPersonalData.age.ToString();
			playerGender = dataController.playerPersonalData.gender;
		} else {
			summaryData = new SummaryData();
			playerName = "Test_Name";
			playerAge = "99";
			playerGender = "Female";
		}

		string LITTLE_SPACE = "   "; 
		string BIG_SPACE = "     ";
		personalData.color = Color.white;
		personalData.text = "Personal Data: \n\n" + BIG_SPACE +
							"Name: " + playerName + "\n" + BIG_SPACE +
							"Age: " + playerAge + "\n" + BIG_SPACE +
							"Gender: " + playerGender + "\n" + BIG_SPACE; 

		generalErrors.text = "General Errors: \n\n" + BIG_SPACE +
			"# Of Unmatched Vectors:" + BIG_SPACE + summaryData.numOfDifferentVectors + "\n" + BIG_SPACE +
			"# Of Reversed Vectors:" + BIG_SPACE + summaryData.numOfReversedVectors + "\n" + BIG_SPACE +
			"Total Unmatched Vectors Error =" + BIG_SPACE + summaryData.vectorsDiffError + "%\n" + BIG_SPACE +
			"Total Values Diff. Absolute Error =" + BIG_SPACE + summaryData.valuesDiffAbsError + "%\n" + BIG_SPACE +
			"Total Values Diff. Reversed Error =" + BIG_SPACE + summaryData.valuesDiffRevError + "%\n" + BIG_SPACE +
			"Total Values Diff. Support Error =" + BIG_SPACE + summaryData.valuesDiffSupError + "%\n" + BIG_SPACE +
			"Total Values Per Identical Vector Error" + BIG_SPACE + summaryData.valuesIdenticalPerVectorError + "%\n" + BIG_SPACE;

		int num_of_errors = 1;
		double [] accumlativeErrorsArray = summaryData.accumlativeErrors;
		accumlativeErrors.text = "Accumlative Errors: \n\n" + LITTLE_SPACE;
		while(num_of_errors <= accumlativeErrorsArray.Length) {
			accumlativeErrors.text+= "Phase " + num_of_errors + "\n" + BIG_SPACE +
								accumlativeErrorsArray[num_of_errors - 1] + "%\n" + LITTLE_SPACE;
			num_of_errors++;
		}
		accumlativeErrors.text+= "*Accumlation Done using Errors Per Identical Vector";
 	}

	public void ReturnToMenu() {
		SceneManager.LoadScene("MenuScreen");
	}
}
