Shader "Custom/BlankTileShader"
{
    Properties
    {
        _NoiseTex ("Base (RGB)", 2D) = "white" {}
        _BAndTex ("Bitwise AND Map (RGB)", 2D) = "white" {}
        _Neighbours ("Neighbours", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "BlankTile" = "True" }
        Pass
        {
            ZWrite Off

            CGPROGRAM
            #pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _NoiseTex;
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

            float edgeShadow(float diff, float mul)
            {
                return diff * 4 * mul + (1 - mul);
            }

            float vertShadow(float2 coord, float2 vert, float mul)
            {
                return min(1, length(coord - vert) * 4) * mul + (1 - mul);
            }

            float4 frag(fragmentInput i) : COLOR
            {
				float4 edgeShads = float4(
					edgeShadow(i.texCoord.y, i.nbrsEdge.x),
					edgeShadow(1 - i.texCoord.y, i.nbrsEdge.z),
					edgeShadow(1 - i.texCoord.x, i.nbrsEdge.y),
					edgeShadow(i.texCoord.x, i.nbrsEdge.w)
				);

				float4 vertShads = float4(
					vertShadow(i.texCoord, float2(0, 0), i.nbrsVert.x),
                    vertShadow(i.texCoord, float2(1, 0), i.nbrsVert.y),
                    vertShadow(i.texCoord, float2(1, 1), i.nbrsVert.z),
                    vertShadow(i.texCoord, float2(0, 1), i.nbrsVert.w)
				);
				
				edgeShads = min(edgeShads, float4(1, 1, 1, 1));
				vertShads = min(vertShads, float4(1, 1, 1, 1));

				float edgeShad = edgeShads.x * edgeShads.y * edgeShads.z * edgeShads.w;
				float vertShad = vertShads.x * vertShads.y * vertShads.z * vertShads.w;
				
                return float4(tex2D(_NoiseTex, i.screenPos).rgb * _Color.rgb * (0.8 + min(edgeShad, vertShad) * 0.2), 1);
            }
            ENDCG
        }
    } 
    FallBack "Custom/SimpleBlankTileShader"
}
