using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.Network;
using Newtonsoft.Json;
using SpatialLLM.Type;
using UnityEngine;
using static SpatialLLM.Network.NetworkDataType;


namespace SpatialLLM.Device{
public class SACurtain : SADevice
{


   private SADeviceType sADeviceType = SADeviceType.Curtain;


    [Range(0f, 100f)]
    public float value ;
    public Transform curtainVisual; // CurtainVisualオブジェクトをここにアサイン

    private Vector3 closedPosition = new Vector3(0f, 0f, 0f);
    private Vector3 openedPosition = new Vector3(2.65f, 0f, 0f);

    private Coroutine currentCoroutine = null;

    public float lerpDuration = 1f; // 開閉にかかる時間（秒）

    private bool isOpen = false; // 現在の状態


   private void Awake() {


            this.saDeviceType = SADeviceType.Curtain;
            string id = Guid.NewGuid().ToString();

            this.deviceData = new DBDeviceData(
                id,
                this.gameObject.name,
                "curtain",
                "This is a curtain device. You need this when you want to open or close the curtain. You can specify the value of the curtain by specifying the value. 0 is fully open, 100 is closed.",
                "device/" + id,
                this.transform.position
            );

            this.spatialData = new DeviceSpatialData(id , gameObject.name, transform.position, Vector3.Distance(transform.position, Camera.main.transform.position));
            // Debug.Log($"[SACurtain]{this.spatialData}");

        }

    private void Start() {


        this.currentOperatingData.intensity = (int)value;
         MRMqttController.Instance.OnConnectionCompleted += () => {
            MRMqttController.Instance.SubscribeDeviceTopic(this.deviceData.device_name, this.deviceData.product_topic,  OnReceiveMsgFromServer);
        };
    }

        private void OnReceiveMsgFromServer(string payload)
        {
          OperatingDeviceData operatingDeviceData = JsonConvert.DeserializeObject<OperatingDeviceData>(payload);


           if(operatingDeviceData.intensity.HasValue){
            OperateDevice(operatingDeviceData);
           }

           
        }

          public override void OperateDevice(OperatingDeviceData operatingDeviceData )
        {
              this.currentOperatingData = operatingDeviceData;
              SetOpenCloseValue(operatingDeviceData.intensity.Value);
        }

        public void SetOpenCloseValue(int value)
    {
        // 0 (完全に開く) から 100 (完全に閉じる) の値を受け取り、スムーズに位置を設定
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        float clampedValue = Mathf.Clamp(value, 0, 100);
        Vector3 targetPosition = Vector3.Lerp(openedPosition, closedPosition, clampedValue / 100f);
        currentCoroutine = StartCoroutine(LerpCurtain(curtainVisual.localPosition, targetPosition));
    }

    public void Open()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(LerpCurtain(curtainVisual.localPosition, openedPosition));
        isOpen = true;
    }

    public void Close()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(LerpCurtain(curtainVisual.localPosition, closedPosition));
        isOpen = false;
    }

    private IEnumerator LerpCurtain(Vector3 start, Vector3 end)
    {
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            curtainVisual.localPosition = Vector3.Lerp(start, end, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最終位置を設定
        curtainVisual.localPosition = end;
    }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void TurnOnWithColor(Color color)
        {
            throw new NotImplementedException();
        }

        public override void TurnOff()
        {
            throw new NotImplementedException();
        }
    }
}
