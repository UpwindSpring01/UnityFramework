Shader "Poisson/PoissonHelperShader" 
{
	Properties
	{
		_OuterColor("Outer Color", Color) = (1,1,1,1)
		_InnerColor("Inner Color", Color) = (1,1,1,1)
		_PercentageX("Percentage X", Range(0.0, 1.0)) = 0.0
		_PercentageZ("Percentage Z", Range(0.0, 1.0)) = 0.0
		[MaterialToggle] _IsEllipse("Is Ellipse", Int) = 0
		[MaterialToggle] _UseMap("UseMap", Int) = 0
		[NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}

	}
		SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 200

		Cull Back
		ZWrite Off
		BlendOp Add
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard alpha:blend nolightmap noshadow nofog nometa noshadowmask

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		float _PercentageX;
		float _PercentageZ;
		int _IsEllipse;
		int _UseMap;
		fixed4 _OuterColor;
		fixed4 _InnerColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float halfX = _PercentageX * 0.5;
			float halfZ = _PercentageZ * 0.5;
			fixed4 c;
			if (
				(
					_IsEllipse == 1 &&
					(
						((((IN.uv_MainTex.x - 0.5) * (IN.uv_MainTex.x - 0.5)) / (halfX * halfX)) + ((IN.uv_MainTex.y - 0.5) * (IN.uv_MainTex.y - 0.5)) / (halfZ * halfZ)) < 1.0f
					)
				)
				||
				(
					_IsEllipse == 0 &&
					(
						(IN.uv_MainTex.x >= 0.5 - halfX) && (IN.uv_MainTex.x <= 0.5 + halfX) && (IN.uv_MainTex.y >= 0.5 - halfZ) && (IN.uv_MainTex.y <= 0.5 + halfZ)
					)
				)
				)
			{
				c = _InnerColor;
			}
			else
			{
				if (_UseMap)
				{
					c.rgb = tex2D(_MainTex, IN.uv_MainTex).rgb;
					c.a = _OuterColor.a;
				}
				else
				{
					c = _OuterColor;
				}
			}

			o.Albedo = c.rgb;
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = c.a;
		}
		ENDCG
	}
}
