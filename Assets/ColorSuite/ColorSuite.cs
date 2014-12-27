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
    [SerializeField] float _saturation = 1.0f;

    public float saturation {
        get { return _saturation; }
        set { _saturation = value; } // no UpdateCurves
    }

    // Tone mapping parameters.
    [SerializeField] bool _toneMapping = false;
    [SerializeField] float _exposure   = 1.8f;

    public bool toneMapping {
        get { return _toneMapping; }
        set { _toneMapping = value; }
    }
    public float exposure {
        get { return _exposure; }
        set { _exposure = value; }
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
        for (var x = 0; x < _texture.width; x++)
        {
            var u = 1.0f / (_texture.width - 1) * x;
            var r = _lCurve.Evaluate(_rCurve.Evaluate(u));
            var g = _lCurve.Evaluate(_gCurve.Evaluate(u));
            var b = _lCurve.Evaluate(_bCurve.Evaluate(u));
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

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetUpResources();

        _material.SetTexture("_Curves", _texture);
        _material.SetFloat("_Saturation", _saturation);

        if (_toneMapping)
        {
            _material.EnableKeyword("TONEMAPPING_ON");
            _material.SetFloat("_Exposure", _exposure);
        }
        else
            _material.DisableKeyword("TONEMAPPING_ON");

        Graphics.Blit(source, destination, _material);
    }
}
