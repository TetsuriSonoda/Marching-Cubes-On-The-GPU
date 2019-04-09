// Modified by T. Sonoda
// Released under the MIT license.
// see https ://opensource.org/licenses/MIT

// Original code is:
// Copyright (c) Scrawk.
// Released under the MIT license.

Shader "MarchingCubesGPUProject/DrawStructuredBuffer" 
{
	SubShader 
	{
		Pass 
		{
			Cull back

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			struct Vert
			{
				float4 position;
				float3 normal;
			};

			uniform StructuredBuffer<Vert> _Buffer;
			uniform float4x4 model;

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float3 normal : NORMAL;
			};

			v2f vert(uint id : SV_VertexID)
			{
				Vert vert = _Buffer[id];

				v2f OUT;
//				o.normal = mul(unity_ObjectToWorld, triangles[pid].v[vid].vNormal);

//				OUT.pos = UnityObjectToClipPos(float4(vert.position.xyz, 1));
				OUT.pos = mul(UNITY_MATRIX_VP, mul(model, float4(vert.position.xyz, 1)));
				OUT.normal = mul(unity_ObjectToWorld, vert.normal);

//				OUT.col = dot(float3(0,1,0), vert.normal) * 0.5 + 0.5;
				
				return OUT;
			}

			float4 frag(v2f IN) : COLOR
			{
				float d = max(dot(normalize(_WorldSpaceLightPos0.xyz), IN.normal), 0);
				return float4(d, d, d,1);
			}

			ENDCG

		}
	}
}