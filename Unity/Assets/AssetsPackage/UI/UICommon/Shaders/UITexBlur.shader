Shader "Common/UITexBlur"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_BlurRadius("Blur Radius", Range(0, 8)) = 3.0
		_BlurTex("Blur Texture", 2D) = "white" {}
	}
 
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas" = "True" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
 
		Pass
		{
			Tags { "LightMode" = "UniversalForward" }
			
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			
			HLSLPROGRAM
			#pragma only_renderers gles3 glcore d3d11 metal vulkan
			#pragma target 3.0
			
			#pragma vertex UIVert
			#pragma fragment UIFrag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			TEXTURE2D(_MainTex);		            SAMPLER(sampler_MainTex);
			TEXTURE2D(_BlurTex);		            SAMPLER(sampler_BlurTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;
				float4 _Color;
				float _BlurRadius;
			CBUFFER_END
 
			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 texcoord		: TEXCOORD0;
			};
 
			struct Varyings
			{
				float4 positionCS   : SV_POSITION;
				float2 uv			: TEXCOORD0;
			};
 
			Varyings UIVert(Attributes input)
			{
				Varyings output = (Varyings)0;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.texcoord; 
				return output;
			}				
 
			half4 UIFrag(Varyings input) : SV_Target
			{
				half4 color = half4(0,0,0,0);
				// 采样的25个像素点的偏移量
				float2 offsets[25] = 
				{	
					float2(-2, 2), float2(-1, 2), float2(0, 2), float2(1, 2), float2(2, 2),
					float2(-2, 1), float2(-1, 1), float2(0, 1), float2(1, 1), float2(2, 1),
					float2(-2, 0), float2(-1, 0), float2(0, 0), float2(1, 0), float2(2, 0),
					float2(-2, -1), float2(-1, -1), float2(0, -1), float2(1, -1), float2(2, -1),
					float2(-2, -2), float2(-1, -2), float2(0, -2), float2(1, -2), float2(2, -2),
				};
				float pi = 3.1415;	// 圆周率
				float e = 2.7182;	// 数学常数
				float sum = 0;
				float weight[25];
				// 卷积
				for (int j = 0; j < 25; j++)
				{
					float l = length(_BlurRadius * _MainTex_TexelSize * offsets[j]);	// 求距离
					float g = (1.0 / (2.0 * pi * pow(1.5, 2.0))) * pow(e, (-(l * l) / (2.0 * pow(1.5, 2.0))));	// 高斯函数值
					weight[j] = g;
					sum += g;
				}

				for (int j = 0; j < 25; j++)
					weight[j] /= sum;
				half blurScale = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, input.uv).a;

				for (int j = 0; j < 25; j++)
				{
					color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv +  blurScale * _BlurRadius * _MainTex_TexelSize * offsets[j]) * weight[j];
				}
				return color * _Color;
			}
			ENDHLSL
		}
	}
}