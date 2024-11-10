using System.Collections;
using System.Collections.Generic;
using M2MqttUnity;
using UnityEngine;
using UnityEngine.Events;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace MRFlow.Network
{
    public class MRMqttController : M2MqttUnityClient
    {
    private static MRMqttController instance;
    public static MRMqttController Instance 
    {
        get
        {
            
            if (instance == null)
            {
                SetupInstance();
            }
            return instance;
        }
    }

        
    protected override void Awake()
    {
        base.Awake();
        SetupInstance();
    }
    private static void SetupInstance()
    {
        instance = FindObjectOfType<MRMqttController>();
	
        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = "MRMqttController";
            instance = gameObj.AddComponent<MRMqttController>();
            DontDestroyOnLoad(gameObj);
        }
    }
    private List<string> eventMessages = new List<string>();

    private Dictionary<string, UnityAction<string>> topicSubscribers = new Dictionary<string, UnityAction<string>>();  

     protected override void Start() {
        base.Start();
        base.Connect();
        Debug.Log($"<color=yellow>[MqttConnector] Start</color>");
    }


    private void ProcessMessage(string msg)
    {
        Debug.Log($"<color=red>[MqttConnector] Received: {msg}</color>");
    }

     


    protected override void Update() {
        base.Update();



        
            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
    }



    protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log($"<color=red>[MqttConnector] Received: {msg}</color>");   

              if (topicSubscribers.ContainsKey(topic))
            {
                topicSubscribers[topic]?.Invoke(msg);
            }
        }


      protected override void OnConnecting()
        {
            base.OnConnecting();
            Debug.Log($"<color=yellow>[MqttConnector] Connecting to broker on {brokerAddress}:{brokerPort}...</color>");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Debug.Log($"<color=yellow>[MqttConnector] Connected to broker on {brokerAddress}</color>");
        }



        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.Log($"<color=yellow>[MqttConnector] Connection failed: {errorMessage}</color>");
        }



        public void SubscribeTopic(string action_id, UnityAction<string> callback)
        {
           
            string topic = "mrflow/" + action_id; 
            if (!topicSubscribers.ContainsKey(topic))
        {
            topicSubscribers[topic] = callback;
            Debug.Log($"<color=yellow>[MqttConnector] {this.client != null}</color>");
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Debug.Log($"<color=yellow>[MqttConnector] Subscribed to {topic}</color>");
          
        }else{
            Debug.Log($"<color=red>[MqttConnector] Topic {topic} already subscribed</color>");
        }
        }

      

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { "unity/test" });
        }
        

    }
}