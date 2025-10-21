Shader "Hidden/WhiteShadedPosterizeV5_Camera"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _PosterizeLevels ("Posterize Levels", Range(2,16)) = 5.6
        _PosterizeStrength ("Posterize Strength", Range(0,1)) = 1
        _ShadingBlend ("Shading Blend", Range(0,1)) = 0
        _Contrast ("Contrast", Range(0.1,3)) = 1.3
        _Brightness ("Brightness", Range(0,10)) = 2.25
        _WhiteTint ("White Tint", Color) = (1,1,1,1)
        _BlackTint ("Black Tint", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Back
        ZWrite On
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _PosterizeLevels;
            float _PosterizeStrength;
            float _ShadingBlend;
            float _Contrast;
            float _Brightness;
            float4 _WhiteTint;
            float4 _BlackTint;

            inline float3 LumaWeights() { return float3(0.2126, 0.7152, 0.0722); }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float3 tex = tex2D(_MainTex, uv).rgb;
                float lum = dot(tex, LumaWeights());
                lum = (lum - 0.5) * _Contrast + 0.5;
                lum = saturate(lum * _Brightness);

                float levels = max(2.0, floor(_PosterizeLevels + 0.5));
                float stepIndex = floor(lum * levels + 1e-6);
                float denom = max(1.0, levels - 1.0);
                float quant = (levels <= 1.0) ? lum : stepIndex / denom;
                quant = saturate(quant);

                float preserved = lerp(quant, lum, _ShadingBlend);
                float posterized = lerp(lum, preserved, _PosterizeStrength);

                float3 gray = lerp(_BlackTint.rgb, _WhiteTint.rgb, posterized);
                return float4(gray, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
