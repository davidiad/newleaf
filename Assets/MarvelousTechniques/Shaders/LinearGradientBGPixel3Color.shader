//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2015 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
Shader "Kirnu/Marvelous/LinearGradientBGPixel3Color" {
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_TopColor ("Top Color", Color) = (1,1,1,1)
		_MidColor ("Middle Color", Color) = (1,0,0,0)
		_BottomColor ("Bottom Color", Color) = (0,0,0,0)
		_Middle("Middle", Range(0,1)) = 0.5
	}
	SubShader
	{
		Cull Off
        //ZWrite Off
                 
		Tags { "QUEUE"="Background" "RenderType"="Opaque" }
		LOD 200
		
		Pass {

		Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Background" "RenderType"="Opaque" }
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			uniform fixed4 _TopColor;
			uniform fixed4 _MidColor;
			uniform fixed4 _BottomColor;
			uniform fixed _Middle;
			
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

			sampler2D _MainTex;
			float4 _MainTex_ST;

			OUT vert (IN v)
			{
				OUT o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX (v.uv, _MainTex);

				return o;
			}
			

			float4 frag (OUT i) : COLOR{
				fixed4 c = lerp(_BottomColor, _MidColor, i.uv.y / _Middle) * step(i.uv.y, _Middle);
				c += lerp(_MidColor, _TopColor, (i.uv.y - _Middle) / (1 - _Middle)) * step(_Middle, i.uv.y);
				return c;
			}
			ENDCG
		}
	}
}
