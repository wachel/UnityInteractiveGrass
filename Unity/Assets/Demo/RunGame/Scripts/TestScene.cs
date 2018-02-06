using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TestScene : MonoBehaviour
{
    void Start ()
    {
        GameObject prefab = (GameObject)Resources.Load("Characters/JianXian_Man");
        GameObject obj = GameObject.Instantiate(prefab);
        obj.transform.position = transform.position;
	}
}
