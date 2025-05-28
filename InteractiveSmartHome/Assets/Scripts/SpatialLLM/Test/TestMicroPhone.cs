using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMicroPhone : MonoBehaviour
{
     private AudioClip micClip;
    private string micDevice;
    private int sampleRate = 16000;
    private bool isRecording = false;
    private int lastSamplePos = 0;

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            Debug.Log($"マイクデバイス: {micDevice}");
        }
        else
        {
            Debug.LogError("マイクデバイスが見つかりません");
        }
    }

    void Update()
    {
        // トリガーを離したときに録音を開始
        if (!isRecording && OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
            StartRecording();
        }

        if (isRecording)
        {
            LogMicLevel();
        }
    }

    void StartRecording()
    {
        micClip = Microphone.Start(micDevice, true, 10, sampleRate); // 10秒バッファ
        isRecording = true;
        lastSamplePos = 0;
        Debug.Log("🎙 録音開始");
    }

    void LogMicLevel()
    {
        if (micClip == null) return;

        int currentPos = Microphone.GetPosition(micDevice);
        int samplesToRead = currentPos - lastSamplePos;
        if (samplesToRead < 0) samplesToRead += micClip.samples;

        float[] samples = new float[samplesToRead];
        micClip.GetData(samples, lastSamplePos);

        float level = 0f;
        foreach (float sample in samples)
        {
            level += Mathf.Abs(sample);
        }
        level /= samples.Length;

        Debug.Log($"🔊 マイク音レベル: {level:F4}");

        lastSamplePos = currentPos;
    }

    void OnDisable()
    {
        if (isRecording)
        {
            Microphone.End(micDevice);
            isRecording = false;
            Debug.Log("🛑 録音停止");
        }
    }
}
