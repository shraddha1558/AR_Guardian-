using UnityEngine;

public class Detection
{
    public Rect rect;
    public float confidence;
    public string label;

    public Detection(Rect rect, float confidence, string label)
    {
        this.rect = rect;
        this.confidence = confidence;
        this.label = label;
    }
}
