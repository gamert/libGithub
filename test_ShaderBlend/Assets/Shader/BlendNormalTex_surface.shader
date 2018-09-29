Shader "Custom/BlendNormalTex_surface" {
	Properties {
		_RTexture("Red Channel Texture", 2D) = "" {}
		_GTexture("Green Channel Texture", 2D) = "" {}
		_BTexture("Blue Channel Texture", 2D) = "" {}
		_ATexture("Alpha Channel Texture", 2D) = "" {}

		_RNormalTex ("RNormal Map", 2D) = "bump" {}
		_GNormalTex ("GNormal Map", 2D) = "bump" {}
		_BNormalTex ("BNormal Map", 2D) = "bump" {}
		_ANormalTex ("ANormal Map", 2D) = "bump" {}

        _RNormalIntensity ("RNormal Map Intensity", Range(-1,1)) = 1
		_GNormalIntensity ("GNormal Map Intensity", Range(-1,1)) = 1
		_BNormalIntensity ("BNormal Map Intensity", Range(-1,1)) = 1
		_ANormalIntensity ("ANormal Map Intensity", Range(-1,1)) = 1
		_IllumFactor ("Illumin Factor", Range(1,2)) = 1
		
        
		
		_Mask("Mask(RG)",2D) = ""{}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert
		// #pragma surface surf Lambert vertex:vert
		#pragma target 4.0

		sampler2D _RTexture,_GTexture,_BTexture,_ATexture;
		sampler2D _RNormalTex,_GNormalTex,_BNormalTex,_ANormalTex;
        float _RNormalIntensity,_GNormalIntensity,_BNormalIntensity,_ANormalIntensity;
		float _IllumFactor;

		sampler2D _Mask;

		struct Input {
			float2 uv_RTexture;
			float2 uv_GTexture;
			float2 uv_BTexture;
			float2 uv_ATexture;
			float2 uv_RNormalTex;
			float2 uv_GNormalTex;
			float2 uv_BNormalTex;
			float2 uv_ANormalTex;
			float2 uv_Mask;
		//	float3 m_normal;
		};
		void surf (Input IN, inout SurfaceOutput o) {
			float4 rTexData = tex2D(_RTexture, IN.uv_RTexture);
			float4 gTexData = tex2D(_GTexture, IN.uv_GTexture);
			float4 bTexData = tex2D(_BTexture, IN.uv_BTexture);
			float4 aTexData = tex2D(_ATexture, IN.uv_ATexture);
			float4 blendData = tex2D(_Mask, IN.uv_Mask);

			float4 finalColor;
			//根据blendData.g 将 RTexture 和 GTexture 混合
		    finalColor = lerp(rTexData, gTexData, blendData.g);
			//根据blendData.b 将 BTexture 混合
			finalColor = lerp(finalColor, bTexData, blendData.b);
			//根据blendData.a 将 ATexture 混合
			finalColor = lerp(finalColor, aTexData , blendData.a);
			finalColor = saturate(finalColor);


			float3 rNormalMap = UnpackNormal(tex2D(_RNormalTex, IN.uv_RNormalTex));
			rNormalMap.xy*=_RNormalIntensity;
			float3 gNormalMap = UnpackNormal(tex2D(_GNormalTex, IN.uv_GNormalTex));
			gNormalMap.xy*=_GNormalIntensity;
			float3 bNormalMap = UnpackNormal(tex2D(_BNormalTex, IN.uv_BNormalTex));
			bNormalMap.xy*=_BNormalIntensity;
			//bNormalMap.z=sqrt(1.0-saturate(dot(bNormalMap.xy,bNormalMap.xy)));这个似乎并没有效果
			float3 aNormalMap = UnpackNormal(tex2D(_ANormalTex, IN.uv_ANormalTex));
			aNormalMap.xy*=_ANormalIntensity;

			float3 finalNormal;
			//根据blendData.g 将 RTexture 和 GTexture 混合
		    finalNormal = lerp(rNormalMap, gNormalMap, blendData.g);
			//根据blendData.b 将 BTexture 混合
			finalNormal = lerp(finalNormal, bNormalMap, blendData.b);
			//根据blendData.a 将 ATexture 混合
			finalNormal = lerp(finalNormal, aNormalMap , blendData.a);

			o.Albedo =finalColor*_IllumFactor;
			o.Alpha = finalColor.a;
			o.Normal=finalNormal;
			//o.Normal=rNormalMap*blendData.r+gNormalMap*blendData.g+bNormalMap*blendData.b+aNormalMap*blendData.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}