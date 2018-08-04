using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class LoadingController : MonoBehaviour {

	public Image loadingPanel;
	public Text loadingText;
	public Text loadingDetails;

	private DataController dataController;
	private bool isRecentUpdated = false;
	private string recentUpdate;
	
	// Use this for initialization
	void Start () {
		dataController = FindObjectOfType<DataController>();
	}

	// Update is called once per frame
	void Update () {
		if(!dataController.loadingData) {
			UnityEngine.Debug.Log("About to load to menu screen.");
			SceneManager.LoadScene("MenuScreen");
		} else {
			Color loadingTextColor = loadingText.color;
			loadingText.color =
					new Color(loadingTextColor.r, loadingTextColor.g, loadingTextColor.b, Mathf.PingPong(Time.time, 1));

			Color loadingDetailsColor = loadingDetails.color;
			loadingDetails.color =
					new Color(loadingDetailsColor.r, loadingDetailsColor.g, loadingDetailsColor.b, Mathf.PingPong(Time.time, 10));
			if(isRecentUpdated) {
				isRecentUpdated = false;
				loadingDetails.text = recentUpdate;
			}

		}
		
	}

	public void TrackData(string msg) {
		recentUpdate = msg;
		isRecentUpdated = true;
	}
}
