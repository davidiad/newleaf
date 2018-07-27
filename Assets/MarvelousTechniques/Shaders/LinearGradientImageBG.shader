//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2015 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
Shader "Kirnu/Marvelous/LinearGradientBGImage" {
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Cull Off
        ZWrite Off
                 
		Tags { "QUEUE"="Background" "RenderType"="Opaque" }
		LOD 200
		
		Pass {

		Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Background" "RenderType"="Opaque" }
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			uniform fixed4 _TintColor;
			
			struct IN
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct OUT
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _MainTex_ST;

			OUT vert (IN v)
			{
				OUT o;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv = TRANSFORM_TEX (v.uv, _MainTex);
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (OUT i) : SV_Target{
				return tex2D (_MainTex, i.uv)*_TintColor;
			}
			ENDCG
		}
	}
}
