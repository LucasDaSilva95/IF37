using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    // UI RawImage we're applying the web cam texture to
    public RawImage cameraProjection;
    // texture which displays what our camera is seeing
    private WebCamTexture camText;
    // Start is called before the first frame update

    void Start()
    {
        // create the camera texture
        camText = new WebCamTexture(Screen.width, Screen.height);
        cameraProjection.texture = camText;
        
        camText.Play();
    }

    // takes a picture and converts the data to a byte array
    IEnumerator TakePicture ()
    {   
        yield return new WaitForEndOfFrame();
        // create a new texture the size of the web cam texture
        Texture2D screenTex = new Texture2D(camText.width, camText.height);
        // read the pixels on the web cam texture and apply them
        screenTex.SetPixels(camText.GetPixels());
        screenTex.Apply();
        // convert the texture to PNG, then get the data as a byte array
        byte[] byteData = screenTex.EncodeToPNG();
        
        
         // send the image data off to the Computer Vision API
        AppManager.instance.StartCoroutine("GetImageData", byteData);
    }

    // Update is called once per frame
    void Update()
    {
        // click / touch input to take a picture
        if(Input.GetMouseButtonDown(0))
            StartCoroutine(TakePicture());
        else if(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            StartCoroutine(TakePicture());
    }
}
