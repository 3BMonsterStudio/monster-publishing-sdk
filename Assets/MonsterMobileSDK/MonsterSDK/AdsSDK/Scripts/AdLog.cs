using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdLog : MonoBehaviour
{
    public static bool initializeDone = false;

    private static Dictionary<string, Text> adsInstacesDict;
    private static Text _adsInstances1;
    private static Text _adsInstances2;
    private static Text _statusText;

    public Text mediation1, mediation2, statusText;

    private static bool toastShowing = false;
    private Coroutine hideToast = null;

    private void Awake()
    {
#if ENV_PROD
Destroy(gameObject);
#endif

    }
    private void OnEnable()
    {
        _adsInstances1 = mediation1;
        _adsInstances2 = mediation2;
        _statusText = statusText;
        adsInstacesDict = new Dictionary<string, Text>();
        _adsInstances1.gameObject.SetActive(false);
        _adsInstances2.gameObject.SetActive(false);
        _statusText.gameObject.SetActive(false);

        initializeDone = true;
        Debug.Log("Init ADLog Done");
    }

    private void Update()
    {
        if (toastShowing)
        {
            if (hideToast != null)
            {
                StopCoroutine(hideToast);
                hideToast = null;
            }
            hideToast = StartCoroutine(AdsController.StartAction(() =>
            {
                _statusText.gameObject.SetActive(false);
                hideToast = null;
            }, 1.5f));
            toastShowing = false;
        }
    }

    public static void ActivateAdList(string source, int adListIndex)
    {
        if (!initializeDone) return;

        if (adListIndex == 0)
        {
            _adsInstances1.gameObject.SetActive(true);
            if (!adsInstacesDict.ContainsKey(source))
                adsInstacesDict.Add(source, _adsInstances1);
        }
        if (adListIndex == 1)
        {
            _adsInstances2.gameObject.SetActive(true);
            if (!adsInstacesDict.ContainsKey(source))
                adsInstacesDict.Add(source, _adsInstances2);
        }


    }

    public static void ShowMedText(string source, string message)
    {
        if (!initializeDone) return;
        if (adsInstacesDict.ContainsKey(source))
            adsInstacesDict[source].text = message;
    }

    public static void DisplayToast(string msg)
    {
        if (!initializeDone) return;

        _statusText.text = msg;
        _statusText.gameObject.SetActive(true);
        toastShowing = true;
    }
}
