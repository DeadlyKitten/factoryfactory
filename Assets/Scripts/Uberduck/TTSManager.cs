using System;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
<<<<<<< Updated upstream:Assets/Scripts/Uberduck/UberduckManager.cs
using System.Collections;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
=======
using Xtts;
>>>>>>> Stashed changes:Assets/Scripts/Uberduck/TTSManager.cs

public class TTSManager : MonoBehaviour
{
    public static TTSManager Instance;
    public string voiceName = "";

    [SerializeField]
    private int _maxClipGenerationTries = 2;

    CancellationTokenSource _cancellationTokenSource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }

    private void Start() => XttsClient.SetBaseURL("http://steven-factory.local:8020");

    private void OnApplicationQuit() => _cancellationTokenSource?.Cancel();

    public async Task<AudioClip> GenerateAudioClip(string text)
    {
        if (text == null || !Application.isPlaying)
        {
            return null;
        }

        text = FixMispronouncedWords(text);

        //UberduckGeneratedResult voice = null;
        int tryCounter = 0;

        //do
        //{
        //    try
        //    {
        //        // This is where the HttpRequestExceptions happen
        //        voice = await _client.GenerateVoiceAsync(text, voiceName);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogWarning("Caught error from Uberduck API when generating voice:");
        //        Debug.LogError(e);
        //        tryCounter++;
        //        await Task.Delay(1000);
        //    }
        //} while (voice == null && tryCounter < _maxClipGenerationTries);

        //if (voice == null)
        //{
        //    Debug.LogError($"Failed to generate audio clip {_maxClipGenerationTries} times! Skipping.");
        //    return null;
        //}

        string audioPath = null;

        //while (true)
        //{
        //    try
        //    {
        //        var audioStream = await voice.GetDeserializedAudioData();
        //        if (audioStream.Path != null)
        //        {
        //            audioPath = audioStream.Path;
        //            break;
        //        }
        //        await Task.Delay(1000);
        //    } catch (Exception e)
        //    {
        //        Debug.LogWarning("Caught exception trying to GetDeserializedAudioData from Uberduck API");
        //        Debug.LogError(e);
        //        break;
        //    }
        //}

        if (audioPath == null)
        {
            return null;
        }

        return await GetAudioClip(audioPath);
    }

    public async Task<AudioClip> GetAudioClip(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            var result = www.SendWebRequest();

            while (!result.isDone) { await Task.Delay(50); }

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                return myClip;
            }
            else
            {
                Debug.LogError(www.error);
                return null;
            }
        }
    }

    public async Task<bool> CheckIfVoiceExists(string voiceID)
    {
        // TODO: Use the actual voice endpoint to check if the voice exists.
        // Currently this tries to generate using the voice, and if it fails it assumes the voice doesn't exist.
        //try
        //{
        //    // This is where the HttpRequestExceptions happen
        //    await _client.GenerateVoiceAsync("Testing.", voiceID);
        //    return true;
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError(e);
        //    return false;
        //}

        return false;
    } 

    private IEnumerator GetAudioclipFromPath(string path, Action<AudioClip> callback)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(null);
            }
            else
            {
                //Debug.Log("Got audio clip!");
                callback(DownloadHandlerAudioClip.GetContent(www));
            }
        }
    }

    private string FixMispronouncedWords(string text)
    {
        // Should translate all acronyms instead of doing this manually
        text = Regex.Replace(text, @"(\b[3][Dd]\b)", "three dee"); // "3D" is pronounced "trid" by uberduck
        text = Regex.Replace(text, @"(\b[rR][gG][bB]\b)", "are gee bee"); // uberduck says "grab"
        text = Regex.Replace(text, @"(\b[vV][rR]\b)", "vee are");
        text = Regex.Replace(text, @"(\b[aA][iI]\b)", "ayy eye");
        text = Regex.Replace(text, @"(\b[bB][bB][qQ]\b)", "barbecue");

        return text;
    }
}