Shader "Custom/WallTileShader"
{
	Properties
    {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_EdgeTex ("Edge (RGB)", 2D) = "white" {}
		_BAndTex ("Bitwise AND Map (RGB)", 2D) = "white" {}
        _Neighbours ("Neighbours", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader
    {
        Pass
        {
		    CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

		    uniform sampler2D _MainTex;
		    uniform sampler2D _EdgeTex;
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

                return o;
            }

		    float4 frag(fragmentInput i) : COLOR
            {
			    half3 c = tex2D(_MainTex, i.screenPos).rgb * _Color.rgb;
			    float n = tex2D(_EdgeTex, i.texCoord).r;

                float a = bitwiseAnd(n, _Neighbours) ? 0.25 : 0;
            
			    return float4(c + (half3(1, 1, 1) - c) * a, 1);
		    }
		    ENDCG
        }
	} 
	FallBack "Diffuse"
}
