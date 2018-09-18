Shader "Leaves/UnlitTrail"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _MainTex2 ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Hilight ("Highlight Strength", Range(0,5)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_custom
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float4 color: COLOR;
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
                half2 uv2 : TEXCOORD1;
				float4 vertex : SV_POSITION;
                float4 color : COLOR;
			};

			sampler2D _MainTex;
            sampler2D _MainTex2;
			float4 _MainTex_ST;
            float4 _MainTex2_ST;
            float4 _Color;
            float _Hilight;
            float2 uv2;

			
			v2f vert (appdata_custom v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.texcoord1, _MainTex2);
                o.color = _Color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                fixed4 hi = tex2D(_MainTex2, i.uv2);
				fixed4 col = tex2D(_MainTex, i.uv) * i.color + _Hilight * (hi * hi.a);
				return col;
			}
			ENDCG
		}
	}
}
