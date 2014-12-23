// Gradient test card shader
// Note: assuming a linear color space.

Shader "Custom/Gradient Test"
{
    CGINCLUDE

#include "UnityCG.cginc"

float3 hue_to_rgb(float h)
{
    float r = abs(h * 6 - 3) - 1;
    float g = 2 - abs(h * 6 - 2);
    float b = 2 - abs(h * 6 - 4);
    return saturate(float3(r, g, b));
}

// sRGB and linear color space conversion.
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

float4 frag(v2f_img i) : SV_Target 
{
    float u = i.uv.x;
    float v = i.uv.y;
    float3 rgb;

    if (v < 0.1)
    {
        rgb = srgb_to_linear(float3(u));
    }
    else if (v < 0.2)
    {
        rgb = float3(u);
    }
    else if (v < 0.3)
    {
        rgb = float3(u * 2);
    }
    else if (v < 0.65)
    {
        rgb = srgb_to_linear(hue_to_rgb(u) * (v - 0.3) / 0.35);
    }
    else
    {
        rgb = hue_to_rgb(u) * (v - 0.65) / 0.35;
    }

    return float4(rgb, 1);
}

    ENDCG 
    
    Subshader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
