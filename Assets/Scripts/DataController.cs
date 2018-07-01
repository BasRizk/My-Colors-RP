using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Threading;

public class DataController : MonoBehaviour {

	private ArrayList allRoundsData;
	private RoundData currentRound;
	private LearningData currentLearningData;
	private PlayerProgress playerProgress;
	private bool learning;

	// Pathes
	private static string PYTHON_PATH  = @"C:\ProgramData\Anaconda3\python.exe";
	private static string MY_COLORS_BACK_MAIN_FILE_NAME = "MyColorsBack.py";
	private static string MY_COLORS_BACK_PATH;
	private static string BACK_LEARNING_DATA_GENERATION_COMMAND = "GenerateLearningData";
	private static string BACK_QUESTIONS_DATA_GENERATION_COMMAND = "GenerateQuestionsData";
	private static string UPDATED_QUESTIONS_DATA_FILE_NAME = "updated_questions_data.json";
	private static string UPDATED_QUESTIONS_DATA_PATH;
	private static string UPDATED_LEARNING_DATA_FILE_NAME = "updated_learning_data.json";
	private static string UPDATED_LEARNING_DATA_PATH;
	private static string PAST_ANSWERS_FILE_NAME = "past_answers.csv";
	private static string PAST_ANSWERS_DATA_PATH;
	private static string PAST_GAMES_DATA_FOLDER_NAME = @"Past Games";
	private static string PAST_GAMES_DATA_FOLDER_PATH;

	private static string APP_STREAMING_ASSETS_PATH;
		
