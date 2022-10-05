using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickLog : MonoBehaviour
{
    public string nameScreen;
    public string nameButton;
    public void LogEventButtonClick()
    {
        //Debug.Log("click ");
        Scene scene = SceneManager.GetActiveScene();
        GameAnalytics.LogEventButton(nameScreen, nameButton);
    }
}
