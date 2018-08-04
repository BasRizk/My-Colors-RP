using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine.Networking;

public class DataController : MonoBehaviour {


	private LoadingController loadingController;
	private Queue msgsQueue;
	private SignalsTrackerConnection signalsTracker;
	private static Thread signalsTrackerServerThread;
	private static bool errorOccured;

	private LearningData currentLearningSet;
	private bool learningOn;
	
	private RoundData currentQuestionSet;
	private SummaryData summaryData;
	public PlayerPersonalData playerPersonalData;

	public int colorSetNum = 0;
	public int MAX_NUM_OF_PHASES = 3;


	// Filenames & Pathes
	private static string PYTHON_3_PATH  = @"C:\ProgramData\Anaconda3\python.exe";
	private static string PYTHON_27_32_PATH = @"C:\ProgramData\Anaconda3\pkgs32\python-2.7.15-he216670_0\python.exe";
	private static string SIGNALS_TRACKER_SERVER_FILE_NAME = @"Backend\CyKITv2\CyKITv2.py";
	private static string SIGNALS_TRACKER_SERVER_PATH;
	private static string MY_COLORS_BACK_MAIN_FILE_NAME = @"Backend\MyColorsBack.py";
	private static string MY_COLORS_BACK_PATH;
	private static string BACK_LEARNING_DATA_GENERATION_COMMAND = "GenerateLearningData";
	private static string BACK_QUESTIONS_DATA_GENERATION_COMMAND = "GenerateColorsQuestions";
	private static string BACK_ERRORS_CALCULATION_COMMAND = "CaculateErrors";
	private static string UPDATED_QUESTIONS_DATA_FILE_NAME = "updated_questions_data.json";
	private static string UPDATED_QUESTIONS_DATA_PATH;
	private static string UPDATED_LEARNING_DATA_FILE_NAME = "updated_learning_data.json";
	private static string UPDATED_LEARNING_DATA_PATH;
	private static string THIS_GAME_FOLDERNAME;
	private static string THIS_GAME_DATA_PATH;
	private static string PAST_ANSWERS_FILE_NAME = "past_answers.csv";
	private static string PAST_ANSWERS_DATA_PATH;
	private static string PAST_COLORS_FILE_NAME = "past_colors.json";
	private static string PAST_COLORS_DATA_PATH;
	private static string SESSION_ERRORS_FILE_NAME = "session_errors.json";
	private static string SESSION_ERRORS_DATA_PATH;
	private static string APP_STREAMING_ASSETS_PATH;
	private static string APP_DATA_PATH;
	private static string SIGNALS_RECORD = "eeg_record_";

	protected bool serverIntialized = false;
	protected bool signalsTrackerConnected = false;
	protected bool PreviousDataCleared = false;
	protected bool LearningSetLoaded = false;
	public bool loadingData = true;

	
	void Start () {

		DontDestroyOnLoad(gameObject);

		learningOn = false;
		errorOccured = false;
		
		APP_STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
		APP_DATA_PATH = Path.Combine(APP_STREAMING_ASSETS_PATH, "Data");
		MY_COLORS_BACK_PATH = Path.Combine(APP_STREAMING_ASSETS_PATH, MY_COLORS_BACK_MAIN_FILE_NAME);
		SIGNALS_TRACKER_SERVER_PATH = Path.Combine(APP_STREAMING_ASSETS_PATH, SIGNALS_TRACKER_SERVER_FILE_NAME);
		UPDATED_QUESTIONS_DATA_PATH = Path.Combine(APP_DATA_PATH, UPDATED_QUESTIONS_DATA_FILE_NAME);
		UPDATED_LEARNING_DATA_PATH = Path.Combine(APP_DATA_PATH, UPDATED_LEARNING_DATA_FILE_NAME);
		PAST_ANSWERS_DATA_PATH = Path.Combine(APP_DATA_PATH, PAST_ANSWERS_FILE_NAME);
		PAST_COLORS_DATA_PATH = Path.Combine(APP_DATA_PATH, PAST_COLORS_FILE_NAME);
		SESSION_ERRORS_DATA_PATH = Path.Combine(APP_DATA_PATH, SESSION_ERRORS_FILE_NAME);

		loadingData = true;
		SceneManager.LoadScene("Loading");

		loadingController = FindObjectOfType<LoadingController>();

		Thread dataInitThread = new Thread(new ThreadStart(Init));
		dataInitThread.Start();	
	}

	public void Init() {
		msgsQueue = new Queue();
		InitSignals();
		InitNewSessionData();

		while(signalsTracker != null && !signalsTracker.isConnected()) {
			UnityEngine.Debug.Log("signals Tracker not connected.");
		}
		
		loadingData = false;

	}

