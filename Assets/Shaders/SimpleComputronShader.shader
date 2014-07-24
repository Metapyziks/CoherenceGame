Shader "Custom/SimpleComputronShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise (RGB)", 2D) = "white" {}
        _UpColor ("Up Color", Color) = (1, 1, 1, 1)
        _DownColor ("Down Color", Color) = (1, 1, 1, 1)
        _State ("State", Float) = 0
    }

    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
        
            uniform sampler2D _MainTex;
            uniform sampler2D _NoiseTex;

            uniform half4 _UpColor;
            uniform half4 _DownColor;

            uniform half _State;

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
                o.screenPos = ComputeScreenPos(o.position).xy * _ScreenParams.xy / 1024;

                return o;
            }

            float4 frag(fragmentInput i) : COLOR
            {
                half3 clr = tex2D(_MainTex, i.texCoord).rgb;
                float noise = tex2D(_NoiseTex, i.screenPos).rgb;

                half3 hue = (_State * _UpColor.rgb + (1 - _State) * _DownColor.rgb);

                if (clr.g < 0.5) discard;

                return float4(hue * noise, 1);
            }
            ENDCG
        }
    } 
    FallBack "Diffuse"
}
