Shader "Custom/BlankTileShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BAndTex ("Bitwise AND Map (RGB)", 2D) = "white" {}
        _Neighbours ("Neighbours", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _BAndTex;
            
            uniform float _Neighbours;

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

                half4 nbrsEdge : TEXCOORD2;
                half4 nbrsVert : TEXCOORD3;
            };

            bool bitwiseAnd(float a, float b)
            {
                return tex2D(_BAndTex, float2(a, b)).r > 0;
            }

            fragmentInput vert(vertexInput i)
            {
                fragmentInput o;

                o.position = mul(UNITY_MATRIX_MVP, i.vertex);
                o.texCoord = i.texCoord;
                o.screenPos = ComputeScreenPos(o.position).xy * _ScreenParams.xy / 512;

                float n = round(_Neighbours * 255);

                o.nbrsEdge.w = floor(n / 128); n -= o.nbrsEdge.w * 128;
                o.nbrsVert.w = floor(n / 64); n -= o.nbrsVert.w * 64;
                o.nbrsEdge.z = floor(n / 32); n -= o.nbrsEdge.z * 32;
                o.nbrsVert.z = floor(n / 16); n -= o.nbrsVert.z * 16;
                o.nbrsEdge.y = floor(n / 8); n -= o.nbrsEdge.y * 8;
                o.nbrsVert.y = floor(n / 4); n -= o.nbrsVert.y * 4;
                o.nbrsEdge.x = floor(n / 2); n -= o.nbrsEdge.x * 2;
                o.nbrsVert.x = floor(n / 1); n -= o.nbrsVert.x * 1;

                return o;
            }

            float vertShadow(float2 coord, float2 vert, float mul)
            {
                return min(1, length(coord - vert) * 4) * mul + (1 - mul);
            }

            float4 frag(fragmentInput i) : COLOR
            {
                float shad = 1;

                if (i.texCoord.y < 0.25) {
                    shad *= i.texCoord.y * 4 * i.nbrsEdge.x + (1 - i.nbrsEdge.x);
                } else if (i.texCoord.y >= 0.75) {
                    shad *= (1 - i.texCoord.y) * 4 * i.nbrsEdge.z + (1 - i.nbrsEdge.z);
                }
                
                if (i.texCoord.x >= 0.75) {
                    shad *= (1 - i.texCoord.x) * 4 * i.nbrsEdge.y + (1 - i.nbrsEdge.y);
                } else if (i.texCoord.x < 0.25) {
                    shad *= i.texCoord.x * 4 * i.nbrsEdge.w + (1 - i.nbrsEdge.w);
                }

                if (shad == 1) {
                    shad *= vertShadow(i.texCoord, float2(0, 0), i.nbrsVert.x)
                        * vertShadow(i.texCoord, float2(1, 0), i.nbrsVert.y)
                        * vertShadow(i.texCoord, float2(1, 1), i.nbrsVert.z)
                        * vertShadow(i.texCoord, float2(0, 1), i.nbrsVert.w);
                }

                return float4(tex2D(_MainTex, i.screenPos).rgb * _Color.rgb * (0.8 + shad * 0.2), 1);
            }
            ENDCG
        }
    } 
    FallBack "Diffuse"
}
