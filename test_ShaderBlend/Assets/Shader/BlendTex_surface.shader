Shader "Custom/BlendTex_surface" {
	Properties {
		_RTexture("Red Channel Texture", 2D) = "" {}
		_GTexture("Green Channel Texture", 2D) = "" {}
		_BTexture("Blue Channel Texture", 2D) = "" {}
		_ATexture("Alpha Channel Texture", 2D) = "" {}
		_Mask("Mask(RG)",2D) = ""{}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert
		#pragma target 4.0

		sampler2D _RTexture;
		sampler2D _GTexture;
		sampler2D _BTexture;
		sampler2D _ATexture;

		sampler2D _Mask;

		struct Input {
			float2 uv_RTexture;
			float2 uv_GTexture;
			float2 uv_BTexture;
			float2 uv_ATexture;
			float2 uv_Mask;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			float4 rTexData = tex2D(_RTexture, IN.uv_RTexture);
			float4 gTexData = tex2D(_GTexture, IN.uv_GTexture);
			float4 bTexData = tex2D(_BTexture, IN.uv_BTexture);
			float4 aTexData = tex2D(_ATexture, IN.uv_ATexture);
			float4 blendData = tex2D(_Mask, IN.uv_Mask);

			float4 finalColor;
			//根据blendData.g 将 RTexture 和 GTexture 混合
		    finalColor = lerp(rTexData, gTexData, blendData.g);//原本为g
			//根据blendData.b 将 BTexture 混合
			finalColor = lerp(finalColor, bTexData, blendData.b);//原本为b
			//根据blendData.a 将 ATexture 混合
			finalColor = lerp(finalColor, aTexData , blendData.a);//原本为a
			finalColor = saturate(finalColor);
			o.Albedo = finalColor.rgb;
			o.Alpha = finalColor.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}