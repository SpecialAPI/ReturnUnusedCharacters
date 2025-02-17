Shader "Brave/Alpha Mask Mood Renderer"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "" { }
	}
	SubShader
	{
		LOD 200
		Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }

		Pass
		{
			LOD 200
			Tags { "QUEUE" = "Transparent" "RenderType" = "Transparent" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 in_POSITION0 : POSITION;
				float4 in_TEXCOORD0 : TEXCOORD0;
				float4 in_COLOR0 : COLOR0;
			};

			struct FragmentInput
			{
				float4 gl_Position : SV_POSITION;
				float2 vs_TEXCOORD0 : TEXCOORD0;
				float4 vs_COLOR0 : COLOR0;
			};

			float4 _MainTex_ST;
			sampler2D _MainTex;

			FragmentInput vert(VertexInput input)
			{
				FragmentInput output;

				float4 u_xlat0;
				float4 u_xlat1;

				u_xlat0 = input.in_POSITION0.yyyy * unity_ObjectToWorld[1];
				u_xlat0 = unity_ObjectToWorld[0] * input.in_POSITION0.xxxx + u_xlat0;
				u_xlat0 = unity_ObjectToWorld[2] * input.in_POSITION0.zzzz + u_xlat0;
				u_xlat0 = u_xlat0 + unity_ObjectToWorld[3];
				u_xlat1 = u_xlat0.yyyy * UNITY_MATRIX_VP[1];
				u_xlat1 = UNITY_MATRIX_VP[0] * u_xlat0.xxxx + u_xlat1;
				u_xlat1 = UNITY_MATRIX_VP[2] * u_xlat0.zzzz + u_xlat1;
				output.gl_Position = UNITY_MATRIX_VP[3] * u_xlat0.wwww + u_xlat1;
				output.vs_TEXCOORD0.xy = input.in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				output.vs_COLOR0 = input.in_COLOR0;

				return output;
			}

			fixed4 frag(FragmentInput output) : SV_Target
			{
				fixed4 finalOutput;

				float4 u_xlat10_0;

				u_xlat10_0 = tex2D(_MainTex, output.vs_TEXCOORD0.xy);
				finalOutput = u_xlat10_0.wwww * float4(-1.0, -1.0, -1.0, 1.0) + float4(1.0, 1.0, 1.0, 0.0);

				return finalOutput;
			}

			ENDCG
		}
	}
}
