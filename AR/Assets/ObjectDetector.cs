using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;

public class ObjectDetector : MonoBehaviour
{
    public NNModel modelAsset; // Assign YOLOv8 ONNX model in Unity Inspector
    private Model model;
    private IWorker worker;

    public WebCamTextureHandler webcamHandler;
    public int inputWidth = 640;
    public int inputHeight = 640;

    void Start()
    {
        if (modelAsset == null)
        {
            Debug.LogError("Model asset is not assigned!");
            return;
        }

        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);

        if (webcamHandler == null)
        {
            webcamHandler = FindObjectOfType<WebCamTextureHandler>();
        }
        
        if (webcamHandler == null)
        {
            Debug.LogError("WebCamTextureHandler is missing! Attach it to a GameObject in the scene.");
        }
    }

    void Update()
    {
        if (webcamHandler == null || !webcamHandler.HasNewFrame())
            return;

        ProcessFrame(webcamHandler.GetFrame());
    }

    void ProcessFrame(Texture2D inputTexture)
{
    Texture2D resizedTexture = ResizeTexture(inputTexture, 224, 224);  // Adjust dimensions as per model
    Tensor inputTensor = new Tensor(resizedTexture, channels: 3);
    worker.Execute(inputTensor);
}

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        rt.Release();
        Destroy(rt);

        return result;
    }

    void ProcessResults(Tensor outputTensor)
    {
        if (outputTensor == null)
        {
            Debug.LogError("Output tensor is null!");
            return;
        }

        List<Detection> detections = DecodeYOLOOutput(outputTensor);
        foreach (var detection in detections)
        {
            Debug.Log($"Detected: {detection.label} at {detection.rect}");
        }
    }

    List<Detection> DecodeYOLOOutput(Tensor outputTensor)
    {
        List<Detection> detections = new List<Detection>();

        for (int i = 0; i < outputTensor.shape.batch; i++)
        {
            float confidence = outputTensor[i, 4];
            if (confidence > 0.5f)
            {
                float x = outputTensor[i, 0] * webcamHandler.GetWidth();
                float y = outputTensor[i, 1] * webcamHandler.GetHeight();
                float w = outputTensor[i, 2] * webcamHandler.GetWidth();
                float h = outputTensor[i, 3] * webcamHandler.GetHeight();

                int classIndex = ArgMax(outputTensor, i);
                string label = "Object " + classIndex;

                detections.Add(new Detection(new Rect(x, y, w, h), confidence, label));
            }
        }
        return detections;
    }

    int ArgMax(Tensor tensor, int row)
    {
        float maxVal = float.MinValue;
        int maxIndex = 0;

        for (int j = 5; j < tensor.shape.width; j++)
        {
            if (tensor[row, j] > maxVal)
            {
                maxVal = tensor[row, j];
                maxIndex = j - 5;
            }
        }
        return maxIndex;
    }

    private void OnDestroy()
    {
        if (worker != null)
        {
            worker.Dispose();
        }
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
}