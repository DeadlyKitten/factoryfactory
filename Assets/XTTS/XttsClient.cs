using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;

namespace Xtts
{
    public static class XttsClient
    {
        private static string baseURL = "http://localhost:8020";

        private const string ENDPOINT = "/tts_to_audio/";
        private const string JSON_CONTENT_TYPE = "application/json";
        private const string HTTP_POST = "POST";

        private const string DEFAULT_SPEAKER = "stanley";
        private const string DEFAULT_LANGUAGE = "en";

        private const AudioType AUDIO_TYPE = AudioType.WAV;

        public static void SetBaseURL(string url) => baseURL = url;

        public static async UniTask<AudioClip> GenerateTTS(string text, CancellationToken cancellationToken, string speaker = DEFAULT_SPEAKER, string language = DEFAULT_LANGUAGE)
        {
            var request = new XttsRequest
            {
                text = text,
                speaker_wav = speaker,
                language = language
            };

            var requestPayload = JsonConvert.SerializeObject(request);

            return await DoUnityPostRequest(requestPayload, cancellationToken);
        }

        private static async UniTask<WebResponse> DoPostRequest(string requestPayload, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            HttpWebRequest httpWebRequest;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create($"{baseURL}{ENDPOINT}");
                httpWebRequest.ContentType = JSON_CONTENT_TYPE;
                httpWebRequest.Method = HTTP_POST;

                using var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                await streamWriter.WriteAsync(requestPayload);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
                return default;
            }

            return await httpWebRequest.GetResponseAsync();
        }

        private static async UniTask<AudioClip> DoUnityPostRequest(string requestPayload, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var url = $"{baseURL}{ENDPOINT}";

            using var downloadHandler = new DownloadHandlerAudioClip(url, AUDIO_TYPE);
            using var uploadHandler = new UploadHandlerRaw(Encoding.ASCII.GetBytes(requestPayload));
            using UnityWebRequest unityWebRequest = new UnityWebRequest(url, HTTP_POST, downloadHandler, uploadHandler); ;

            uploadHandler.contentType = JSON_CONTENT_TYPE;

            await unityWebRequest.SendWebRequest().WithCancellation(cancellationToken);

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(unityWebRequest.error);
                return default;
            }

            await UniTask.WaitUntil(() => downloadHandler.isDone, cancellationToken: cancellationToken);

            if (!String.IsNullOrEmpty(downloadHandler.error))
            {
                Debug.LogError(downloadHandler.error);
                return default;
            }

            return downloadHandler.audioClip;
        }

        private static async UniTask<AudioClip> GetAudioClipFromResponse(WebResponse webResponse, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            using var responseStream = webResponse.GetResponseStream();

            var tempPath = Path.GetTempFileName();
            using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
            await responseStream.CopyToAsync(fs);
            fs.Close();

            var loader = UnityWebRequestMultimedia.GetAudioClip(tempPath, AUDIO_TYPE);
            await loader.SendWebRequest();

            var audioClip = DownloadHandlerAudioClip.GetContent(loader);
            audioClip.name = Path.GetFileName(tempPath);

            File.Delete(tempPath);

            return audioClip;
        }

        public class XttsRequest
        {
            public string text;
            public string speaker_wav;
            public string language;
        }
    }
}
