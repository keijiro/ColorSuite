using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ColorCurve : MonoBehaviour
{
    public AnimationCurve rCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve gCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve bCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve lCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public float brightness = 0.0f;
    public float saturation = 1.0f;
    public float contrast = 1.0f;

    Material material;
    Texture2D texture;

    void CheckResources()
    {
        if (material != null && texture != null) return;

        if (material == null)
        {
            material = new Material(Shader.Find("Hidden/Color Curve"));
            material.hideFlags = HideFlags.DontSave;
        }

        if (texture == null)
        {
            texture = new Texture2D(256, 1, TextureFormat.ARGB32, false, true);
            texture.hideFlags = HideFlags.DontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
        }

        UpdateParameters();
    }

    void UpdateParameters()
    {
        var bt = brightness > 0 ? 1.0f : -1.0f;
        var bp = Mathf.Abs(brightness);

        for (var x = 0; x < 256; x++)
        {
            var u = 1.0f / 255 * x;
            var r = Mathf.Lerp(lCurve.Evaluate((rCurve.Evaluate(u) - 0.5f) * contrast + 0.5f), bt, bp);
            var g = Mathf.Lerp(lCurve.Evaluate((gCurve.Evaluate(u) - 0.5f) * contrast + 0.5f), bt, bp);
            var b = Mathf.Lerp(lCurve.Evaluate((bCurve.Evaluate(u) - 0.5f) * contrast + 0.5f), bt, bp);
            texture.SetPixel(x, 0, new Color(r, g, b, 0));
        }

        texture.Apply();
    }

    void Start()
    {
        CheckResources();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CheckResources();

        material.SetTexture("_Curves", texture);
        material.SetFloat("_Saturation", saturation);

        Graphics.Blit(source, destination, material);           
    }
}
