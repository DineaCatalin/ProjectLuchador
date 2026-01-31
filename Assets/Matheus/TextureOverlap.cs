using UnityEngine;

public class TextureOverlap : MonoBehaviour
{
    public RenderTexture faceRT;
    public RenderTexture maskRT;
    
    public Texture2D newTex;
    float timeCounter = 0;
    float result;
    int whiteFound = 0;
    int maskWhites = 0;

    float ratio = 0.0f;

    void Start()
    {
        newTex = new Texture2D(faceRT.width, faceRT.height, TextureFormat.ARGB32, true, true);
    }

    void PerformEvaluation()
    {
        whiteFound = 0;
        maskWhites = 0;
        
        //Checks how many whites the original Mask has and builds white pixel array for Mask
        RenderTexture.active = maskRT;

        newTex.ReadPixels(new Rect(0, 0, maskRT.width, maskRT.height), 0, 0);
        newTex.Apply();
        Color[] colorArray = newTex.GetPixels(0, 0, newTex.width, newTex.height);
        int[] maskPixels = new int[colorArray.Length];
        for(int i=0; i < colorArray.Length; i++)
        {
            maskPixels[i] = 0;
            if(colorArray[i] == Color.white)
            {
                maskWhites++;
                maskPixels[i] = 1;
            }

        }
        RenderTexture.active = faceRT;

        newTex.ReadPixels(new Rect(0, 0, faceRT.width, faceRT.height), 0, 0);
        newTex.Apply();

        colorArray = newTex.GetPixels(0, 0, faceRT.width, faceRT.height);

        int[] facePixels = new int[colorArray.Length];

        
        for(int i=0; i < colorArray.Length; i++)
        {
            facePixels[i] = 0;
            if(colorArray[i] == Color.white)
            {
                facePixels[i] = 1;
            }
        }

        int[] result = new int[facePixels.Length];
        for(int i=0; i < result.Length; i++)
        {
            result[i] = facePixels[i] * maskPixels[i];
            if(result[i] == 1)
            {
                whiteFound++;
            }
        }
        RenderTexture.active = null;

        if(maskWhites > 0)
            ratio = (float)whiteFound / (float)maskWhites;
        Debug.Log("You got " + whiteFound + " pixels out of " + maskWhites + " with accuracy of " + (ratio*100f) + "%");
        
    }
    
    void Update()
    {
        timeCounter += Time.deltaTime;
        if(timeCounter >= 1f)
        {
            timeCounter = 0;
            PerformEvaluation();
        }
    }
}