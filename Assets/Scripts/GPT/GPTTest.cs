using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GPT
{
    public class GptTest : MonoBehaviour
    {
        public static GptTest Instance;

        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private string apiUrl = "";
        [SerializeField] private string apiKey = "";
        [SerializeField] private string currentModel = "";

        [SerializeField, Multiline] private string prompt = "";
        [SerializeField] private KeyCode sendKey;
        
        private string result = string.Empty;

        private IEnumerator SendRequest()
        {
            Debug.Log("Ask: " + prompt);
        
            OpenAIRequestData requestData = new OpenAIRequestData()
            {
                messages = new Message[]
                {
                    new Message { role = "user", content = prompt }
                },
                stream = false,
                model = currentModel,
                temperature = 0,
                presence_penalty = 2
            };

            string jsomBody = JsonUtility.ToJson(requestData);

            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsomBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Response response = JsonUtility.FromJson<Response>(request.downloadHandler.text);
                string content = response.choices[0].message.content;

                Debug.Log("Response: "+ content);
            }
            else
            {
                Debug.Log("Error: "+ request.error);
            }
        }

        #region 设置方法
        public void SendARequest()
        {
            StartCoroutine(SendRequest());
        }

        public void Stop()
        {
            StopCoroutine(SendRequest());
        }

        public void SetApiKey(string str)
        {
            apiKey = str;
        }

        public void SetUrl(string str)
        {
            apiUrl = str;
        }

        public void SetCurrentModel(string str)
        {
            currentModel = str;
        }
    
        public void SetPrompt(string str)
        {
            prompt = str;
        }
        #endregion

        private void Update()
        {
            if (Input.GetKeyDown(sendKey))
            {
                SendARequest();
            }
        }
    }

    #region 数据请求类

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class OpenAIRequestData
    {
        public Message[] messages;
        public bool stream;
        public string model;
        public float temperature;
        public float presence_penalty;
    }

// 定义 Response 类
    [System.Serializable]
    public class Response
    {
        public string id;
        public string @object;
        public long created;
        public string model;
        public Choice[] choices;
    }

// 定义 Choice 类
    [System.Serializable]
    public class Choice
    {
        public int index;
        public Message message;
    }
    #endregion
}