	private void InitNewSessionData() {
		ClearPreviousCachedData();
		UnityEngine.Debug.Log("All Previous cached data have been deleted successfully.");

		LoadGameData();	
		UnityEngine.Debug.Log("Game data Loader has been started and learning set has been observed successfully.");
	}

	void InitSignals() {
		signalsTrackerServerThread = new Thread(new ThreadStart(RunSignalsTrackerServer));
		signalsTrackerServerThread.Start();
		UnityEngine.Debug.Log("Signals Tracker Server should be up and running.");

		signalsTracker = new SignalsTrackerConnection();
		signalsTracker.ConnectToTcpServer();
	}

	private void TellLoadingControllerAboutNewUpdates() {
		loadingController.TrackData((string)msgsQueue.Dequeue());
	}

	// Update is called once per frame
	void Update() {
		
		if (Input.GetKey("escape") || errorOccured) {

			Application.Quit();
		}
        
	}
	
	private void OnApplicationQuit() {
		UnityEngine.Debug.Log("System is about to quit..and abort server thread with it.");
		if (signalsTrackerServerThread != null) 
			signalsTrackerServerThread.Abort();
		if (signalsTrackerServerThread != null)
			signalsTracker.AbortThreads();
	}

	private void ClearPreviousCachedData() {
		File.Delete(UPDATED_QUESTIONS_DATA_PATH);
		File.Delete(UPDATED_LEARNING_DATA_PATH);
		File.Delete(PAST_ANSWERS_DATA_PATH);
		File.Delete(PAST_COLORS_DATA_PATH);
	}

	public void IntializeNewGameDataFiles() {
		CreateGameCsvFile();
		CreateAnswersCsvFile();
		//CreatePastColorsCSVFile();
	}

	public void HandleError(string errorMsg) {
		errorOccured = true;
		UnityEngine.Debug.Log("Error just occured" + " : " + errorMsg);
	}
	
	private void RunSignalsTrackerServer() {
		
		UnityEngine.Debug.Log("About to Run Signals Tracker Server");
		ProcessStartInfo backProcessStartInfo = new ProcessStartInfo(PYTHON_27_32_PATH);
		// Making sure that it can read the output from stdout
		backProcessStartInfo.UseShellExecute = false;
		backProcessStartInfo.CreateNoWindow = false;
		backProcessStartInfo.RedirectStandardOutput = true;
		backProcessStartInfo.RedirectStandardError = true;
		
		string EEG_LOGS_PATH = Path.Combine(APP_DATA_PATH, @"EEG-Logs\");
		backProcessStartInfo.Arguments = 
			string.Format("\"{0}\" \"{1}\"", SIGNALS_TRACKER_SERVER_PATH, EEG_LOGS_PATH);
		
		UnityEngine.Debug.Log("Arguments to be sent: \n" + backProcessStartInfo.Arguments);

		Process signalsTrackerServerProcess = new Process();
		signalsTrackerServerProcess.StartInfo = backProcessStartInfo;
		signalsTrackerServerProcess.Start();

		UnityEngine.Debug.Log("Signals Tracker Server Process has just started.");
		
		using (StreamReader reader = signalsTrackerServerProcess.StandardOutput)
		{
			string stderr = signalsTrackerServerProcess.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
			if (stderr != null && stderr != "") {
				UnityEngine.Debug.LogError("Standard Error from Python Process:\n" + stderr);
				this.HandleError("Error Running the Singals Tracker Server.");
				return;
			}
			string outputString = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
			if (outputString != null && outputString != "") {
				UnityEngine.Debug.Log("Output String from Python Process:\n" + outputString);
			}
				
		}

		if (signalsTrackerServerProcess != null) {
			// Waiting for exit signal from the Back Process			
			signalsTrackerServerProcess.WaitForExit();
			signalsTrackerServerProcess.Close();
		}
		
		UnityEngine.Debug.Log("Signals Tracker Server loaded successfully");
	}

	private void RunMyColorsBack(string operation) {
		
		ProcessStartInfo backProcessStartInfo = new ProcessStartInfo(PYTHON_3_PATH);
		// Making sure that it can read the output from stdout
		backProcessStartInfo.UseShellExecute = false;
		backProcessStartInfo.CreateNoWindow = true;
		backProcessStartInfo.RedirectStandardOutput = true;
		backProcessStartInfo.RedirectStandardError = true;
		backProcessStartInfo.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"",
			MY_COLORS_BACK_PATH, APP_STREAMING_ASSETS_PATH, operation);

		Process backProcess = new Process();
		backProcess.StartInfo = backProcessStartInfo;
		backProcess.Start();

		
		using (StreamReader reader = backProcess.StandardOutput)
		{
			string stderr = backProcess.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
			if (stderr != null && stderr != "")
				UnityEngine.Debug.LogError("Standard Error from Python Process:\n" + stderr);
				//HandleError("Error in operation: " + operation);
			string outputString = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
			if (outputString != null && outputString != "")
				UnityEngine.Debug.Log("Output String from Python Process:\n" + outputString);
		}

		
		// Waiting for exit signal from the Back Process
		backProcess.WaitForExit();
			
		backProcess.Close();
		
		UnityEngine.Debug.Log(operation + " loaded successfully");
	}

