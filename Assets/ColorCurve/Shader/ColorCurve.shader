Shader "Hidden/ColorCurve"
{
    Properties
    {
        _MainTex("Base", 2D) = ""{}
        _Curves("Curves", 2D) = ""{}
    }
    
    CGINCLUDE

    #include "UnityCG.cginc"
    
    sampler2D _MainTex;
    sampler2D _Curves;
    float _Exposure;
    half _Saturation;

    // Simple reinhard tonemapping operator.
    float3 reinhard(float3 rgb)
    {
        float l = Luminance(rgb); 
        float lT = l * _Exposure;
        return rgb * (lT / ((1 + lT) * l));
    }

    // Color adjustment function.
    half3 adjust_color(half3 s)
    {
        half3 r = tex2D(_Curves, half2(s.r, 0)) * half3(1, 0, 0);
        half3 g = tex2D(_Curves, half2(s.g, 0)) * half3(0, 1, 0);
        half3 b = tex2D(_Curves, half2(s.b, 0)) * half3(0, 0, 1);
        half3 c = r + g + b;
        half l = Luminance(c);
        return lerp(half3(l, l, l), c, _Saturation);
    }

    half4 frag_default(v2f_img i) : SV_Target 
    {
        half4 source = tex2D(_MainTex, i.uv); 
        return half4(adjust_color(source.rgb), source.a);
    }

    float4 frag_tonemapped(v2f_img i) : SV_Target 
    {
        float4 source = tex2D(_MainTex, i.uv); 
        float3 rgb = reinhard(source.rgb);
        return half4(adjust_color(rgb), source.a);
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
            #pragma fragment frag_default
            ENDCG
        }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }      
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert_img
            #pragma fragment frag_tonemapped
            ENDCG
        }
    }
}
