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
    half _Saturation;
    
    half4 frag(v2f_img i) : SV_Target 
    {
        half4 source = tex2D(_MainTex, i.uv); 
        
        half r = tex2D(_Curves, half2(source.r)).r;
        half g = tex2D(_Curves, half2(source.g)).g;
        half b = tex2D(_Curves, half2(source.b)).b;

        half3 rgb = half3(r, g, b);
        half l = Luminance(rgb);

        rgb = lerp(half3(l, l, l), rgb, _Saturation);
        return half4(rgb, source.a);
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
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag
            ENDCG
        }
    }
}
