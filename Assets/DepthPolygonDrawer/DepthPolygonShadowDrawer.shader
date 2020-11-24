Shader "Custom/DepthPolygonShadowDrawer"
{
	Properties
	{
		_SpriteTex("RGBTexture", 2D) = "white" {}
		_DepthTex("DepthTexture", 2D) = "white" {}
		_FX("Focal length X", Range(0, 100)) = 7.584
		_FY("Focal length Y", Range(0, 100)) = 6.132
		_PPX("Center X", Range(0, 10000)) = 0.5
		_PPY("Center Y", Range(0, 10000)) = 0.5
		_WindowX("Window X", Range(0, 1)) = 0.005
		_WindowY("Window Y", Range(0, 1)) = 0.006

		_DiffThreshold("DiffThreshold", Range(0, 10)) = 0.2
		_PolygonQuality("PolygonQuality", Int) = 1
		_ShadowPolygonQuality("ShadowPolygonQuality", Int) = 1
		_ScaleBias("ScaleBias", Range(0, 1000)) = 0.001
		_OffsetZ("Offset Z", Range(-10, 10)) = 0
		_MedianSize("MedianSize", Range(0, 1)) = 0.001
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv	  : TEXCOORD0;
			};

			struct v2g
			{
				float4 vertex : POSITION;
				float2 uv	  : TEXCOORD0;
			};

			struct g2f
			{
				float4 vertex : SV_POSITION;
				float2 uv	  : TEXCOORD0;
				float2 uv2	  : TEXCOORD1;
			};

			sampler2D _SpriteTex;
			sampler2D _DepthTex;
			float4 _SpriteTex_ST;
			float _FX;
			float _FY;
			float _PPX;
			float _PPY;
			float _WindowX;
			float _WindowY;
			float _DiffThreshold;
			float _ScaleBias;
			float _MedianSize;
			float _OffsetZ;
			int _PolygonQuality;

			float SampleDepth(float2 in_xy)
			{
				return tex2Dlod(_DepthTex, float4(float2(in_xy.x, in_xy.y), 0, 0)).r * 0xffff * _ScaleBias;
//				Median filter is disabled for now
			};

			v2g vert(appdata v)
			{
				v2g o;
				float d = SampleDepth(v.uv.xy);

				// Convert Depth to 3D point
				// UnityObjectToClipPos is applied in geometory shader
				o.vertex.z = d;
				o.vertex.x = d * (_PPX - v.uv.x) / _FX;
				o.vertex.y = d * (_PPY - v.uv.y) / _FY;
				o.vertex.w = 1.0f;

				o.uv = v.uv;
				return o;
			}

			[maxvertexcount(4)]
			void geom(point v2g i[1], inout TriangleStream<g2f> o)
			{
				if (i[0].vertex.z < 0.01f) return;

				float2 uv[4];
				float d[4];
				float4 out_pos[4];

				// I. 4 point depth reference
				uv[0] = float2(i[0].uv.x - _WindowX, i[0].uv.y - _WindowY);	// right up
				uv[1] = float2(i[0].uv.x - _WindowX, i[0].uv.y + _WindowY);	// right down
				uv[2] = float2(i[0].uv.x + _WindowX, i[0].uv.y - _WindowY);	// left up
				uv[3] = float2(i[0].uv.x + _WindowX, i[0].uv.y + _WindowY);	// left down

				d[0] = SampleDepth(uv[0]);	// right up
				d[1] = SampleDepth(uv[1]);	// right down
				d[2] = SampleDepth(uv[2]);	// left up
				d[3] = SampleDepth(uv[3]);	// left down

				// II. Boundary scoring
				int point_score = 0;

				for (int j = 0; j < 4; j++)
				{
					if (abs(d[j] - i[0].vertex.z) < _DiffThreshold * i[0].vertex.z && d[j] > 0)
					{
						out_pos[j] = float4 (d[j] * (_PPX - uv[j].x) / _FX,
												d[j] * (_PPY - uv[j].y) / _FY,
												d[j],
												1.0);
						point_score++;
					}
					else
					{
						out_pos[j] = float4 (i[0].vertex.z * (_PPX - uv[j].x) / _FX,
												i[0].vertex.z * (_PPY - uv[j].y) / _FY,
												i[0].vertex.z,
												1.0);
					}
				}

				// III. Score evaluation
				if (point_score >= _PolygonQuality)
				{
					g2f out_v;

					// IV. Put result
					for (int j = 0; j < 4; j++)
					{
						// tex1.x is used for depth threshold flag
						out_pos[j].z += _OffsetZ;
						out_v.vertex = UnityObjectToClipPos(out_pos[j]);
						out_v.uv = uv[j];
						out_v.uv2 = float2(0, i[0].vertex.z);
						o.Append(out_v);
					}
				}
			}

			fixed4 frag(g2f i) : COLOR
			{
				// sample the texture
				fixed4 col = tex2D(_SpriteTex, i.uv);

				// depth diff evaluation between polygon center and each depth pixel
				float depth = tex2D(_DepthTex, i.uv).r * 0xffff * _ScaleBias;
				if (abs(depth - i.uv2.y) > _DiffThreshold) discard;

				fixed4 ret_col = fixed4(col.r, col.g, col.b, col.a);
				return ret_col;
			}
			ENDCG
		}

		// Pass to render object as a shadow caster
		Pass
		{
			Name "CastShadow"
			Tags { "Queue" = "Transparent"  "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex svert
			#pragma geometry sgeom
			#pragma fragment sfrag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv	  : TEXCOORD0;
			};

			struct v2fs
			{
				float4 vertex : SV_POSITION;
				float2 uv	  : TEXCOORD0;
			};

			struct g2fs
			{
				float4 vertex : SV_POSITION;
			};

			sampler2D _SpriteTex;
			sampler2D _DepthTex;
			float4 _SpriteTex_ST;
			float _FX;
			float _FY;
			float _PPX;
			float _PPY;
			float _WindowX;
			float _WindowY;
			float _DiffThreshold;
			float _ScaleBias;
			float _MedianSize;
			float _OffsetZ;
			int _ShadowPolygonQuality;

			float SampleDepth(float2 in_xy)
			{
				return tex2Dlod(_DepthTex, float4(float2(in_xy.x, in_xy.y), 0, 0)).r * 0xffff * _ScaleBias;
			};

			v2fs svert(appdata v)
			{
				v2fs o;
				float d = SampleDepth(v.uv.xy);

				// Convert Depth to 3D point
				// UnityObjectToClipPos is applied in geometory shader
				o.vertex.z = d;
				o.vertex.x = d * (_PPX - v.uv.x) / _FX;
				o.vertex.y = d * (_PPY - v.uv.y) / _FY;
				o.vertex.w = 1.0f;
				o.uv = v.uv;

				return o;
			}

			[maxvertexcount(4)]
			void sgeom(point v2fs i[1], inout TriangleStream<g2fs> o)
			{
				if (i[0].vertex.z < 0.01f) return;

				float2 uv[4];
				float d[4];
				float4 out_pos[4];

				// I. 4 point depth reference
				uv[0] = float2(i[0].uv.x - _WindowX, i[0].uv.y - _WindowY);	// right up
				uv[1] = float2(i[0].uv.x - _WindowX, i[0].uv.y + _WindowY);	// right down
				uv[2] = float2(i[0].uv.x + _WindowX, i[0].uv.y - _WindowY);	// left up
				uv[3] = float2(i[0].uv.x + _WindowX, i[0].uv.y + _WindowY);	// left down

				d[0] = SampleDepth(uv[0]);	// right up
				d[1] = SampleDepth(uv[1]);	// right down
				d[2] = SampleDepth(uv[2]);	// left up
				d[3] = SampleDepth(uv[3]);	// left down

				// II. Boundary scoring
				int point_score = 0;

				for (int j = 0; j < 4; j++)
				{
					if (abs(d[j] - i[0].vertex.z) < _DiffThreshold * i[0].vertex.z && d[j] > 0)
					{
						out_pos[j] = float4 (d[j] * (_PPX - uv[j].x) / _FX,
							d[j] * (_PPY - uv[j].y) / _FY,
							d[j],
							1.0);
						point_score++;
					}
					else
					{
						out_pos[j] = float4 (i[0].vertex.z * (_PPX - uv[j].x) / _FX,
							i[0].vertex.z * (_PPY - uv[j].y) / _FY,
							i[0].vertex.z,
							1.0);
					}
				}

				// III. Score evaluation
				if (point_score >= _ShadowPolygonQuality)
				{
					g2fs out_v;

					// IV. Put result
					for (int j = 0; j < 4; j++)
					{
						// tex1.x is used for depth threshold flag
						out_pos[j].z += _OffsetZ;
						out_v.vertex = UnityObjectToClipPos(out_pos[j]);
						o.Append(out_v);
					}
				}
			}

			float4 sfrag(v2fs i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}

	}
}
