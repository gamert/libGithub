// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/BlendNormalTex_vertfrag" 
{
Properties 
{
	Tex0 ("Layer 0 (R)", 2D) = "white" {}
	Tex1 ("Layer 1 (G)", 2D) = "white" {}
	Tex2 ("Layer 2 (G)", 2D) = "white" {}
	Tex3 ("Layer 3 (A)", 2D) = "white" {}

	_Normal0("Normal 0 (A)", 2D) = "bump" {}
	_Normal1("Normal 1 (B)", 2D) = "bump" {}
	_Normal2("Normal 2 (G)", 2D) = "bump" {}
	_Normal3("Normal 3 (R)", 2D) = "bump" {}

	_BumpScale0("BumpScale 0", Range(-1.0,1.0)) = 1.0
	_BumpScale1("BumpScale 1", Range(-1.0, 1.0)) = 1.0
	_BumpScale2("BumpScale 2", Range(-1.0, 1.0)) = 1.0
	_BumpScale3("BumpScale 3", Range(-1.0, 1.0)) = 1.0
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
			#include "Lighting.cginc"

			sampler2D _Control;
			//贴图及纹理
			sampler2D Tex0,Tex1,Tex2,Tex3;
			half4 Tex0_ST,Tex1_ST,Tex2_ST,Tex3_ST;
			//法线贴图及纹理
			sampler2D _Normal0,_Normal1,_Normal2,_Normal3;
			half4 _Normal0_ST,_Normal1_ST,_Normal2_ST,_Normal3_ST;
			half _BumpScale0, _BumpScale1, _BumpScale2, _BumpScale3;

			half _IllumFactor;
			half4 _Control_ST;


			struct v2f
			{
				float4	pos : SV_POSITION;
				float3 normal:NORMAL;
				float4 tangnent:TANGENT;
				half4  uv_Tex0:TEXCOORD0;
				half4  uv_Tex1:TEXCOORD1;
				half4  uv_Tex2:TEXCOORD2;
				half4  uv_Tex3:TEXCOORD3;
				half2  uv_Control:TEXCOORD4;
				half3 lightDir:TEXCOORD5;
				half3 viewDir:TEXCOORD6;
			}; 
			
			struct appdata
			{
			    float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 tangent:TANGENT;
				float3 normal:NORMAL;
			};


			v2f vert (appdata v)
			{
				v2f o;
			    o.pos =  UnityObjectToClipPos(v.vertex);
				o.uv_Control = v.texcoord.xy;
				o.uv_Tex0.xy = TRANSFORM_TEX(v.texcoord ,Tex0);
				o.uv_Tex0.zw = TRANSFORM_TEX(v.texcoord ,_Normal0);

				o.uv_Tex1.xy = TRANSFORM_TEX(v.texcoord ,Tex1);
				o.uv_Tex1.zw = TRANSFORM_TEX(v.texcoord ,_Normal1);

				o.uv_Tex2.xy = TRANSFORM_TEX(v.texcoord, Tex2);
				o.uv_Tex2.zw = TRANSFORM_TEX(v.texcoord ,_Normal2);
				
				o.uv_Tex3.xy = TRANSFORM_TEX(v.texcoord, Tex3);
				o.uv_Tex3.zw = TRANSFORM_TEX(v.texcoord ,_Normal3);

				TANGENT_SPACE_ROTATION;
				o.lightDir=mul(rotation,ObjSpaceLightDir(v.vertex)).xyz;
				o.viewDir=mul(rotation,ObjSpaceViewDir(v.vertex)).xyz;

				return o;
			}
			fixed4 frag(v2f i):SV_Target
			{
				fixed3 tangentLightDir=normalize(i.lightDir);
				fixed3 tangentViewDir=normalize(i.viewDir);//暂时没用到,用于补充高光


				fixed4 splat_control = tex2D (_Control, i.uv_Control.xy);


				fixed3 tangentNormal0=UnpackNormal(tex2D(_Normal0, i.uv_Tex0.zw));
				tangentNormal0.xy*=_BumpScale0;

			    fixed3 tangentNormal1=UnpackNormal(tex2D(_Normal1, i.uv_Tex1.zw));
				tangentNormal1.xy*=_BumpScale1;

				  fixed3 tangentNormal2=UnpackNormal(tex2D(_Normal2, i.uv_Tex2.zw));
				tangentNormal2.xy*=_BumpScale2;

				  fixed3 tangentNormal3=UnpackNormal(tex2D(_Normal3, i.uv_Tex3.zw));
				tangentNormal3.xy*=_BumpScale3;

				fixed4 albedo = 0.0f;

				fixed4 Tex0Color = tex2D(Tex0, i.uv_Tex0);
				fixed4 Tex1Color = tex2D(Tex1, i.uv_Tex1);
				fixed4 Tex2Color = tex2D(Tex2, i.uv_Tex2);
				fixed4 Tex3Color = tex2D(Tex3, i.uv_Tex3);

				albedo = lerp(Tex0Color*max(0, dot(tangentNormal1, tangentLightDir)),Tex1Color*max(0, dot(tangentNormal1, tangentLightDir)), splat_control.g);
				albedo = lerp(albedo,Tex2Color*max(0, dot(tangentNormal2, tangentLightDir)), splat_control.b);
				albedo = lerp(albedo,Tex3Color*max(0, dot(tangentNormal3, tangentLightDir)), splat_control.a);

				return albedo*_IllumFactor;
			}
		ENDCG
	}	
}
FallBack "Diffuse"
}
