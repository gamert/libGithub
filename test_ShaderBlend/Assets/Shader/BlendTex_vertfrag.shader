// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/BlendTex_vertfrag" 
{
Properties 
{
	Tex0 ("Layer 0 (R)", 2D) = "white" {}
	Tex1 ("Layer 1 (G)", 2D) = "white" {}
	Tex2 ("Layer 2 (G)", 2D) = "white" {}
	Tex3 ("Layer 3 (A)", 2D) = "white" {}
	_Control ("Control (RGBA)", 2D) = "red" {}
	_IllumFactor ("Illumin Factor", Range(1,2)) = 1
}
	
SubShader 
{
	Tags{ "Queue" = "Geometry+110"   }
	Pass 
	{
			
		Tags{"LightMode" = "ForwardBase" }

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			sampler2D _Control;
			sampler2D Tex0;
			sampler2D Tex1;
			sampler2D Tex2;
			sampler2D Tex3; 
			half _IllumFactor;

			half4 Tex0_ST;
			half4 Tex1_ST;
			half4 Tex2_ST;
			half4 Tex3_ST;
			half4 _Control_ST;


			struct v2f
			{
				float4	pos : SV_POSITION;
				half2  uv_Tex0:TEXCOORD0;
				half2  uv_Tex1:TEXCOORD1;
				half2  uv_Tex2:TEXCOORD2;
				half2  uv_Tex3:TEXCOORD3;
				half2  uv_Control:TEXCOORD4;
			}; 
			
			struct appdata
			{
			    float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};


			v2f vert (appdata v)
			{
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

				v2f o;
			
				o.uv_Control = v.texcoord.xy;
				o.uv_Tex0 = TRANSFORM_TEX(v.texcoord ,Tex0);
				o.uv_Tex1 = TRANSFORM_TEX(v.texcoord ,Tex1);
				o.uv_Tex2 = TRANSFORM_TEX(v.texcoord, Tex2);
				o.uv_Tex3 = TRANSFORM_TEX(v.texcoord, Tex3);
				o.pos =  UnityObjectToClipPos(float4(v.vertex.xyz, 1));

				return o;
			}
				

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 splat_control = tex2D (_Control, i.uv_Control.xy);
			
				fixed4 albedo = fixed4(1,1,1,1);
				fixed4 Tex0Color = tex2D(Tex0, i.uv_Tex0);
				//albedo.rgb  = splat_control.r * Tex0Color.rgb;...用这个会出现不明黑色阴影
				fixed4 Tex1Color = tex2D(Tex1, i.uv_Tex1);
				//albedo.rgb += splat_control.g * Tex1Color.rgb;

				fixed4 Tex2Color = tex2D(Tex2, i.uv_Tex2).rgba;
				//albedo.rgb += splat_control.b * Tex2Color;
				fixed4 Tex3Color = tex2D(Tex3, i.uv_Tex3).rgba;
				//albedo.rgb += splat_control.a * Tex3Color;
				
				albedo = lerp(Tex0Color,Tex1Color, splat_control.g);
				albedo = lerp(albedo,Tex2Color, splat_control.b);
				albedo = lerp(albedo,Tex3Color, splat_control.a);

				return albedo* _IllumFactor;
			}

		ENDCG
	}	
}
	FallBack "Diffuse"
}
