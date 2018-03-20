Shader "Magic Leap/Wireframe"
{
	Properties
	{
		_Color("Wire Color", Color) = (0, 1, 0, 1)
		_Speed("Speed", Range(0, 10)) = 0.5
		_WorkArea("Work Area", Range(0, 10)) = 2
		_Distance("Distance", Range(1,10)) = 3
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4 _Color;
			float _Speed;
			float _WorkArea;
			float _Distance;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2g
			{
				float4 projectionSpaceVertex : SV_POSITION;
				float4 world : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2g vert(appdata v)
			{
				v2g o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex);
				o.world = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			float ScaleGrid(float3 pos, float angle, float scale)
			{
				float a = (radians(angle));
				float2 c = (pos.xy + float2(0.0, pos.z)) * float2(sin(a), 1.0) / 2;

				// Rotate
				c = ((c + float2(c.y, 0.0)*cos(a)) / scale) + float2(floor((c.x - c.y * cos(a)) / scale * 4.0) / 4.0, 0.0);

				// Align
				float v1 = min((1.2 - (2.0 * abs(frac((c.x - c.y)* 4.0) - 0.4))), (1.2 - (2.0 * abs(frac(c.y * 4) - 0.4))));
				float v2 = min(v1, (1.2 - (2.0 * abs(frac(c.x * 4) - 0.4))));

				return v2;
			}

			fixed4 frag(v2g i) : SV_Target
			{
				float d = (length(i.world.xyz.xz - _WorldSpaceCameraPos.xz) + i.world.xyz.y - _WorldSpaceCameraPos.y);
				float border = (fmod(_Time.y * _Speed, _Distance) - 1) + _WorkArea;

				// Grid
				float3 grid = ScaleGrid(i.world.xyz, 90, 0.5);
				grid = lerp(0.5, smoothstep(0.15, 0.05, grid), 0.5);

				// Ring
				float3 effect = float3(1.0, 1.0, 1.0) * smoothstep(border - 0.25, border, d);

				effect += grid;
				fixed4 finalColor = _Color;

				// Front (Cutoff)
				effect *= smoothstep(border, border - 0.02, d);

				// Back (Cutoff)
				effect *= smoothstep(border - 10, border, d);

				// Fade
				effect *= smoothstep(1, 4.0, border);

				return fixed4(finalColor.rgb * effect, finalColor.a);
			}
			ENDCG
		}
	}
}