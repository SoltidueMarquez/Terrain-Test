using UnityEngine;

namespace GPT
{
    public class StepPrompt : MonoBehaviour
    {
        [SerializeField, TextArea(10, 100)] protected string prompt = "";

        protected void SendRequest()
        {
            
        }
    }
}