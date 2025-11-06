Shader "Hidden/BlackoutReplacement"
{
Properties
{
    _BlackTint ("Black Tint", Color) = (0,0,0,1)
}
SubShader
{
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }
    Pass
    {
        ZWrite On
        ZTest LEqual
        Cull Off
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        struct appdata { float4 vertex : POSITION; };
        struct v2f { float4 pos : SV_POSITION; };

        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            return o;
        }

        fixed4 _BlackTint;

        fixed4 frag(v2f i) : SV_Target
        {
            return _BlackTint;
        }
        ENDCG
    }
}
Fallback Off
}
