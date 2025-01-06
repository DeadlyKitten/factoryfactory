using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Ollama
{
    public class OllamaClient
    {
        private const string SERVER = "http://steven-factory.local:11434/";

        /// <summary>
        /// Generate a simple response from prompt <i>(no context/history)</i>
        /// </summary>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
        /// <param name="images">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
        /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
        /// <returns>response string from the LLM</returns>
        public static async UniTask<string> Generate(string model, string prompt, CancellationToken cancellationToken, KeepAlive keep_alive = KeepAlive.UnloadImmediately)
        {
            Debug.Log("Sending request to Ollama server...\nPrompt: " + prompt);

            var request = new OllamaRequest(model, prompt, null, false, keep_alive);
            string requestPayload = JsonConvert.SerializeObject(request);
            var result = await UnityPostRequest<OllamaResponse>(requestPayload, Endpoints.GENERATE, cancellationToken);
            return result.Response;
        }

        private static async UniTask<T> PostRequest<T>(string requestPayload, string endpoint)
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

        private static async UniTask<T> UnityPostRequest<T>(string requestPayload, string endpoint, CancellationToken cancellationToken)
        {
            using var downloadHandler = new DownloadHandlerBuffer();
            using var uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(requestPayload));
            using var unityWebRequest = new UnityWebRequest($"{SERVER}{endpoint}", "POST", downloadHandler, uploadHandler);

            uploadHandler.contentType = "application/json";
                
            await unityWebRequest.SendWebRequest().WithCancellation(cancellationToken);

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(unityWebRequest.error);
                return default;
            }

            return JsonConvert.DeserializeObject<T>(downloadHandler.text);
        }
    }
}
