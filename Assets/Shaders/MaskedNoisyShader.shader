Shader "Custom/MaskedNoisyShader"
{
    Properties
    {
        _NoiseTex ("Noise", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _Scale ("Scale", Float) = 1
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _NoiseTex;
            uniform sampler2D _MaskTex;

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
                float2 texCoord : TEXCOORD0;
                float2 screenPos : TEXCOORD1;
            };

            fragmentInput vert(vertexInput i)
            {
                fragmentInput o;

                o.position = mul(UNITY_MATRIX_MVP, i.vertex);
                o.texCoord = i.texCoord;
                o.screenPos = ComputeScreenPos(o.position).xy * _ScreenParams.xy / (512 * _Scale);

                return o;
            }

            float4 frag(fragmentInput i) : COLOR
            {            
                return float4(tex2D(_NoiseTex, i.screenPos).rgb, tex2D(_MaskTex, i.texCoord).r) * _Color;
            }
            ENDCG
        }
    }
}
