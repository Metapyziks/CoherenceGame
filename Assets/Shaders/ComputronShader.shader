Shader "Custom/ComputronShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise (RGB)", 2D) = "white" {}
        _UpColor ("Up Color", Color) = (1, 1, 1, 1)
        _DownColor ("Down Color", Color) = (1, 1, 1, 1)
        _Direction ("Direction", Float) = 0
        _StateQ1 ("States Q1", Vector) = (1, 1, 1, 0)
        _StateQ2 ("States Q2", Vector) = (0, 0, 1, 0)
        _StateQ3 ("States Q3", Vector) = (0, 1, 0, 1)
        _StateQ4 ("States Q4", Vector) = (1, 1, 1, 0)
    }

    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
        
            uniform sampler2D _MainTex;
            uniform sampler2D _NoiseTex;

            uniform half4 _UpColor;
            uniform half4 _DownColor;

            uniform half _Direction;
            
            uniform half4 _StateQ1;
            uniform half4 _StateQ2;
            uniform half4 _StateQ3;
            uniform half4 _StateQ4;

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
                float midState : TEXCOORD2;
            };

            fragmentInput vert(vertexInput i)
            {
                fragmentInput o;

                o.position = mul(UNITY_MATRIX_MVP, i.vertex);
                o.texCoord = i.texCoord;
                o.screenPos = ComputeScreenPos(o.position).xy * _ScreenParams.xy / 1024;
                o.midState = dot(_StateQ1 + _StateQ2 + _StateQ3 + _StateQ4, half4(1, 1, 1, 1) / 16);

                return o;
            }

            float indexVector(float4 vec, int index)
            {
                switch (index) {
                    case 0: return vec.x;
                    case 1: return vec.y;
                    case 2: return vec.z;
                    case 3: return vec.w;
                }

                return 0;
            }

            float getState(int index)
            {
                int quad = index / 4;
                index -= quad * 4;
                switch (quad) {
                    case 0: return indexVector(_StateQ1, index);
                    case 1: return indexVector(_StateQ2, index);
                    case 2: return indexVector(_StateQ3, index);
                    case 3: return indexVector(_StateQ4, index);
                }

                return 0;
            }

            float2 rotateTexCoord(float2 coord)
            {
                switch (_Direction) {
                    case 0: return coord;
                    case 1: return float2(1 - coord.y, coord.x);
                    case 2: return float2(1, 1) - coord;
                    case 3: return float2(coord.y, 1 - coord.x);
                }

                return coord;
            }

            float4 frag(fragmentInput i) : COLOR
            {
                const float pi = 3.1415926535897932384626433832795;

                half3 clr = tex2D(_MainTex, rotateTexCoord(i.texCoord)).rgb;

                float2 diff = i.texCoord - float2(0.5, 0.5);

                float state = i.midState;

                if (dot(diff, diff) > 1.0 / 16) {
                    int index = floor(atan2(diff.x, diff.y) * 8 / pi) + 8;
                    state = getState(index);
                }

                float noise = tex2D(_NoiseTex, i.screenPos).rgb;
                half3 hue = (state * _UpColor.rgb + (1 - state) * _DownColor.rgb);

                return float4(hue * noise * clr.r, clr.g);
            }
            ENDCG
        }
    } 
    FallBack "Diffuse"
}
