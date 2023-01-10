using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class TextToSpeech : MonoBehaviour
{

    public string subKey;
    // TTS service region
    public string region;
    // TTS resource name
    public string resourceName;

    // token needed to access the TTS API
    private string accessToken;
    // audio source to play the TTS voice
    private AudioSource ttsSource;

    public static TextToSpeech instance;

    void Awake ()
    {
        // set the instance
        instance = this;
        // get the audio source
        ttsSource = GetComponent<AudioSource>();
    }

    IEnumerator GetAccessToken ()
    {

        // create a web request and set the method to POST
        UnityWebRequest webReq = new UnityWebRequest(string.Format("https://{0}.api.cognitive.microsoft.com/sts/v1.0/issuetoken", region));
        webReq.method = UnityWebRequest.kHttpVerbPOST;
        // create a download handler to receive the access token
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // set the header
        webReq.SetRequestHeader("Ocp-Apim-Subscription-Key", subKey);
        // send the request and wait for a response
        yield return webReq.SendWebRequest();


        // if we got an error - log it and return
        if(webReq.isHttpError)
        {  
            Debug.Log(webReq.error);
            yield break;
        }
        // otherwise set the access token
        accessToken = webReq.downloadHandler.text;

    }

    // sends the text to the Speech API and returns audio data
    public IEnumerator GetSpeech (string text)
    {

        // create the body - specifying the text, voice, language, etc
        string body = @"<speak version='1.0' xml:lang='fr-FR'><voice xml:lang='fr-FR' xml:gender='Male'
                        name='fr-FR-AlainNeural'> " + text + "</voice></speak>";

        // create a web request and set the method to POST
        UnityWebRequest webReq = new UnityWebRequest(string.Format("https://westeurope.tts.speech.microsoft.com/cognitiveservices/v1", region));
        webReq.method = UnityWebRequest.kHttpVerbPOST;
        

        // create a download handler to receive the audio data
        webReq.downloadHandler = new DownloadHandlerBuffer();

        // set the body to be uploaded
        webReq.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
        webReq.uploadHandler.contentType = "application/ssml+xml";
        print("GetAccessToken" + accessToken);

        // set the headers
        webReq.SetRequestHeader("Authorization", "Bearer " + accessToken);
        webReq.SetRequestHeader("User-Agent", resourceName);
        webReq.SetRequestHeader("X-Microsoft-OutputFormat", "riff-24khz-16bit-mono-pcm");

        // send the request and wait for a response
        yield return webReq.SendWebRequest();
        print("req " + webReq.responseCode);
        // if there's a problem - return
        if(webReq.isHttpError)
            yield break;
        // play the audio
        StartCoroutine(PlayTTS(webReq.downloadHandler.data));
    }

    // converts the audio data and plays the clip
    IEnumerator PlayTTS (byte[] audioData)
    {
        // save the audio data temporarily as a .wav file
        string tempPath = Application.persistentDataPath + "/tts.wav";
        print("tempPath" + tempPath);

        System.IO.File.WriteAllBytes(tempPath, audioData);
        // load that file in
        UnityWebRequest loader = UnityWebRequestMultimedia.GetAudioClip(tempPath, AudioType.WAV);
        yield return loader.SendWebRequest();
        // convert it to an audio clip
        AudioClip ttsClip = DownloadHandlerAudioClip.GetContent(loader);
        // play it
        ttsSource.PlayOneShot(ttsClip);
    }

    // Start is called before the first frame update
    void Start()
    {
        // before we can do anything, we need an access token
        StartCoroutine(GetAccessToken());   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
