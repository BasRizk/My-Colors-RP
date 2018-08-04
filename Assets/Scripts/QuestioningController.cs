using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Collections;

public class QuestioningController : MonoBehaviour {
	
	public SimpleObjectPool answerButtonObjectPool;
	public Text questionText;
	public Text scoreDisplayText;
	public Text timeRemainingDisplay;
	public Transform answerButtonParent;
	public GameObject questionDisplay;
	public GameObject roundEndDisplay;
	public Button nextRoundBtn;
	public Text highScoreDisplay;

	private DataController dataController;
	private QuestionData[] questionPool;

	private bool isRoundActive;
	private float timeRemaining;
	private int playerScore;
	
	private int questionIndex;
	private List<GameObject> answerButtonGameObjects = new List<GameObject>();

	// Manipulation Values
	private Color questionDisplayBackgroundColor;
	private int colorMultiplier;

	// Constants
	private const int OBAQUE_VALUE = 255;
	private string CURRENT_PLAYER_SCORE_PREF = "currentPlayerScore";

	// Current Round Data and Results
	private RoundData currentQuestionSet;
	private ArrayList currentAnswers;
	private static bool created = false;

	void Start () {


		if (!created)
        {
            created = true;
			playerScore = 0;
			PlayerPrefs.SetInt(CURRENT_PLAYER_SCORE_PREF, playerScore);
			UnityEngine.Debug.Log("Here in Start in GameController for first Creation.");
        }
		
		dataController = FindObjectOfType<DataController>();
		UnityEngine.Debug.Log("Here in Start in GameController.");
		InitRound();
		
	}

	private void InitRound() {
		
		// In case of coming from another scene
		playerScore = PlayerPrefs.GetInt(CURRENT_PLAYER_SCORE_PREF);
		scoreDisplayText.text = playerScore.ToString();

		while(!dataController.isQuestionSetLoaded() || !dataController.isLearning());
		
		questionDisplay.SetActive(true);
		roundEndDisplay.SetActive(false);

		currentQuestionSet = dataController.GetCurrentQuestionSet();
		currentAnswers = new ArrayList();
		questionPool = currentQuestionSet.questions;
		
		ResetTimeRemainingDisplay();
		UpdateTimeRemainingDisplay();

		questionIndex = 0;
		ShowQuestion();
		isRoundActive = true;

		questionDisplayBackgroundColor = new Color32(0,0,0,OBAQUE_VALUE);
	}

	// Update is called once per frame
	void Update () {


		if(isRoundActive) {
			timeRemaining -= Time.deltaTime;
			UpdateTimeRemainingDisplay();
			
			//PlayWithQuestionDisplayColors();

			if(timeRemaining <= 0f) {
				//EndRound();
				//AnswerData nullifiedAnswerData= new AnswerData("");
				//AnswerButtonClicked(nullifiedAnswerData);
				NextQuestionOrEndRound();
			}
		} else {
			/*
			if (loadingLearningData){
		 
				Color loadingBtnTextColor = nextRoundBtn.GetComponentInChildren<Text>().color;
				nextRoundBtn.GetComponentInChildren<Text>().color =
					new Color(loadingBtnTextColor.r, loadingBtnTextColor.g, loadingBtnTextColor.b, Mathf.PingPong(Time.time, 1));

				if(dataController.isLearningSetLoaded()) {
					nextRoundBtn.enabled = true;
					nextRoundBtn.GetComponentInChildren<Text>().text = "Next Round";
					loadingLearningData = false;
					nextRoundBtn.GetComponentInChildren<Text>().color = loadingBtnTextDefaultColor;

				}
			} else if (!dataController.isLearningSetLoaded()) {
				loadingLearningData = true;
				Thread loadingGameDataThread = new Thread(new ThreadStart(LoadNewRoundData));
				loadingGameDataThread.Priority = System.Threading.ThreadPriority.Highest;
				loadingGameDataThread.Start();
			}
			*/
		}
		
		
		
	}