	void Start () {
		allRoundsData = new ArrayList();
		learning = false;
		
		APP_STREAMING_ASSETS_PATH = Application.streamingAssetsPath;
		MY_COLORS_BACK_PATH = Path.Combine(Application.streamingAssetsPath, MY_COLORS_BACK_MAIN_FILE_NAME);
		UPDATED_QUESTIONS_DATA_PATH = Path.Combine(Application.streamingAssetsPath, UPDATED_QUESTIONS_DATA_FILE_NAME);
		UPDATED_LEARNING_DATA_PATH = Path.Combine(Application.streamingAssetsPath, UPDATED_LEARNING_DATA_FILE_NAME);
		PAST_ANSWERS_DATA_PATH = Path.Combine(Application.streamingAssetsPath, PAST_ANSWERS_FILE_NAME);
		PAST_GAMES_DATA_FOLDER_PATH = Path.Combine(Application.streamingAssetsPath, PAST_GAMES_DATA_FOLDER_NAME);
		
		DontDestroyOnLoad(gameObject);

		ClearPreviousCachedData();

		LoadGameData();	
		LoadPlayerProgress();
		IntializeNewGameDataFiles();

		SceneManager.LoadScene("MenuScreen");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKey("escape"))
			Application.Quit();
        
	}

	public void setPlayerProgressData(string name, int age, bool genderMale) {
		playerProgress.playerName = name;
		playerProgress.age = age;
		playerProgress.gender = genderMale;
		playerProgress.highestScore = 0;
	}
	private void ClearPreviousCachedData() {
		File.Delete(UPDATED_QUESTIONS_DATA_PATH);
		File.Delete(UPDATED_LEARNING_DATA_PATH);
		File.Delete(PAST_ANSWERS_DATA_PATH);
	}

	private void IntializeNewGameDataFiles() {
		CreateAnswersCsvFile();
		CreateLearnedDataCsvFile();
	}
	
	private static void RunMyColorsBack(string operation) {
		
		ProcessStartInfo backProcessStartInfo = new ProcessStartInfo(PYTHON_PATH);
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

	private void LoadLearningData() {

		File.Delete(UPDATED_LEARNING_DATA_PATH);
		RunMyColorsBack(BACK_LEARNING_DATA_GENERATION_COMMAND);

		if(File.Exists (UPDATED_LEARNING_DATA_PATH)) {
			string dataAsJson = File.ReadAllText(UPDATED_LEARNING_DATA_PATH);
			currentLearningData = JsonUtility.FromJson<LearningData>(dataAsJson);
		} else {
            UnityEngine.Debug.LogError("No learning data found.");
		}

	}

	private void LoadQuestionsData() {
		
		File.Delete(UPDATED_QUESTIONS_DATA_PATH);
		RunMyColorsBack(BACK_QUESTIONS_DATA_GENERATION_COMMAND);

		if(File.Exists (UPDATED_QUESTIONS_DATA_PATH)) {
			string dataAsJson = File.ReadAllText(UPDATED_QUESTIONS_DATA_PATH);
			RoundData loadedData = JsonUtility.FromJson<RoundData>(dataAsJson);
			allRoundsData.Add(loadedData);
			currentRound = loadedData;
		} else {
            UnityEngine.Debug.LogError("No round data found.");

		}

	}

	public void LoadGameData() {
		LoadLearningData();

		Thread loadingGameDataThread = new Thread(new ThreadStart(LoadQuestionsData));
		loadingGameDataThread.Start();
	}

	public void GameFinished(ArrayList roundAnswers) {
		currentRound = null;
		currentLearningData = null;
		AnswersToCSV(roundAnswers);
		LearningDataToCSV();
		SaveGameData();
	}

	private void SaveGameData() {
		string name = playerProgress.playerName;
		string age = playerProgress.age.ToString();
		string gender = playerProgress.gender.ToString();
		string score = playerProgress.highestScore.ToString();
		
		var csv = new StringBuilder();
		var csvLine = string.Format("Name,Age,Gender==Male,Score");
		csv.AppendLine(csvLine);

		csvLine = string.Format("{0},{1},{2},{3}", name, age, gender, score);
		csv.AppendLine(csvLine);

		csvLine = string.Format("ColorName,Red,Blue,Green,timeToLearn");
		csv.AppendLine(csvLine);


		foreach (ColorToLearnData colorData in currentLearningData.colorsToLearn) {
			string colorName = colorData.colorName;
			byte [] colorRGB = colorData.colorRGB;
			float timeToLearn = colorData.timeToLearn;
			csvLine = string.Format("{0},{1},{2},{3}",
				colorName,colorRGB[0], colorRGB[1], colorRGB[2], timeToLearn);
			csv.AppendLine(csvLine);  
		}

		int nextInt = PlayerPrefs.GetInt("gameID") + 1;
		PlayerPrefs.SetInt("gameID", nextInt);

		string this_game_file_name = name + "_" + age + "_" + PlayerPrefs.GetInt("gameID");  
		string this_game_data_path = Path.Combine(PAST_GAMES_DATA_FOLDER_PATH, this_game_file_name);
		File.WriteAllText(this_game_data_path, csv.ToString());

	}

	private void CreateAnswersCsvFile() {
		var csv = new StringBuilder();
		var newLine = string.Format("ColorName,Red,Blue,Green,isCorrect");
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

	private void CreateLearnedDataCsvFile() {
		var csv = new StringBuilder();
		var newLine = string.Format("ColorName,Red,Blue,Green,timeToLearn");
		csv.AppendLine(newLine);  
		File.WriteAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
	}

	private void LearningDataToCSV() {
		var csv = new StringBuilder();
		var newLine = ""; 
		foreach (ColorToLearnData colorData in currentLearningData.colorsToLearn) {
			string colorName = colorData.colorName;
			byte [] colorRGB = colorData.colorRGB;
			float timeToLearn = colorData.timeToLearn;
			newLine = string.Format("{0},{1},{2},{3}",
				colorName,colorRGB[0], colorRGB[1], colorRGB[2], timeToLearn);
			csv.AppendLine(newLine);  
		}

		File.AppendAllText(PAST_ANSWERS_DATA_PATH, csv.ToString());
	}

	public LearningData GetCurrentLearningData() {
		return currentLearningData;
	}

	public RoundData GetCurrentRoundData() {
		return currentRound;
	}

	public bool isRoundDataLoaded() {
		return currentRound != null;
	}

	public bool isLearningDataLoaded() {
		return currentLearningData != null;
	}

	public void SubmitNewPlayerScore(int newScore) {
		
		if(newScore > playerProgress.highestScore) {
			playerProgress.highestScore = newScore;
			SavePlayerProgress();
		}

	}
	
	public int GetHighestScore() {
		return playerProgress.highestScore;
	}

	public bool isLearning() {
		return learning;
	}
	
	public void setLearning(bool learningOrNot) {
		learning = learningOrNot;
	}

	private void LoadPlayerProgress() {
		playerProgress = new PlayerProgress();
		if(PlayerPrefs.HasKey("highestScore")) {
			playerProgress.highestScore = PlayerPrefs.GetInt("highestScore");
		} else {
			playerProgress.highestScore = 0;
		}
	}

	private void SavePlayerProgress() {
		PlayerPrefs.SetInt("highestScore", playerProgress.highestScore);
	}
	
}
