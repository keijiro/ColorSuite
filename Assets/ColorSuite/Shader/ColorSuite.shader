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

Shader "Hidden/ColorSuite"
{
    Properties
    {
        _MainTex("Base", 2D) = ""{}
        _Curves("Curves", 2D) = ""{}
    }
    
    CGINCLUDE

    // Multi compilation options (tonemap/vignette/dither)
    #pragma multi_compile TONEMAPPING_OFF TONEMAPPING_ON
    #pragma multi_compile VIGNETTE_OFF VIGNETTE_ON
    #pragma multi_compile DITHER_OFF DITHER_ORDERED DITHER_TRIANGULAR
    #pragma multi_compile LINEAR_OFF LINEAR_ON

    #include "UnityCG.cginc"
    
    sampler2D _MainTex;
    sampler2D _Curves;
    float _Saturation;
    float2 _MainTex_TexelSize;

    // Color space conversion between sRGB and linear.
    // http://chilliant.blogspot.jp/2012/08/srgb-approximations-for-hlsl.html
    float3 srgb_to_linear(float3 s)
    {
        return s * (s * (s * 0.305306011 + 0.682171111) + 0.012522878);
    }
    float3 linear_to_srgb(float3 s)
    {
        s = saturate(s);
        float3 s1 = sqrt(s);
        float3 s2 = sqrt(s1);
        float3 s3 = sqrt(s2);
        return 0.585122381 * s1 + 0.783140355 * s2 - 0.368262736 * s3;
    }

#if TONEMAPPING_ON
    // Reinhard tonemapping operator
    float _Exposure;
    float3 reinhard(float3 s)
    {
        float l = Luminance(s); 
        float lT = l * _Exposure;
        return s * (lT / ((1 + lT) * l));
    }
#endif

#if VIGNETTE_ON
    // Pseudo vignette function
    float _Vignette;
    float vignette(float2 uv)
    {
        float2 cuv = (uv - 0.5) * 2;
        return max(1 - dot(cuv, cuv) * _Vignette * 0.1, 0.0);
    }
#endif

#if DITHER_ORDERED
    // Interleaved gradient function
    // http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
    float interleaved_gradient(float2 uv)
    {
        float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
        return frac(magic.z * frac(dot(uv, magic.xy)));
    }
    float3 dither(float2 uv)
    {
        return float3(interleaved_gradient(uv / _MainTex_TexelSize) / 255);
    }
#endif

#if DITHER_TRIANGULAR
    // Triangular PDF.
    float nrand(float2 n)
    {
        return frac(sin(dot(n, float2(12.9898, 78.233))) * 43758.5453);
    }
    float3 dither(float2 uv)
    {
        float r = nrand(uv) + nrand(uv + float2(1.1)) - 0.5;
        return float3(r / 255);
    }
#endif

    // Color adjustment function.
    float3 adjust_color(float3 s)
    {
        float4 r = tex2D(_Curves, float2(s.r, 0));
        float4 g = tex2D(_Curves, float2(s.g, 0));
        float4 b = tex2D(_Curves, float2(s.b, 0));
        float3 c = float3(r.r * r.a, g.g * g.a, b.b * b.a);
        float l = Luminance(c);
        return lerp(float3(l, l, l), c, _Saturation);
    }

    float4 frag(v2f_img i) : SV_Target 
    {
        float4 source = tex2D(_MainTex, i.uv); 
        float3 rgb = source.rgb;
#if TONEMAPPING_ON
        rgb = reinhard(rgb);
#endif
#if VIGNETTE_ON
#if LINEAR_ON
        rgb *= srgb_to_linear(vignette(i.uv));
#else
        rgb *= vignette(i.uv);
#endif
#endif
        rgb = adjust_color(rgb);
#if !DITHER_OFF
#if LINEAR_ON
        rgb = srgb_to_linear(linear_to_srgb(rgb) + dither(i.uv));
#else
        rgb += dither(i.uv);
#endif
#endif
        return float4(rgb, source.a);
    }

    ENDCG 
    
    Subshader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }      
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
