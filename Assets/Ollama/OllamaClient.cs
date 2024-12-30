using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Ollama
{
    public class OllamaClient
    {
        private const string SERVER = "http://localhost:11434/";

        /// <summary>
        /// Generate a simple response from prompt <i>(no context/history)</i>
        /// </summary>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
        /// <param name="images">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
        /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
        /// <returns>response string from the LLM</returns>
        public static async Task<string> Generate(string model, string prompt, KeepAlive keep_alive = KeepAlive.UnloadImmediately)
        {
            Debug.Log("Sending request to Ollama server...");
            Debug.Log("Prompt: " + prompt);

            var request = new OllamaRequest(model, prompt, null, false, keep_alive);
            string requestPayload = JsonConvert.SerializeObject(request);
            var result = await PostRequest<OllamaResponse>(requestPayload, Endpoints.GENERATE);
            return result.response;
        }

        private static async Task<T> PostRequest<T>(string requestPayload, string endpoint)
        {
            HttpWebRequest httpWebRequest;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync());
                await streamWriter.WriteAsync(requestPayload);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
                return default;
            }

            var httpResponse = await httpWebRequest.GetResponseAsync();
            using var streamReader = new StreamReader(httpResponse.GetResponseStream());

            string result = await streamReader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
