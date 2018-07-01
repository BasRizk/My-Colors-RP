using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackFader : MonoBehaviour {

	public Animator animator;
	private string sceneToLoad;

	public void FadeToScreen(string sceneName) {
		sceneToLoad = sceneName;
		animator.SetTrigger("FadeOut");
	}

	public void OnFadeComplete() {
		SceneManager.LoadScene(sceneToLoad);
	}
}
