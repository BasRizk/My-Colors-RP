using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Collections;

public class GameController : MonoBehaviour {
	
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
	private bool loadingLearningData;
	private float timeRemaining;
	private int playerScore;
	
	private int questionIndex;
	private List<GameObject> answerButtonGameObjects = new List<GameObject>();
	

	// Constants
	private Color questionDisplayBackgroundColor;
	private int colorMultiplier;
	private const int OBAQUE_VALUE = 255;
	private readonly string LOADING_STATEMENT = "Loading...";
	private readonly string NEXT_ROUND_STATEMENT = "Next Round";
	private Color loadingBtnTextDefaultColor;
	private string CURRENT_PLAYER_SCORE_PREF = "currentPlayerScore";


	// Current Round Data and Results
	private RoundData currentRoundData;
	private ArrayList currentRoundAnswers;
	private static bool created = false;

	void Start () {


		if (!created)
        {
            created = true;
			playerScore = 0;
			PlayerPrefs.SetInt(CURRENT_PLAYER_SCORE_PREF, playerScore);
			UnityEngine.Debug.Log("I am here in Start in GameController for first Creation.");
        }
		
		dataController = FindObjectOfType<DataController>();
		loadingBtnTextDefaultColor = nextRoundBtn.GetComponentInChildren<Text>().color;
		UnityEngine.Debug.Log("I am here in Start in GameController.");
		InitRound();
		
	}

	private void InitRound() {
		
		// In case of coming from another scene
		playerScore = PlayerPrefs.GetInt(CURRENT_PLAYER_SCORE_PREF);
		scoreDisplayText.text = playerScore.ToString();

		loadingLearningData = true;
		while(!dataController.isRoundDataLoaded() && !dataController.isLearning());
		questionDisplay.SetActive(true);
		roundEndDisplay.SetActive(false);
		loadingLearningData = false;
		currentRoundData = dataController.GetCurrentRoundData();
		currentRoundAnswers = new ArrayList();
		questionPool = currentRoundData.questions;
		
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
				EndRound();
			}
		} else {
			if (loadingLearningData){
				
				Color loadingBtnTextColor = nextRoundBtn.GetComponentInChildren<Text>().color;
				nextRoundBtn.GetComponentInChildren<Text>().color =
					new Color(loadingBtnTextColor.r, loadingBtnTextColor.g, loadingBtnTextColor.b, Mathf.PingPong(Time.time, 1));

				if(dataController.isLearningDataLoaded()) {
					nextRoundBtn.enabled = true;
					nextRoundBtn.GetComponentInChildren<Text>().text = NEXT_ROUND_STATEMENT;
					loadingLearningData = false;
					nextRoundBtn.GetComponentInChildren<Text>().color = loadingBtnTextDefaultColor;

				}
			} else if (!dataController.isLearningDataLoaded()) {
				loadingLearningData = true;
				Thread loadingGameDataThread = new Thread(new ThreadStart(LoadNewRoundData));
				loadingGameDataThread.Priority = System.Threading.ThreadPriority.Highest;
				loadingGameDataThread.Start();
			}
			
		}
		
		
		
	}

	public void AnswerButtonClicked(AnswerData answer) {
		if(answer.isCorrect) {
			playerScore += currentRoundData.pointsAddedForCorrectAnswer;
			scoreDisplayText.text = playerScore.ToString();

		}

		currentRoundAnswers.Add(answer);

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
		nextRoundBtn.GetComponentInChildren<Text>().text = LOADING_STATEMENT;

		isRoundActive = false;

		dataController.SubmitNewPlayerScore(playerScore);
		highScoreDisplay.text = dataController.GetHighestScore().ToString();
		
		questionDisplay.SetActive(false);
		roundEndDisplay.SetActive(true);

		nextRoundBtn.enabled = false;
		dataController.GameFinished(currentRoundAnswers);
		ClearPastRoundCache();

	}

	private void ClearPastRoundCache() {
		currentRoundAnswers.Clear();
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
		timeRemaining = currentRoundData.timeLimitInSeconds;
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
