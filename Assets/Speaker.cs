using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Windows.Speech;
using UnityEngine;
using TMPro;


public class Speaker : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public static Speaker instance;
    public TextMeshProUGUI uiText;  

    void Awake ()
    {
        // set the instance
        instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
         AudioSource audioSource = GetComponent<AudioSource>();
         audioSource.clip = Microphone.Start("Microphone (HD Pro Webcam C920)", true, 10, 44100);
        // audioSource.Play();
    }

    void recognizeWord(string sentence){
        keywords.Clear();
        string[] words = sentence.Split(' ','\n');

        foreach(string word in words){
            
            if(word.Length > 0)
            {
                Debug.Log("words "+word);
                keywords.Add(word, () => {
                Debug.Log("IT WORKS: " + word);
                uiText.text = uiText.text.Replace(word,"");
                });
            }
        }
        

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {   
    System.Action keywordAction;
    // if the keyword recognized is in our dictionary, call that Action.
    if (keywords.TryGetValue(args.text, out keywordAction))
    {
        keywordAction.Invoke();
    }
    }   



    // Update is called once per frame
    void Update()
    {
        
    }
}
