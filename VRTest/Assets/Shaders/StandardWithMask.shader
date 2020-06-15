Shader "StandardWithMask"
{
	Properties
	{
		// Standard
		_MainTex ("Albedo", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_MetallicMap ("Metallic map", 2D) = "white" {}
		_Normal ("Normal", Range(0,1)) = 0.0
		_NormalMap ("Normal map", 2D) = "bump" {}
		_Occlusion ("Occlusion", Range(0,1)) = 0.0
		_OcclusionMap ("Occlusion map", 2D) = "white" {}
		_EmissionToggle ("Emission toggle", Float) = 0.0
		_EmissionColor ("Emission color", Color) = (1,1,1,1)
		_EmissionMap ("Emission map", 2D) = "white" {}
		_Cutout ("Alpha cutout", Range(0, 1)) = 0.0

		// Destruction
		_Mask ("Opacity", Range(0,1)) = 0.0
		_MaskMap ("Mask map", 2D) = "white" {}
		_MaskTex ("Mask texture", 2D) = "white" {}
		_MaskMetallic ("Mask metallic", Range(0,1)) = 0.0
		_MaskMetallicMap ("Mask metallic map", 2D) = "white" {}
		_MaskNormal ("Mask normal", Range(0,1)) = 0.0
		_MaskNormalMap ("Mask normal map", 2D) = "bump" {}
		_MaskNormalDent ("Mask normal (dent)", Range(0,1)) = 0.0
		_MaskNormalDentScale ("Mask normal scale (dent)", Float) = 0.0
		_MaskNormalDentMap ("Mask normal map (dent)", 2D) = "bump" {}
		_Treshold ("Blend trehshold", Range(0.01,1)) = 0.01

		// Stains
		_StainOpacity ("Stain opacity", Range(0,1)) = 1.0
		_StainMaskMap ("Stain mask", 2D) = "alpha" {}
		_StainMaskNormalMap ("Stain mask normal map", 2D) = "bump" {}
		_StainMaskNormal ("Stain mask normal", Range(0,1)) = 0.7
		_StainMaskNormalFill ("Stain mask normal fill opacity", Range(0,1)) = 0.3
		_StainMaskNormalFillMap ("Stain mask normal fill map", 2D) = "bump" {}
		_StainMaskSmooth ("Stain mask normal smoothness", Range(0,1)) = 0.5
		_StainMaskBrightness ("Brightness", Range(0,2)) = 1.0
		_StainMaskContrast ("Contrast", Range(0,2)) = 1.0

		// Triplanar
		_Triplanar ("Triplanar toggle", Float) = 0.0
		_Scale ("Scale", Float) = 0.0
		_ScaleTUV ("Scale TUV", Float) = 0.0
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
		}

		LOD 200

		CGPROGRAM

		#pragma surface surf Standard
		#pragma target 4.0

		UNITY_DECLARE_TEX2D(_MainTex);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_NormalMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_OcclusionMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap);
		sampler2D _MaskMap;
		sampler2D _MaskTex;
		sampler2D _MaskMetallicMap;
		sampler2D _MaskNormalMap;
		sampler2D _MaskNormalDentMap;
		UNITY_DECLARE_TEX2D(_StainMaskMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_StainMaskNormalMap);
		UNITY_DECLARE_TEX2D(_StainMaskNormalFillMap);
		
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;

			float2 uv_MainTex;
			float2 uv_StainMaskMap;
			float2 uv_StainMaskNormalFillMap;

			INTERNAL_DATA
		};

		float4 _Color;
		float4 _EmissionColor;
		float _Metallic;
		float _Normal;
		float _MaskNormal;
		float _MaskNormalDent;
		float _MaskNormalDentScale;
		float _Occlusion;
		float _Mask;
		float _MaskMetallic;
		float _EmissionToggle;
		float _Cutout;
		float _Treshold;
		float _Scale;
		float _ScaleTUV;
		float _Triplanar;
		float _StainOpacity;
		float _StainMaskNormal;
		float _StainMaskNormalFill;
		float _StainMaskSmooth;
		float _StainMaskBrightness;
		float _StainMaskContrast;

		// Blend two textures
		float4 blend(float4 texture1, float4 texture2, float p, float m, float treshold)
		{
			float a1 = 1 - ((p - m) / treshold);
			float a2 = (p - m) / treshold;

			return texture2 * a1 + texture1 * a2;
		}

		// Triplanar for textures
		float4 triplanar(Input IN, SurfaceOutputStandard o, sampler2D tex, float scale)
		{
			float3 n = WorldNormalVector(IN, o.Normal);
			float3 projNormal = saturate(pow(n * 1.4, 4));

			float4 x = (tex2D(tex, frac(IN.worldPos.zy * scale)) * abs(n.x)) * _Triplanar + tex2D(tex, frac(IN.uv_MainTex * scale)) * (1 - _Triplanar);
			float4 y = (tex2D(tex, frac(IN.worldPos.zx * scale)) * abs(n.y)) * _Triplanar + tex2D(tex, frac(IN.uv_MainTex * scale)) * (1 - _Triplanar);
			float4 z = (tex2D(tex, frac(IN.worldPos.xy * scale)) * abs(n.z)) * _Triplanar + tex2D(tex, frac(IN.uv_MainTex * scale)) * (1 - _Triplanar);

			float4 color = z;
			color = lerp(color, x, projNormal.x);
			color = lerp(color, y, projNormal.y);

			return color;
		}

		// Triplanar for normal maps
		float3 triplanarNormal(Input IN, SurfaceOutputStandard o, sampler2D tex, float scale, float scaleNormal)
		{
			float3 n = WorldNormalVector(IN, o.Normal);
			float3 projNormal = saturate(pow(n * 1.4, 4));

			float3 x = UnpackScaleNormal(tex2D(tex, frac(IN.worldPos.zy * scale)) * abs(n.x), scaleNormal) * _Triplanar + 
					  UnpackScaleNormal(tex2D(tex, frac(IN.uv_MainTex * scale)), scaleNormal) * (1 - _Triplanar);
			float3 y = UnpackScaleNormal(tex2D(tex, frac(IN.worldPos.zx * scale)) * abs(n.y), scaleNormal) * _Triplanar + 
					  UnpackScaleNormal(tex2D(tex, frac(IN.uv_MainTex * scale)), scaleNormal) * (1 - _Triplanar);
			float3 z = UnpackScaleNormal(tex2D(tex, frac(IN.worldPos.xy * scale)) * abs(n.z), scaleNormal) * _Triplanar + 
					  UnpackScaleNormal(tex2D(tex, frac(IN.uv_MainTex * scale)), scaleNormal) * (1 - _Triplanar);

			float3 color = z;
			color = lerp(color, x, projNormal.x);
			color = lerp(color, y, projNormal.y);

			return color;
		}

		// Mix normal in correct way
		float3 mixNormal(float3 normal1, float3 normal2)
		{
			float x = normal1.x + normal2.x;
			float y = normal1.y + normal2.y;
			float z = normal1.z;

			return float3(x, y, z);
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// Default material
			float4 color = UNITY_SAMPLE_TEX2D(_MainTex, IN.uv_MainTex) * _Color;

			// Alpha cutout
			clip(step(_Cutout, color.a) == 0 ? -1 : 1);

			float4 metallic = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMap, _MainTex, IN.uv_MainTex);
			float3 normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_NormalMap, _MainTex, IN.uv_MainTex), _Normal);
			float4 occlusion = lerp(1.0, UNITY_SAMPLE_TEX2D_SAMPLER(_OcclusionMap, _MainTex, IN.uv_MainTex), _Occlusion);
			float4 emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, IN.uv_MainTex) * _EmissionColor;
			
			// Stains
			float4 stainMask = UNITY_SAMPLE_TEX2D(_StainMaskMap, IN.uv_StainMaskMap) * _StainOpacity;

			stainMask.rgb = stainMask.rgb * _StainMaskBrightness;
			stainMask.rgb = (stainMask.rgb - 0.5) * _StainMaskContrast + 0.5;

			float3 stainMaskNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_StainMaskNormalMap, _StainMaskMap, IN.uv_StainMaskMap), _StainMaskNormal);
			float3 stainNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D(_StainMaskNormalFillMap, IN.uv_StainMaskNormalFillMap), _StainMaskNormalFill);

			// Mask triplanar
			float scale = _ScaleTUV + _Triplanar * (_Scale - _ScaleTUV);
			float4 maskTexture = triplanar(IN, o, _MaskTex, scale);
			float4 mask = triplanar(IN, o, _MaskMap, scale);
			float4 metallicMask = triplanar(IN, o, _MaskMetallicMap, scale);
			float3 normalMask = triplanarNormal(IN, o, _MaskNormalMap, scale, _MaskNormal);
			float maskNormalDentScale = _MaskNormalDent * (_Mask - .5) * step(.5, _Mask);
			float3 normalMaskDent = triplanarNormal(IN, o, _MaskNormalDentMap, scale * _MaskNormalDentScale, maskNormalDentScale);
			float repMaskAlpha = maskTexture.a * step(.2, maskTexture.a);
			
			// Triplanar toggle
			mask = mask.r * repMaskAlpha;

			// Invert mask value
			_Mask = 1 - _Mask;
			
			// Tresholds
			float stepValue = step(_Mask, mask) * repMaskAlpha;
			float stepTresholdValue = step(_Mask, mask - _Treshold);

			// Blended colors
			float4 blendColor = blend(maskTexture, color, mask, _Mask, _Treshold);
			float4 blendMetallic = blend(metallicMask, metallic, mask, _Mask, _Treshold);
			float4 blendSmoothness = blend(metallicMask.a * _MaskMetallic, metallic.a * _Metallic, mask, _Mask, _Treshold);
			float3 blendNormal = blend(float4(normalMask, 1.0), float4(normal, 1.0), mask, _Mask, _Treshold);
			float3 blendNormalDent = blend(float4(normalMaskDent, 1.0), float4(normal, 1.0), mask, _Mask, _Treshold);

			o.Albedo = lerp(color, lerp(blendColor, maskTexture, stepTresholdValue), stepValue);
			o.Albedo = lerp(o.Albedo, stainMask, stainMask.a);
			o.Metallic = lerp(metallic, lerp(blendMetallic, metallicMask, stepTresholdValue), stepValue);
			o.Smoothness = lerp(metallic.a * _Metallic, lerp(blendSmoothness, metallicMask.a * _MaskMetallic, stepTresholdValue), stepValue);
			o.Smoothness = lerp(o.Smoothness, _StainMaskSmooth, stainMask.a);
			o.Normal = lerp(normal, lerp(blendNormal.xyz, normalMask, stepTresholdValue), stepValue);
			o.Normal = lerp(mixNormal(o.Normal, normalMaskDent), mixNormal(stainNormal, stainMaskNormal), stainMask.a);
			o.Occlusion = occlusion;
			o.Emission = emission * _EmissionToggle;
			o.Alpha = color.a;
		}

		ENDCG
	}

	CustomEditor "StandardWithMaskShaderGUI"
	FallBack "Standard"
}