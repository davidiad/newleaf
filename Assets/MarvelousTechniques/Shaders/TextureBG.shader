//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2016 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
Shader "Kirnu/Marvelous/TextureBG" {
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			
			uniform fixed4 _TopColor;
			uniform fixed4 _BottomColor;
			uniform fixed _Ratio;

			float4 _MainTex_ST;

			struct IN{
				half4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
			};
		
			struct OUT {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
			};

			OUT vert (IN v)
			{
				OUT o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (OUT i) : SV_Target{
				return tex2D (_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
