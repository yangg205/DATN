
Shader "UI/GlowSoft"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,0.5,1)
        _GlowSize ("Glow Size", Range(0, 1)) = 0.1
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Lighting Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _GlowColor;
            float _GlowSize;
            float _GlowIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float glow = smoothstep(1 - _GlowSize, 1.0, 1 - col.a);
                col.rgb += _GlowColor.rgb * glow * _GlowIntensity;
                return col;
            }
            ENDCG
        }
    }
}