	private void LoadLearningSet() {
		if(File.Exists (UPDATED_LEARNING_DATA_PATH))
			File.Delete(UPDATED_LEARNING_DATA_PATH);
		RunMyColorsBack(BACK_LEARNING_DATA_GENERATION_COMMAND);

		if(File.Exists (UPDATED_LEARNING_DATA_PATH)) {
			string dataAsJson = File.ReadAllText(UPDATED_LEARNING_DATA_PATH);
			currentLearningSet = JsonUtility.FromJson<LearningData>(dataAsJson);
		} else {
            UnityEngine.Debug.LogError("No learning data found.");
			HandleError("Error Loading the Learning Set.");
		}
	}
	private void LoadQuestionsData() {		
		File.Delete(UPDATED_QUESTIONS_DATA_PATH);
		RunMyColorsBack(BACK_QUESTIONS_DATA_GENERATION_COMMAND);

		if(File.Exists (UPDATED_QUESTIONS_DATA_PATH)) {
			string dataAsJson = File.ReadAllText(UPDATED_QUESTIONS_DATA_PATH);
			RoundData loadedData = JsonUtility.FromJson<RoundData>(dataAsJson);
			currentQuestionSet = loadedData;
		} else {
            UnityEngine.Debug.LogError("No Question data found.");
			HandleError("Error Loading Question Set.");
		}
	}

	private void LoadGameSummary() {
		File.Delete(SESSION_ERRORS_DATA_PATH);
		RunMyColorsBack(BACK_ERRORS_CALCULATION_COMMAND);

		if(File.Exists (UPDATED_QUESTIONS_DATA_PATH)) {
			string dataAsJson = File.ReadAllText(SESSION_ERRORS_DATA_PATH);
			SummaryData loadedData = JsonUtility.FromJson<SummaryData>(dataAsJson);
			summaryData = loadedData; 
		} else {
            UnityEngine.Debug.LogError("No Summary data found.");
			HandleError("Error Loading Summary Data.");
		}
	}


	public void StartRecordingSignals() {
		signalsTracker.StartRecord(SIGNALS_RECORD + THIS_GAME_FOLDERNAME + "_" + colorSetNum);
	}

	public void StopRecordingSignals() {
		signalsTracker.StopRecord();
	}

	public void LoadNextPhase() {
		LearningPhaseFinished();
		StopRecordingSignals();
		if(MAX_NUM_OF_PHASES > 0) {
			StartRecordingSignals();
		}
		LoadGameData();
	}

	public void LoadGameData() {
		LearningPhaseFinished();
		MAX_NUM_OF_PHASES--;
		
		if(MAX_NUM_OF_PHASES >= 0) {
			LoadLearningSet();
		} else {
			UnityEngine.Debug.Log("This was the last load of Learning Sets..Set #" +
				colorSetNum +" ..Question are about to be Loaded.");
			Thread loadingGameDataThread = new Thread(new ThreadStart(LoadQuestionsData));
			loadingGameDataThread.Start();
		} 
		
	}

	private void LearningPhaseFinished() {
		currentQuestionSet = null;
		currentLearningSet = null;
		colorSetNum++;
	}

	public void GameFinished(ArrayList roundAnswers) {
		AnswersToCSV(roundAnswers);
		this.SaveGameData();
		currentQuestionSet = null;
		currentLearningSet = null;
		InitNewSessionData();
	}

	private void SaveGameData() {
		try {
			string[] allFiles = System.IO.Directory.GetFiles(APP_DATA_PATH);
			foreach (string file in allFiles) {
				if (file != THIS_GAME_DATA_PATH) {
					string fName = file.Substring(APP_DATA_PATH.Length + 1);
					File.Copy(Path.Combine(APP_DATA_PATH, fName), Path.Combine(THIS_GAME_DATA_PATH, fName), true);
				}
			}
			UnityEngine.Debug.Log("This game data has been saved safely.");
		} catch (DirectoryNotFoundException dirNotFound) {
			UnityEngine.Debug.LogError(dirNotFound.Message);
		}
		
	}

