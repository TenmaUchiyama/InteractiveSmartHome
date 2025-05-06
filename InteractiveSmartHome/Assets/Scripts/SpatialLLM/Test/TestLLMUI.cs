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
    [SerializeField] private LLMQueryRequest llmQueryRequest;





    void Start()
    {


        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        TestDeviceManager.Instance.ResetAllColor();
       llmQueryRequest.SendQuery(inputText.text);
    }




    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("<color=green>[TestLLMUI] Enter Button Pressed </color>");
             llmQueryRequest.SendQuery(inputText.text);
        }
    }
}
