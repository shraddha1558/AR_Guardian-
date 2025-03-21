using UnityEngine;
using UnityEngine.UI; // Required if using RawImage for UI

public class WebCamTextureHandler : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private Renderer objectRenderer;
    private RawImage rawImage; // For UI display

    void Start()
    {
        // Try to get Renderer (for 3D object like a Quad)
        objectRenderer = GetComponent<Renderer>();

        // Try to get RawImage (for UI display)
        rawImage = GetComponent<RawImage>();

        // Start webcam
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();

        if (objectRenderer != null)
        {
            objectRenderer.material.mainTexture = webcamTexture;
        }
        else if (rawImage != null)
        {
            rawImage.texture = webcamTexture;
        }
        else
        {
            Debug.LogError("No Renderer or RawImage found on WebcamHandler!");
        }
    }

    public bool HasNewFrame()
    {
        return webcamTexture.didUpdateThisFrame;
    }

    public Texture2D GetFrame()
    {
        Texture2D frame = new Texture2D(webcamTexture.width, webcamTexture.height);
        frame.SetPixels(webcamTexture.GetPixels());
        frame.Apply();
        return frame;
    }

    public int GetWidth() => webcamTexture.width;
    public int GetHeight() => webcamTexture.height;
}
