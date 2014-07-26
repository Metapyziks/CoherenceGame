Shader "Custom/FlatNoisyShader"
{
    Properties
    {
        _NoiseTex ("Base (RGB)", 2D) = "white" {}
        _Scale ("Scale", Float) = 1
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _NoiseTex;

            uniform float _Scale;

            uniform half4 _Color;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 texCoord : TEXCOORD0;
            };

            struct fragmentInput
            {
                float4 position : SV_POSITION;
                float2 screenPos : TEXCOORD0;
            };

            bool bitwiseAnd(float a, float b)
            {
                return tex2D(_BAndTex, float2(a, b)).r > 0;
            }

            fragmentInput vert(vertexInput i)
            {
                fragmentInput o;

                o.position = mul(UNITY_MATRIX_MVP, i.vertex);
                o.screenPos = ComputeScreenPos(o.position).xy * _ScreenParams.xy / (512 * _Scale);

                return o;
            }

            float4 frag(fragmentInput i) : COLOR
            {            
                return float4(tex2D(_NoiseTex, i.screenPos).rgb, 1) * _Color;
            }
            ENDCG
        }
    }
}
