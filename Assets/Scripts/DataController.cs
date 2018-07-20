using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Threading;
using UnityEngine.Networking;

public class DataController : MonoBehaviour {


	private SignalsTrackerConnection signalsTracker;
	private static Thread signalsTrackerServerThread;
	private static bool errorOccured;

	private LearningData currentLearningSet;
	private bool learningOn;
	private static int colorSetNum = 0;
	
	private RoundData currentQuestionSet;
	private PlayerProgress playerProgress;s


	// Filenames & Pathes
	private static string this_game_file_name;
	private static string this_game_data_path;
	private static string PYTHON_3_PATH  = @"C:\ProgramData\Anaconda3\python.exe";
	private static string PYTHON_27_32_PATH = @"C:\ProgramData\Anaconda3\pkgs32\python-2.7.15-he216670_0\python.exe";
	private static string SIGNALS_TRACKER_SERVER_FILE_NAME = @"Backend\CyKITv2\CyKITv2.py";
	private static string SIGNALS_TRACKER_SERVER_PATH;
	private static string MY_COLORS_BACK_MAIN_FILE_NAME = @"Backend\MyColorsBack.py";
	private static string MY_COLORS_BACK_PATH;
	private static string BACK_LEARNING_DATA_GENERATION_COMMAND = "GenerateLearningData";
	private static string BACK_QUESTIONS_DATA_GENERATION_COMMAND = "GenerateQuestionsData";
	private static string UPDATED_QUESTIONS_DATA_FILE_NAME = "updated_questions_data.json";
	private static string UPDATED_QUESTIONS_DATA_PATH;
	private static string UPDATED_LEARNING_DATA_FILE_NAME = "updated_learning_data.json";
	private static string UPDATED_LEARNING_DATA_PATH;
	private static string PAST_ANSWERS_FILE_NAME = "past_answers.csv";
	private static string PAST_ANSWERS_DATA_PATH;
	//private static string PAST_GAMES_DATA_FOLDER_NAME = @"Past Games";
	private static string PAST_GAMES_DATA_FOLDER_PATH;
	private static string APP_STREAMING_ASSETS_PATH;
	private static string APP_DATA_PATH;
	private static string SIGNALS_RECORD = "eeg_record_";

	
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
		PAST_GAMES_DATA_FOLDER_PATH = Path.Combine(APP_DATA_PATH, "");
		
		signalsTrackerServerThread = new Thread(new ThreadStart(RunSignalsTrackerServer));
		signalsTrackerServerThread.Start();	
		UnityEngine.Debug.Log("Signals Tracker Server should be up and running.");

		signalsTracker = new SignalsTrackerConnection(this);
		signalsTracker.ConnectToTcpServer();


		ClearPreviousCachedData();
		UnityEngine.Debug.Log("All Previous cached data have been deleted successfully.");

		LoadGameData();	
		UnityEngine.Debug.Log("Game data Loader has been started and learning set has been observed successfully.");


