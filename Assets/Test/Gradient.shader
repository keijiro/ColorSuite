// Gradient test card shader
// Note: assuming a linear color space.

Shader "Custom/Gradient Test"
{
    CGINCLUDE

#include "UnityCG.cginc"

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
    float3 rgb;

    if (i.uv.y < 0.25)
    {
        rgb = float3(2, 2, 2) * i.uv.x;
    }
    else
    {
        float u = i.uv.x;
        float v = (i.uv.y - 0.25) * 8 / 3;

        float r = abs(u * 6 - 3) - 1;
        float g = 2 - abs(u * 6 - 2);
        float b = 2 - abs(u * 6 - 4);

        rgb = saturate(float3(r, g, b));

        if (v < 1)
        {
            rgb *= v;
        }
        else
        {
            rgb = lerp(rgb, float3(1, 1, 1), v - 1);
        }
    }

    return float4(srgb_to_linear(rgb), 1);
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
