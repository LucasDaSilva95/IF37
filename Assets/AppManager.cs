using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;

public class AppManager : MonoBehaviour
{

    // Computer Vision subscription key
    public string subKey;
    // Computer Vision API url
    public string url;

    public Image image;

    // on-screen text which shows the text we've analyzed
    public TextMeshProUGUI uiText;      

    // instance
    public static AppManager instance;
    void Awake ()
    {
        // set the instance
        instance = this;
    }
    
    // sends the image to the Computer Vision API and returns a JSON file
    public IEnumerator GetImageData (byte[] imageData)
    {
        // uiText.text = "<i>[Calculating...]</i>";
        // create a web request and set the method to POST
        UnityWebRequest webReq = new UnityWebRequest(url);
        webReq.method = UnityWebRequest.kHttpVerbPOST;

        // create a download handler to receive the JSON file
        webReq.downloadHandler = new DownloadHandlerBuffer();   

        // upload the image data
        webReq.uploadHandler = new UploadHandlerRaw(imageData);
        webReq.uploadHandler.contentType = "application/octet-stream";

        // set the header
        webReq.SetRequestHeader("Ocp-Apim-Subscription-Key", subKey);

        // send the content to the API and wait for a response
        yield return webReq.SendWebRequest();

        // convert the content string to a JSON file
        JSONNode jsonData = JSON.Parse(webReq.downloadHandler.text);
        Debug.Log("JSON : "+webReq.responseCode);

                // get just the text from the JSON file and display on-screen
        string imageText = GetTextFromJSON(jsonData);
        if(imageText.Length > 0){
            image.enabled = true;
            uiText.text = imageText;
        }
        
        
        // send the text to the text to speech API
        // ... called in another script we'll make soon
        TextToSpeech.instance.StartCoroutine("GetSpeech",imageText);
        Speaker.instance.StartCoroutine("recognizeWord",imageText);

    }

    // returns the text from the JSON data
    string GetTextFromJSON (JSONNode jsonData)
    {
        string text = "";
        int count = 0;
        
        JSONNode lines = jsonData["regions"][0]["lines"];
        // loop through each line
        foreach(JSONNode line in lines.Children)
        {
            // loop through each word in the line
            foreach(JSONNode word in line["words"].Children)
            {
                count++;
                if(count > 4) {
                    Debug.Log("text : "+text);

                    text += "\n";
                    count = 0;
                }
                // add the text
                text += word["text"] + " ";
                
            }
        }
        return text;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(uiText.text.Length == 0){
            image.enabled = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
