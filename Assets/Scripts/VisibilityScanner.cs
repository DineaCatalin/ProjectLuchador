using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VisibilityScanner : MonoBehaviour
{
    [Header("Setup")]
    public Canvas mainCanvas;
    public RectTransform scanArea;
    
    [Header("The Target")]
    public Image whiteHolesImage;   // Ensure "Read/Write Enabled" is ON in texture settings!
    
    [Header("The Player")]
    public LayerMask facePartLayer; // Set to "Face" (or wherever your parts are)
    
    [Header("Settings")]
    public int resolutionStep = 15;

    void Start()
    {
        // REQUIRED: This allows the manual check to ignore transparent pixels (holes)
        whiteHolesImage.alphaHitTestMinimumThreshold = 0.1f;
    }

    public float GetIntersectionScore()
    {
        Vector3[] corners = new Vector3[4];
        scanArea.GetWorldCorners(corners);
        
        // We need the camera to convert Screen Points to Local Points correctly
        Camera uiCam = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;
        
        Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        List<RaycastResult> results = new List<RaycastResult>();

        int totalTargetPixels = 0;
        int filledPixels = 0;

        for (float x = min.x; x < max.x; x += resolutionStep)
        {
            for (float y = min.y; y < max.y; y += resolutionStep)
            {
                Vector2 currentPoint = new Vector2(x, y);
                
                // STEP 1: MANUAL CHECK (Bypasses Camera Culling)
                // We ask the image component directly: "Is this point valid on you?"
                // This ignores the fact that the Camera is culling the layer.
                if (whiteHolesImage.IsRaycastLocationValid(currentPoint, uiCam))
                {
                    totalTargetPixels++; // We are on a solid white pixel

                    // STEP 2: RAYCAST CHECK (For the Face Parts)
                    // Now we check if anything else (the Face) is ALSO here.
                    pointerData.position = currentPoint;
                    results.Clear();
                    EventSystem.current.RaycastAll(pointerData, results);
                    
                    if (CheckIfFilled(results))
                    {
                        filledPixels++;
                    }
                }
            }
        }

        if (totalTargetPixels == 0) 
        {
            Debug.LogError("Still 0? Check that 'whiteHolesImage' Texture has Read/Write Enabled!");
            return 0f;
        }

        float score = ((float)filledPixels / totalTargetPixels) * 100f;
        Debug.Log($"FINAL SCORE: {score:F1}% ({filledPixels}/{totalTargetPixels})");
        return score;
    }

    // Helper to find face parts in the stack
    bool CheckIfFilled(List<RaycastResult> results)
    {
        foreach (var result in results)
        {
            // Check if we hit an object on the Face Layer
            if (((1 << result.gameObject.layer) & facePartLayer) != 0)
            {
                return true;
            }
        }
        return false;
    }
}