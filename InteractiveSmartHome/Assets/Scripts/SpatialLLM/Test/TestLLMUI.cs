using System;
using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestLLMUI : MonoBehaviour
{

    
    [SerializeField] public Button button; 
    [SerializeField] public TextMeshProUGUI inputText; 
    [SerializeField] private LLMQueryRequest lLMQueryRequest;





    void Start()
    {


        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        TestDeviceManager.Instance.ResetAllColor();
       lLMQueryRequest.SendQuery(inputText.text);
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
