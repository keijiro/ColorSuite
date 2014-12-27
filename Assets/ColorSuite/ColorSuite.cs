//
// Copyright (C) 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[ImageEffectTransformsToLDR]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Color Adjustments/Color Suite")]
public class ColorSuite : MonoBehaviour
{
    // Curve objects.
    [SerializeField] AnimationCurve _rCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] AnimationCurve _gCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] AnimationCurve _bCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] AnimationCurve _lCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public AnimationCurve redCurve {
        get { return _rCurve; }
        set { _rCurve = value; UpdateCurves(); }
    }
    public AnimationCurve greenCurve {
        get { return _gCurve; }
        set { _gCurve = value; UpdateCurves(); }
    }
    public AnimationCurve blueCurve {
        get { return _bCurve; }
        set { _bCurve = value; UpdateCurves(); }
    }
    public AnimationCurve luminanceCurve {
        get { return _lCurve; }
        set { _lCurve = value; UpdateCurves(); }
    }

    // Adjustment parameters.
    [SerializeField] float _brightness = 0.0f;
    [SerializeField] float _contrast   = 1.0f;
    [SerializeField] float _saturation = 1.0f;

    public float brightness {
        get { return _brightness; }
        set { _brightness = value; UpdateCurves(); }
    }
    public float contrast {
        get { return _contrast; }
        set { _contrast = value; UpdateCurves(); }
    }
    public float saturation {
        get { return _saturation; }
        set { _saturation = value; } // no UpdateCurves
    }

    // Tonemapping parameters.
    [SerializeField] bool _tonemapping = false;
    [SerializeField] float _exposure   = 1.8f;

    public bool tonemapping {
        get { return _tonemapping; }
        set { _tonemapping = value; }
    }
    public float exposure {
        get { return _exposure; }
        set { _exposure = value; }
    }

    // Vignette parameters.
    [SerializeField] float _vignette = 0.0f;

    public float vignette {
        get { return _vignette; }
        set { _vignette = value; }
    }

    // White balance parameters.
    [SerializeField] bool _whiteBalancing = false;
    [SerializeField] float _whiteColorTemp = 6600.0f;
    [SerializeField] float _whiteColorTint = 0.0f;

    public bool whiteBalancing {
        get { return _whiteBalancing; }
        set { _whiteBalancing = value; }
    }
    public float whiteColorTemp {
        get { return _whiteColorTemp; }
        set { _whiteColorTemp = value; }
    }
    public float whiteColorTint {
        get { return _whiteColorTint; }
        set { _whiteColorTint = value; }
    }

    // Reference to the shader.
    [SerializeField] Shader shader;

    // Temporary objects.
    Material _material;
    Texture2D _texture;

    Color EncodeRGBM(float r, float g, float b)
    {
        var a = Mathf.Max(Mathf.Max(r, g), Mathf.Max(b, 1e-6f));
        a = Mathf.Ceil(a * 255) / 255;
        return new Color(r / a, g / a, b / a, a);
    }

    void SetUpResources()
    {
        if (_material == null)
        {
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        if (_texture == null)
        {
            _texture = new Texture2D(512, 1, TextureFormat.ARGB32, false, true);
            _texture.hideFlags = HideFlags.DontSave;
            _texture.wrapMode = TextureWrapMode.Clamp;
            UpdateCurves();
        }
    }

    void UpdateCurves()
    {
        // Variables for brightness adjustment.
        var bt = _brightness > 0 ? 1.0f : -1.0f;
        var bp = Mathf.Abs(_brightness);

        for (var x = 0; x < _texture.width; x++)
        {
            var u = 1.0f / (_texture.width - 1) * x;
            var r = Mathf.Lerp(_lCurve.Evaluate((_rCurve.Evaluate(u) - 0.5f) * _contrast + 0.5f), bt, bp);
            var g = Mathf.Lerp(_lCurve.Evaluate((_gCurve.Evaluate(u) - 0.5f) * _contrast + 0.5f), bt, bp);
            var b = Mathf.Lerp(_lCurve.Evaluate((_bCurve.Evaluate(u) - 0.5f) * _contrast + 0.5f), bt, bp);
            _texture.SetPixel(x, 0, EncodeRGBM(r, g, b));
        }

        _texture.Apply();
    }

    void Start()
    {
        SetUpResources();
    }

    void OnValidate()
    {
        SetUpResources();
        UpdateCurves();
    }

    void Reset()
    {
        SetUpResources();
        UpdateCurves();
    }

    // Converts a color temperature (Kelvin) to sRGB value.
    //
    // This implementation is based on a Tanner Helland's approximation.
    // http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
    //
    // The coefficients have been slightly modified to get a continuous gradient,
    // and therefore it shouldn't be considered as a scientifically accurate color
    // temperature model.
    static Vector3 KelvinToColor(float k)
    {
        float r, g, b;

        k *= 0.01f;

        if (k < 66)
        {
            r = 1;
            g = 0.38855782260195315f * Mathf.Log(k) - 0.6279231240157355f;
            if (k < 19)
                b = 0;
            else
                b = 0.5410848875902343f * Mathf.Log(k - 10) - 1.1888850134384685f;
        }
        else
        {
            r = Mathf.Pow(k - 60, -0.1332047592f) / 0.7876740722020901f;
            g = Mathf.Pow(k - 60, -0.0755148492f) / 0.8734499527546277f;
            b = 1;
        }

        return new Vector3(r, g, b);
    }

    // Color space conversion between RGB and LMS.
    // http://www.daltonize.org/2010/05/lms-daltonization-algorithm.html
    static Vector3 RgbToLms(Vector3 rgb)
    {
        return new Vector3(
            Vector3.Dot(new Vector3(1.78824e+1f, 4.35161e+1f, 4.11935e+0f), rgb),
            Vector3.Dot(new Vector3(3.45565e+0f, 2.71554e+1f, 3.86714e+0f), rgb),
            Vector3.Dot(new Vector3(2.99566e-2f, 1.84309e-1f, 1.46709e+0f), rgb)
        );
    }
    static Vector3 LmsToRgb(Vector3 rgb)
    {
        return new Vector3(
            Vector3.Dot(new Vector3( 8.09444479e-2f, -1.30504409e-1f,  1.16721066e-1f), rgb),
            Vector3.Dot(new Vector3(-1.02485335e-2f,  5.40193266e-2f, -1.13614708e-1f), rgb),
            Vector3.Dot(new Vector3(-3.65296938e-4f, -4.12161469e-3f,  6.93511405e-1f), rgb)
        );
    }

    // Calculate the color balance coefficients.
    Vector3 CalculateColorBalance()
    {
        // Get the white point.
        var white = KelvinToColor(_whiteColorTemp);

        // Magenta to green color tint.
        white += Vector3.Min(new Vector3(-0.2f, 0.3f, -0.4f) * _whiteColorTint, Vector3.zero);

        // Normalize the white point.
        white /= Vector3.Dot(white, new Vector3(0.3f, 0.59f, 0.11f)); // Y'601

        // Calculate the coefficients in the LMS space.
        var c1 = new Vector3(6.551785e+1f, 3.447819e+1f, 1.681356e+0f); // RgbToLms(Vector3.one)
        var c2 = RgbToLms(white);
        return new Vector3(c1.x / c2.x, c1.y / c2.y, c1.z / c2.z);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetUpResources();

        _material.SetTexture("_Curves", _texture);
        _material.SetFloat("_Saturation", _saturation);

        if (_tonemapping)
        {
            _material.EnableKeyword("TONEMAPPING_ON");
            _material.SetFloat("_Exposure", _exposure);
        }
        else
            _material.DisableKeyword("TONEMAPPING_ON");

        if (_vignette > 0.0f)
        {
            _material.EnableKeyword("VIGNETTE_ON");
            _material.SetFloat("_Vignette", _vignette);
        }
        else
            _material.DisableKeyword("VIGNETTE_ON");

        if (_whiteBalancing)
        {
            _material.EnableKeyword("BALANCING_ON");
            _material.SetVector("_Balance", CalculateColorBalance());
        }
        else
            _material.DisableKeyword("BALANCING_ON");

        Graphics.Blit(source, destination, _material);
    }
}
