Shader "4ik0/FakeWindow/FakeWindowDetail" {
	Properties {
		_Color ("Color Tint", Color) = (1,1,1,1)
		_MainTex ("Outside Window (RGB)", 2D) = "white" {}
		_Detail("Detail (RGB) Alpha (A)", 2D) = "black" {}
		_BumpMap("Bumpmap (RGB)", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_EmissionColor("Emission Color", Color) = (0,0,0)
		_EmissionMap("Emission Texture", 2D) = "white" {}
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Scale("Simulated Distance", Range(0, 1)) = 0.1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Detail;
		sampler2D _BumpMap;
		half _Scale;
		half _Glossiness;
		half _Metallic;
		half4 _EmissionColor;
		sampler2D _EmissionMap;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_Detail;
			float2 uv_BumpMap;
			float2 uv_EmissionMap;
			half3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			half4 bump = tex2D(_BumpMap, IN.uv_BumpMap);
			half3 norm = UnpackNormal(bump);
			half scale = (1 - _Scale);
			half coeff = dot(abs(normalize(IN.viewDir)), norm);
			half3 offset = (IN.viewDir + norm) * scale / 2 * (2 - coeff);
			half2 uv = half2(IN.uv_MainTex.x * _Scale + scale / 2 - offset.x, IN.uv_MainTex.y * _Scale + scale / 2 - offset.y);
			half4 color = tex2D (_MainTex, uv) * _Color;
			half4 emiss = tex2D(_EmissionMap, IN.uv_EmissionMap) * _EmissionColor;
			half4 detail = tex2D(_Detail, IN.uv_Detail) * _Color;
			o.Normal = norm;
			o.Albedo = lerp(color.rgb * pow(coeff, 2), detail, detail.a);
			o.Metallic = _Metallic;
			o.Emission = emiss;
			o.Smoothness = _Glossiness;

			o.Alpha = 1;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
