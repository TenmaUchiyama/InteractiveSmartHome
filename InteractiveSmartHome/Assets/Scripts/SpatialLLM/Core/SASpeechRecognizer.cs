using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;

using Oculus.VoiceSDK.UX;
using System;
using TMPro;
using UnityEngine.Events;
using SpatialLLM.Network;
using Meta.WitAi.Data;


namespace SpatialLLM.Core
{
public class SASpeechRecognizer : Singleton<SASpeechRecognizer>
    {
        
        public UnityEvent OnVoiceRecogStart; 
        public UnityEvent OnVoiceRecogStop; 

        public UnityEvent<string> OnVoiceRecognized;




        [SerializeField] private VoiceService _voiceService;
        

        private string _activateText = "Activate";

        [SerializeField] private bool _activateImmediately = false;
        
        private string _deactivateText = "Deactivate";


        [SerializeField] private bool _deactivateAndAbort = false;


        [SerializeField] GameObject recordingIndicator;

        private VoiceServiceRequest _request;
        private bool _isActive = false;
        public bool IsActive => _isActive;


       private void Awake()
{
    if (_voiceService == null)
    {
        _voiceService = FindObjectOfType<VoiceService>();
        if (_voiceService == null)
        {
            Debug.LogError("VoiceService not found in the scene.");
        }
    }

    // Platform Integrations を無効化（Unityマイクを使うため）
    if (_voiceService != null)
    {
        _voiceService.UsePlatformIntegrations = false;
    }
}

private void SetMicWhenReady()
{
    string preferredMicName = "マイク (USBAudio1.0)";
    string selectedMic = null;

    float timeout = 5f;
    float elapsed = 0f;

    while (Microphone.devices.Length == 0 && elapsed < timeout)
    {
        System.Threading.Thread.Sleep(100); // 100ms待機
        elapsed += 0.1f; // 0.1秒を加算
    }

    if (Microphone.devices.Length == 0)
    {
        Debug.LogWarning("マイクが検出できませんでした（5秒待っても見つからない）");
        return;
    }

    foreach (var mic in Microphone.devices)
    {
        if (mic.Contains(preferredMicName))
        {
            selectedMic = mic;
            break;
        }
    }

    if (string.IsNullOrEmpty(selectedMic))
    {
        selectedMic = Microphone.devices[0];
        Debug.LogWarning($"指定されたマイクが見つかりませんでした。代わりに {selectedMic} を使用します。");
    }
    
    
    

    if (!_voiceService.UsePlatformIntegrations &&
                                AudioBuffer.Instance != null &&
                                AudioBuffer.Instance.MicInput is Meta.WitAi.Lib.Mic micInput)
            {
                int micIndex = Array.IndexOf(Microphone.devices, selectedMic);
                if (micIndex >= 0)
                {
                    micInput.ChangeMicDevice(micIndex);
                    Debug.Log($"使用マイクを設定しました: {selectedMic} (Index: {micIndex})");
                }
            }
}


 private void Start()
{
            SetMicWhenReady();

    if (_voiceService != null)
    {
        _voiceService.VoiceEvents.OnStartListening.AddListener(OnStartListening);
        _voiceService.VoiceEvents.OnStoppedListening.AddListener(OnStopListening);
        _voiceService.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
    }
}



    private void OnDisable()
        {
            if (_voiceService != null)
            {
                _voiceService.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
                _voiceService.VoiceEvents.OnStoppedListening.RemoveListener(OnStopListening);
                _voiceService.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);

            }

            // Reset state
            _isActive = false;
            _request = null;
        }


        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         Debug.Log("<color=green>Space Button Pressed </color>");
        //         ToggleVoiceActivation();
        //     }
        // }

   
        public void ToggleVoiceActivation()
        {
            if (!_isActive)
            {


                if(LLMQueryRequest.Instance.IsRequesting) return;
                Activate();
            }
            else
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Public method to activate the voice service
        /// </summary>
        public void ActivateVoice()
        {
            if (!_isActive)
            {
                Activate();
            }
        }

        /// <summary>
        /// Public method to deactivate the voice service
        /// </summary>
        public void DeactivateVoice()
        {
            if (_isActive)
            {
                
                Deactivate();
            }
        }

        /// <summary>
        /// Activate the voice service based on settings
        /// </summary>
        private void Activate()
        {
            if (_voiceService == null)
            {
                Debug.LogError("VoiceService is not assigned.");
                return;
            }



            if (!_activateImmediately)
            {
                _request = _voiceService.Activate(new WitRequestOptions(), new VoiceServiceRequestEvents());
                
            }
            else
            {
                _request = _voiceService.ActivateImmediately(new WitRequestOptions(), new VoiceServiceRequestEvents());
                
            }
        }

        /// <summary>
        /// Deactivate the voice service based on settings
        /// </summary>
        private void Deactivate()
        {
            if (_voiceService == null)
            {
                Debug.LogError("VoiceService is not assigned.");
                return;
            }



            if (!_deactivateAndAbort)
            {
                _voiceService.Deactivate();
                if (_request != null)
                {
                    _request.DeactivateAudio();
                }
                else
                {
                    _voiceService.Deactivate();
                }


            }
            else
            {
                if (_request != null)
                {
                    _request.Cancel();
                }

            }
        }

        /// <summary>
        /// Callback when voice service starts listening
        /// </summary>
        private void OnStartListening()
        {
            OnVoiceRecogStart.Invoke();
            if (recordingIndicator != null)
            {
                recordingIndicator.SetActive(true);
            }
            Debug.Log("<color=yellow>Recognition Started...</color>");
            _isActive = true;
        }

        /// <summary>
        /// Callback when voice service stops listening
        /// </summary>
        private void OnStopListening()
        {
            OnVoiceRecogStop.Invoke();
            if (recordingIndicator != null)
            {
                recordingIndicator.SetActive(false);
            }
            Debug.Log("<color=yellow>Recognition Stopped...</color>");
            _isActive = false;
            _request = null;
        }


    private void OnFullTranscription(string detectedMsg)
    {
        Debug.Log($"<color=yellow>[SpeedRecognizer] Recognized: {detectedMsg}</color>");
        OnVoiceRecognized.Invoke(detectedMsg);
    }


    
    }

}