	public void AnswerButtonClicked(AnswerData answer) {
		if(answer.isCorrect) {
			playerScore += currentQuestionSet.pointsAddedForCorrectAnswer;
			scoreDisplayText.text = playerScore.ToString();
			
		}

		currentAnswers.Add(answer);

		NextQuestionOrEndRound();
		
	}

	private void NextQuestionOrEndRound() {
		if(questionPool.Length > questionIndex + 1) {
			questionIndex++;
			ShowQuestion();
			ResetTimeRemainingDisplay();
		} else {
			EndRound();
		}
	}

	public void NextRound() {
		PlayerPrefs.SetInt(CURRENT_PLAYER_SCORE_PREF, playerScore);
		SceneManager.LoadScene("Learning");
	}
	

	public void EndRound() {
		nextRoundBtn.GetComponentInChildren<Text>().text = "Loading..";

		isRoundActive = false;

		dataController.SubmitNewPlayerScore(playerScore);
		highScoreDisplay.text = dataController.GetHighestScore().ToString();
		
		questionDisplay.SetActive(false);
		roundEndDisplay.SetActive(true);
		SceneManager.LoadScene("Loading");
		nextRoundBtn.enabled = false;
		dataController.GameFinished(currentAnswers);
		ClearPastRoundCache();

	}

	private void ClearPastRoundCache() {
		currentAnswers.Clear();
	}

	private void LoadNewRoundData() {
		dataController.LoadGameData();
	}
	private void ShowQuestion() {
		
		RemoveAnswerButtons();

		QuestionData questionData = questionPool[questionIndex];
		questionText.text = questionData.colorName;
		questionDisplayBackgroundColor = new Color32(
				questionData.colorRGB[0],
				questionData.colorRGB[1],
				questionData.colorRGB[2],
				OBAQUE_VALUE);

		GameObject.Find("QuestionPanel").GetComponent<RawImage>().color = questionDisplayBackgroundColor;

		for(int i = 0; i<questionData.answers.Length; i++) {
			
			GameObject answerButtonGameObject = answerButtonObjectPool.GetObject();
			answerButtonGameObject.transform.SetParent(answerButtonParent);
			answerButtonGameObjects.Add(answerButtonGameObject);
			
			AnswerButton answerButton =
			 answerButtonGameObject.GetComponent<AnswerButton>();
			answerButton.Setup(questionData.answers[i]);	

		}
	}

	private void RemoveAnswerButtons() {
		while(answerButtonGameObjects.Count > 0) {
			answerButtonObjectPool.ReturnObject(answerButtonGameObjects[0]);
			answerButtonGameObjects.RemoveAt(0);
		}
	}

	private void UpdateTimeRemainingDisplay() {
		timeRemainingDisplay.text = Mathf.Round(timeRemaining).ToString();
	}

	private void ResetTimeRemainingDisplay() {
		timeRemaining = currentQuestionSet.timeLimitInSeconds;
	}

	public void ReturnToMenu() {
		SceneManager.LoadScene("MenuScreen");
	}
	
	private void PlayWithQuestionDisplayColors() {

		if(questionDisplayBackgroundColor.r + questionDisplayBackgroundColor.g +
				 questionDisplayBackgroundColor.b >= 0.95) {
			colorMultiplier = -1;
		}
		if(questionDisplayBackgroundColor.r + questionDisplayBackgroundColor.g +
				 questionDisplayBackgroundColor.b <= 0.09){
			colorMultiplier = 1;
		}

		questionDisplayBackgroundColor.r += 0.1f*(colorMultiplier*Time.deltaTime);
		questionDisplayBackgroundColor.g += 0.2f*(colorMultiplier*Time.deltaTime);
		questionDisplayBackgroundColor.b += 0.3f*(colorMultiplier*Time.deltaTime);

		GameObject.Find("QuestionPanel").GetComponent<Image>().color = questionDisplayBackgroundColor;

	}

}
