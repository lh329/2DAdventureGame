using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameSceneManager : MonoBehaviour
{
    public Transform startPosTr;
    public string nextNSceneame;
    public bool limitPlayerRightX;
    private void Start()
    {
        if (Player.instance)
        {
            Player.instance.minX = Camera.main.ViewportToWorldPoint(new Vector3(0,1)).x;
            Player.instance.LimitRightX = limitPlayerRightX;
            Player.instance.maxX = Camera.main.ViewportToWorldPoint(new Vector3(1,1)).x;
            Player.instance.transform.position = startPosTr.position;
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextNSceneame);
    }
    
}
