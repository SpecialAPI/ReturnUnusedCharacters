Shader "Brave/Internal/RainbowChestShader"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" { }
		_OverrideColor ("Override Color", Color) = (1,1,1,0)
		_Perpendicular ("Is Perpendicular Tilt", Float) = 1
		_ValueMaximum ("Value Maximum", Float) = 1
		_AllColorsToggle ("All Colors", Float) = 0
		_HueTestValue ("Hue Test Value", Float) = 50
		_HiddenRainbow ("Hidden Rainbow", Float) = 0
	}
	SubShader
	{
		LOD 110
 		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "TransparentCutout" "UnlitTilted" = "UnlitTilted" }

		Pass
		{
			LOD 110
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent" "RenderType" = "TransparentCutout" "UnlitTilted" = "UnlitTilted" }
			Cull Off
			Fog {
				Mode Off
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 uv2 : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float4 color : COLOR0;
			};

			float _ReflectionYFactor;
			float _ReflectionYOffset;
			float4 _OverrideColor;
			float _MapActive;
			float _HueTestValue;
			float _AllColorsToggle;
			float _HiddenRainbow;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				float4 u_xlat0;
				float4 u_xlat1;
				float4 u_xlat2;
				float u_xlat3;

				u_xlat0.x = _ReflectionYOffset * _ReflectionYFactor;
				u_xlat0.x = u_xlat0.x * 0.5;
				u_xlat3 = (-_ReflectionYFactor) + 1.0;
				u_xlat0.y = v.vertex.y * u_xlat3 + u_xlat0.x;
				u_xlat1 = u_xlat0.yyyy * unity_ObjectToWorld[1];
				u_xlat1 = unity_ObjectToWorld[0] * v.vertex.xxxx + u_xlat1;
				u_xlat1 = unity_ObjectToWorld[2] * v.vertex.zzzz + u_xlat1;
				u_xlat1 = u_xlat1 + unity_ObjectToWorld[3];
				u_xlat2 = u_xlat1.yyyy * UNITY_MATRIX_VP[1];
				u_xlat2 = UNITY_MATRIX_VP[0] * u_xlat1.xxxx + u_xlat2;
				u_xlat2 = UNITY_MATRIX_VP[2] * u_xlat1.zzzz + u_xlat2;
				o.vertex = UNITY_MATRIX_VP[3] * u_xlat1.wwww + u_xlat2;
				o.color = v.color;
				o.uv.xy = v.uv.xy;
				u_xlat0.xzw = v.vertex.xzw;
				o.uv2 = u_xlat0 * float4(0.0166666675, 0.0166666675, 0.0166666675, 0.0166666675);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 u_xlat0;
				float4 u_xlat10_0;
				float4 u_xlat1;
				bool u_xlatb1;
				float4 u_xlat2;
				float3 u_xlat3;
				float u_xlat5;
				float2 u_xlat9;
				float u_xlat12;
				bool u_xlatb12;

				u_xlat10_0 = tex2D(_MainTex, i.uv);
				u_xlat1.x = u_xlat10_0.w * i.color.w + -0.100000001;
				u_xlatb1 = u_xlat1.x<0.0;
				if (u_xlatb1)
					discard;
				u_xlat1.z = float(-1.0);
				u_xlat1.w = float(0.666666687);
				u_xlat2.z = float(1.0);
				u_xlat2.w = float(-1.0);
				u_xlat3.xyz = (-u_xlat10_0.xyz) * i.color.xyz + _OverrideColor.xyz;
				u_xlat0 = u_xlat10_0 * i.color;
				u_xlat0.xyz = _OverrideColor.www * u_xlat3.xyz + u_xlat0.xyz;
				float4 SV_Target0;
				SV_Target0.w = u_xlat0.w;
				u_xlatb12 = u_xlat0.y >= u_xlat0.z;
				u_xlat12 = u_xlatb12 ? 1.0 : float(0.0);
				u_xlat1.xy = u_xlat0.zy;
				u_xlat2.xy = u_xlat0.yz + (-u_xlat1.xy);
				u_xlat1 = u_xlat12 * u_xlat2.xywz + u_xlat1.xywz;
				u_xlat2.z = u_xlat1.w;
				u_xlatb12 = u_xlat0.x >= u_xlat1.x;
				u_xlat12 = u_xlatb12 ? 1.0 : float(0.0);
				u_xlat1.w = u_xlat0.x;
				u_xlat2.xyw = u_xlat1.wyx;
				u_xlat2 = (-u_xlat1) + u_xlat2;
				u_xlat1 = u_xlat12 * u_xlat2 + u_xlat1;
				u_xlat12 = min(u_xlat1.y, u_xlat1.w);
				u_xlat12 = (-u_xlat12) + u_xlat1.x;
				u_xlat12 = u_xlat12 * 6.0 + 1.00000001e-010;
				u_xlat5 = (-u_xlat1.y) + u_xlat1.w;
				u_xlat12 = u_xlat5 / u_xlat12;
				u_xlat12 = u_xlat12 + u_xlat1.z;
				u_xlat1.x = u_xlat1.x + -0.100000001;
				u_xlat1.x = u_xlat1.x * 1000.0;
				u_xlat1.x = clamp(u_xlat1.x, 0.0, 1.0);
				u_xlat12 = -abs(u_xlat12) * abs(_HueTestValue) + 1.0;
				u_xlat12 = max(u_xlat12, 0.0);
				u_xlat5 = u_xlat1.x * u_xlat12;
				u_xlat12 = (-u_xlat12) * u_xlat1.x + 1.0;
				u_xlatb1 = _HueTestValue<0.0;
				u_xlat12 = (u_xlatb1) ? u_xlat12 : u_xlat5;
				u_xlat12 = _HiddenRainbow * (-u_xlat12) + u_xlat12;
				u_xlat1.xy = (-i.uv.xy) + i.uv.xy;
				u_xlat1.xy = float2(float2(_AllColorsToggle, _AllColorsToggle)) * u_xlat1.xy + i.uv.xy;
				u_xlat9.xy = float2(_MapActive, _AllColorsToggle) * float2(-0.300000012, -1.5) + float2(0.5, 3.0);
				u_xlat1.xy = u_xlat1.xy / u_xlat9.yy;
				u_xlat1.x = _Time.x * 0.800000012 + u_xlat1.x;
				u_xlat1.xy = u_xlat9.xx * u_xlat1.xy;
				u_xlat1.xy = u_xlat1.xy * float2(150.0, 150.0);
				u_xlat1.x = sin(u_xlat1.x);
				u_xlat5 = cos(u_xlat1.y);
				u_xlat1.x = u_xlat5 + u_xlat1.x;
				u_xlat1.x = u_xlat1.x + _Time.y;
				u_xlat1.x = frac(u_xlat1.x);
				u_xlat1.xyz = u_xlat1.xxx + float3(1.0, 0.666666687, 0.333333343);
				u_xlat1.xyz = frac(u_xlat1.xyz);
				u_xlat1.xyz = u_xlat1.xyz * float3(6.0, 6.0, 6.0) + float3(-3.0, -3.0, -3.0);
				u_xlat1.xyz = abs(u_xlat1.xyz) + float3(-1.0, -1.0, -1.0);
				u_xlat1.xyz = clamp(u_xlat1.xyz, 0.0, 1.0);
				u_xlat2.xyz = u_xlat1.xyz * float3(2.5, 2.5, 2.5) + (-u_xlat0.xyz);
				u_xlat2.xyz = (u_xlat12) * u_xlat2.xyz + u_xlat0.xyz;
				u_xlat0.x = dot(u_xlat0.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
				u_xlat0.xyz = u_xlat1.xyz * u_xlat0.xxx;
				u_xlat0.xyz = u_xlat0.xyz + u_xlat0.xyz;
				u_xlatb12 = 0.5<_AllColorsToggle;
				SV_Target0.xyz = (u_xlatb12) ? u_xlat0.xyz : u_xlat2.xyz;

				return SV_Target0;
			}
			ENDCG
		}
	}
}
