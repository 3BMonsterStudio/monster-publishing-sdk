using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AdsInterBehavior : MonoBehaviour
{
    public int positionId;
    public UnityEvent closeEvent;
    public Button button;
    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowInter);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(ShowInter);
    }

    public void ShowInter()
    {
        AdsController.Instances.ShowInterstitial(
            () => {
                closeEvent.Invoke();
            }, 
            positionId);
    }
}
