using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;



public class StartGame : MonoBehaviour
{
    float rx;
    float ry;
    int designedWidth = 600;
    int designedHeight = 350;

    void Start ()
    {
        rx = Screen.width / (float)designedWidth;
        ry = Screen.height / (float)designedHeight;

        DontDestroyOnLoad(gameObject);
	}

    IEnumerator LoadLevel(string sceneName)
    {
        yield return null;
    }

    public void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(rx, ry, 1));
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != SceneManager.GetActiveScene()) {
                if (GUILayout.Button(scene.name)) {
                    StartCoroutine(LoadLevel(scene.name));
                }
            }
        }
    }

    void Update () {
	
	}
}
