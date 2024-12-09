using System.Collections;
using System.Collections.Generic;
using M2MqttUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace MRFlow.Network
{
    public class MRMqttController : M2MqttUnityClient
    {

    public UnityAction OnConnectionCompleted; 
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
            OnConnectionCompleted?.Invoke();
        }



        protected override void OnConnectionFailed(string errorMessage)
        {
            Debug.Log($"<color=yellow>[MqttConnector] Connection failed: {errorMessage}</color>");
        }



        public void SubscribeTopic(string node_name, string action_id, UnityAction<string> callback)
        {
           
            string topic = "mrflow/" + action_id; 
            if (!topicSubscribers.ContainsKey(topic))
            {
                topicSubscribers[topic] = callback;
                // Debug.Log($"<color=yellow>[MqttConnector] {this.client != null}</color>");
                client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                Debug.Log($"<color=yellow>[MqttConnector {node_name}] Subscribed to {topic}</color>");
            
            }else{
                Debug.Log($"<color=red>[MqttConnector {node_name}] Topic {topic} already subscribed</color>");
            }
        }

        public void UnsubscribeTopic(string action_id)
        {
            string topic = "mrflow/" + action_id; 
            if (topicSubscribers.ContainsKey(topic))
            {
                topicSubscribers.Remove(topic);
                client.Unsubscribe(new string[] { topic });
                Debug.Log($"<color=yellow>[MqttConnector] Unsubscribed from {topic}</color>");
            }else{
                Debug.Log($"<color=red>[MqttConnector] Topic {topic} not subscribed</color>");
            }
        }

    

    }
}