	private void CreateGameCsvFile() {
		string name = playerPersonalData.playerName;
		string age = playerPersonalData.age.ToString();
		string gender = playerPersonalData.gender.ToString();
		string score = playerPersonalData.highestScore.ToString();
		
		

		var csv = new StringBuilder();
		var csvLine = string.Format("Name,Age,Gender==Male,Score");
		csv.AppendLine(csvLine);
		csvLine = string.Format("{0},{1},{2},{3}", name, age, gender, score);
		csv.AppendLine(csvLine);

		int nextInt = PlayerPrefs.GetInt("gameID") + 1;
		PlayerPrefs.SetInt("gameID", nextInt);

		THIS_GAME_FOLDERNAME = name + "_" + age + "_" + PlayerPrefs.GetInt("gameID");
		THIS_GAME_DATA_PATH = Path.Combine(APP_DATA_PATH, THIS_GAME_FOLDERNAME);
		string this_game_metadata_path = Path.Combine(THIS_GAME_DATA_PATH, THIS_GAME_FOLDERNAME + ".csv");
		
		System.IO.Directory.CreateDirectory(THIS_GAME_DATA_PATH);
		UnityEngine.Debug.Log("This game data folder has been intialized successfully.");
		
		File.WriteAllText(this_game_metadata_path, csv.ToString());
		UnityEngine.Debug.Log("This game data metadata saved.");

	}

	private void CreateAnswersCsvFile() {
		var csv = new StringBuilder();
		var newLine = string.Format("ColorName,Red,Blue,Green,isCorrect");
		csv.AppendLine(newLine);  
		File.WriteAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
	}
	private void CreatePastColorsCSVFile() {
		var csv = new StringBuilder();
		var newLine = string.Format("ColorName,Red,Blue,Green,timeToLearn");
		csv.AppendLine(newLine);  
		File.WriteAllText(PAST_COLORS_DATA_PATH, csv.ToString());
	}
	private void AnswersToCSV(ArrayList roundAnswers) {
		var csv = new StringBuilder();
		var newLine = ""; 
		foreach (AnswerData answer in roundAnswers) {
			string colorName = answer.colorName;
			byte [] colorRGB = answer.colorRGB;
			bool isCorrect = answer.isCorrect;
			newLine = string.Format("{0},{1},{2},{3},{4}",
				colorName,colorRGB[0], colorRGB[1], colorRGB[2], isCorrect);
			csv.AppendLine(newLine);  
		}

		File.AppendAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
	}
	private void ColorsToCSV() {
		var csv = new StringBuilder();
		var csvLine = "";
		
		foreach (ColorToLearnData colorData in currentLearningSet.colorsToLearn) {
			string colorName = colorData.colorName;
			byte [] colorRGB = colorData.colorRGB;
			float timeToLearn = colorData.timeToLearn;
			int colorHexInDec = colorData.colorHexInDec;
			csvLine = string.Format("{0},{1},{2},{3},{4}",
				colorHexInDec, colorName,colorRGB[0], colorRGB[1], colorRGB[2], timeToLearn);
			csv.AppendLine(csvLine);  
		}

		File.AppendAllText(PAST_COLORS_DATA_PATH, csv.ToString());

	}

	public void SetPlayerPersonalData(string name, int age, string playerGender) {
		playerPersonalData = new PlayerPersonalData();
		playerPersonalData.playerName = name;
		playerPersonalData.age = age;
		playerPersonalData.gender = playerGender;
		
		// Player HighestScore is just the highest score of all
		if(PlayerPrefs.HasKey("highestScore")) {
			playerPersonalData.highestScore = PlayerPrefs.GetInt("highestScore");
		} else {
			playerPersonalData.highestScore = 0;
		}
	}
	public void SubmitNewPlayerScore(int newScore) {
		if(newScore > playerPersonalData.highestScore) {
			playerPersonalData.highestScore = newScore;
			PlayerPrefs.SetInt("highestScore", playerPersonalData.highestScore);
		}
	}
	public int GetHighestScore() {
		return playerPersonalData.highestScore;
	}
	public bool isLearning() {
		return learningOn;
	}
	public LearningData GetCurrentLearningSet() {
		return currentLearningSet;
	}
	public RoundData GetCurrentQuestionSet() {
		return currentQuestionSet;
	}
	public SummaryData GetSummaryData() {
		return summaryData;
	}
	public bool isQuestionSetLoaded() {
		return currentQuestionSet != null;
	}
	public bool isLearningSetLoaded() {
		return currentLearningSet != null;
	}
	public bool isSummaryDataLoaded() {
		return summaryData != null;
	}
	public void setLearning(bool learningOrNot) {
		learningOn = learningOrNot;
	}
}
