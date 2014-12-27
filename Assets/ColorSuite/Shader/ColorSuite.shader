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
        _Balance("-", Vector) = (0.5, 0.5, 0.5, 0)
    }
    
    CGINCLUDE

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

    // Color space conversion between RGB and LMS.
    // http://www.daltonize.org/2010/05/lms-daltonization-algorithm.html
    float3 rgb_to_lms(float3 s)
    {
        float3x3 m = {
            1.78824e+1, 4.35161e+1, 4.11935e+0,
            3.45565e+0, 2.71554e+1, 3.86714e+0,
            2.99566e-2, 1.84309e-1, 1.46709e+0
        };
        return mul(m, s);
    }

    float3 lms_to_rgb(float3 s)
    {
        float3x3 m = {
             8.09444479e-2, -1.30504409e-1,  1.16721066e-1,
            -1.02485335e-2,  5.40193266e-2, -1.13614708e-1,
            -3.65296938e-4, -4.12161469e-3,  6.93511405e-1
        };
        return mul(m, s);
    }

    // Multi compilation options (vignette/tonemapping)
    #pragma multi_compile TONEMAPPING_OFF TONEMAPPING_ON
    #pragma multi_compile VIGNETTE_OFF VIGNETTE_ON
    #pragma multi_compile BALANCING_OFF BALANCING_ON

    #include "UnityCG.cginc"
    
    sampler2D _MainTex;
    sampler2D _Curves;
    float _Saturation;
    float3 _Balance;

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
        return 1 - dot(cuv, cuv) * _Vignette * 0.1;
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
        rgb *= vignette(i.uv);
#endif
#if BALANCING_ON
        rgb = linear_to_srgb(rgb);
        rgb = lms_to_rgb(rgb_to_lms(rgb) * _Balance);
        rgb = srgb_to_linear(rgb);
#endif
        return float4(adjust_color(rgb), source.a);
    }

    ENDCG 
    
    Subshader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }      
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            #pragma glsl
            ENDCG
        }
    }
}