		while(signalsTracker != null && !signalsTracker.isConnected()) {
			UnityEngine.Debug.Log("signals Tracker not connected.");
		}
		
		
		UnityEngine.Debug.Log("About to load to menu screen.");
		SceneManager.LoadScene("MenuScreen");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKey("escape") || errorOccured) {
			Application.Quit();
		}
        
	}
	
	private void OnApplicationQuit() {
		signalsTrackerServerThread.Abort();
	}

	private void ClearPreviousCachedData() {
		File.Delete(UPDATED_QUESTIONS_DATA_PATH);
		File.Delete(UPDATED_LEARNING_DATA_PATH);
		File.Delete(PAST_ANSWERS_DATA_PATH);
	}

	public void IntializeNewGameDataFiles() {
		CreateGameCsvFile();
		CreateAnswersCsvFile();
		CreateLearnedDataCsvFile();
	}

	public void HandleError() {
		errorOccured = true;
		
	}
	
	private void RunSignalsTrackerServer() {
		
		UnityEngine.Debug.Log("About to Run Signals Tracker Server");
		ProcessStartInfo backProcessStartInfo = new ProcessStartInfo(PYTHON_27_32_PATH);
		// Making sure that it can read the output from stdout
		backProcessStartInfo.UseShellExecute = false;
		backProcessStartInfo.CreateNoWindow = false;
		backProcessStartInfo.RedirectStandardOutput = true;
		backProcessStartInfo.RedirectStandardError = true;
		string EEG_LOGS_PATH = Path.Combine(APP_DATA_PATH, "EGG-Logs");
		backProcessStartInfo.Arguments = string.Format("\"{0}\" \"{1}\"",
			SIGNALS_TRACKER_SERVER_PATH, EEG_LOGS_PATH);
		
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
				HandleError();
			}
			string outputString = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
			if (outputString != null && outputString != "") {
				UnityEngine.Debug.Log("Output String from Python Process:\n" + outputString);
			}
				
		}

		UnityEngine.Debug.Log("Signals Tracker loaded successfully");
	}

	private static void RunMyColorsBack(string operation) {
		
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
		}

	}

	public void StartRecordingSignals() {
		signalsTracker.StartRecord(SIGNALS_RECORD + this_game_file_name);
	}

	public void StopRecordingSignals() {
		signalsTracker.StopRecord();
	}

	private void LoadQuestionsData() {
		
		File.Delete(UPDATED_QUESTIONS_DATA_PATH);
		RunMyColorsBack(BACK_QUESTIONS_DATA_GENERATION_COMMAND);

		if(File.Exists (UPDATED_QUESTIONS_DATA_PATH)) {
			string dataAsJson = File.ReadAllText(UPDATED_QUESTIONS_DATA_PATH);
			RoundData loadedData = JsonUtility.FromJson<RoundData>(dataAsJson);
			currentQuestionSet = loadedData;
		} else {
            UnityEngine.Debug.LogError("No round data found.");

		}

	}

	public void LoadGameData() {
		LoadLearningSet();

		Thread loadingGameDataThread = new Thread(new ThreadStart(LoadQuestionsData));
		loadingGameDataThread.Start();
	}

	public void GameFinished(ArrayList roundAnswers) {
		AnswersToCSV(roundAnswers);
		LearningDataToCSV();
		this.SaveGameData();
		currentQuestionSet = null;
		currentLearningSet = null;
		
	}

	private void SaveGameData() {

		var csv = new StringBuilder();

		var csvLine = string.Format("ColorName,Red,Blue,Green,timeToLearn");
		csv.AppendLine(csvLine);

		foreach (ColorToLearnData colorData in currentLearningSet.colorsToLearn) {
			string colorName = colorData.colorName;
			byte [] colorRGB = colorData.colorRGB;
			float timeToLearn = colorData.timeToLearn;
			csvLine = string.Format("{0},{1},{2},{3}",
				colorName,colorRGB[0], colorRGB[1], colorRGB[2], timeToLearn);
			csv.AppendLine(csvLine);  
		}

		UnityEngine.Debug.Log("Game data has been saved in '" + this_game_data_path + "' and if game continued it would be overwritten.");
		File.AppendAllText(this_game_data_path, csv.ToString());
	}

	private void CreateGameCsvFile() {
		string name = playerProgress.playerName;
		string age = playerProgress.age.ToString();
		string gender = playerProgress.gender.ToString();
		string score = playerProgress.highestScore.ToString();
		
		

		var csv = new StringBuilder();
		var csvLine = string.Format("Name,Age,Gender==Male,Score");
		csv.AppendLine(csvLine);
		csvLine = string.Format("{0},{1},{2},{3}", name, age, gender, score);
		csv.AppendLine(csvLine);

		int nextInt = PlayerPrefs.GetInt("gameID") + 1;
		PlayerPrefs.SetInt("gameID", nextInt);

		this_game_file_name = name + "_" + age + "_" + PlayerPrefs.GetInt("gameID");  
		this_game_data_path = Path.Combine(PAST_GAMES_DATA_FOLDER_PATH, this_game_file_name);
		UnityEngine.Debug.Log("Game data file has been intialized successfully.");
		File.WriteAllText(this_game_data_path, csv.ToString());

	}
	private void CreateAnswersCsvFile() {
		var csv = new StringBuilder();
		var newLine = string.Format("ColorName,Red,Blue,Green,isCorrect");
		csv.AppendLine(newLine);  
		File.WriteAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
	}
	private void CreateLearnedDataCsvFile() {
		var csv = new StringBuilder();
		var newLine = string.Format("ColorName,Red,Blue,Green,timeToLearn");
		csv.AppendLine(newLine);  
		File.WriteAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
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
	private void LearningDataToCSV() {
		var csv = new StringBuilder();
		var newLine = ""; 
		foreach (ColorToLearnData colorData in currentLearningSet.colorsToLearn) {
			string colorName = colorData.colorName;
			byte [] colorRGB = colorData.colorRGB;
			float timeToLearn = colorData.timeToLearn;
			newLine = string.Format("{0},{1},{2},{3}",
				colorName,colorRGB[0], colorRGB[1], colorRGB[2], timeToLearn);
			csv.AppendLine(newLine);  
		}

		File.AppendAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
	}

	public void SetPlayerProgressData(string name, int age, bool genderMale) {
		playerProgress = new PlayerProgress();
		playerProgress.playerName = name;
		playerProgress.age = age;
		playerProgress.gender = genderMale;

		// Player HighestScore is just the highest score of all
		if(PlayerPrefs.HasKey("highestScore")) {
			playerProgress.highestScore = PlayerPrefs.GetInt("highestScore");
		} else {
			playerProgress.highestScore = 0;
		}
	}
	public void SubmitNewPlayerScore(int newScore) {
		if(newScore > playerProgress.highestScore) {
			playerProgress.highestScore = newScore;
			PlayerPrefs.SetInt("highestScore", playerProgress.highestScore);
		}
	}
	public int GetHighestScore() {
		return playerProgress.highestScore;
	}
	public bool isLearning() {
		return learningOn;
	}
	public LearningData GetcurrentLearningSet() {
		return currentLearningSet;
	}

	public RoundData GetCurrentQuestionSet() {
		return currentQuestionSet;
	}
	public bool isQuestionSetLoaded() {
		return currentQuestionSet != null;
	}
	public bool isLearningSetLoaded() {
		return currentLearningSet != null;
	}
	public void setLearning(bool learningOrNot) {
		learning = learningOrNot;
	}
}
