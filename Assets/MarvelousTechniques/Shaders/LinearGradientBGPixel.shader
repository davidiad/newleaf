//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2015 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
Shader "Kirnu/Marvelous/LinearGradientBGPixel" {
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_TopColor ("Top Color", Color) = (1,1,1,1)
		_BottomColor ("Bottom Color", Color) = (0,0,0,0)
		_Ratio("Ratio", Range(0,1)) = 0.5
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
			uniform fixed4 _BottomColor;
			uniform fixed _Ratio;
			
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
				_Ratio *= 2;
				_Ratio -=1;

				return lerp(_BottomColor,_TopColor,clamp(i.uv.y+(_Ratio),0,1));
			}
			ENDCG
		}
	}
}
