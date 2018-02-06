Shader "Unlit/GrassRenderer"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Lightmap("Lightmap Texture", 2D) = "white" {}
		[HideInInspector]_GrassMinWidth("Grass Min Width", float) = 1		//最小宽度
		[HideInInspector]_GrassMaxWidth("Grass Max Width", float) = 1		//最大宽度
		[HideInInspector]_GrassMinHeight("Grass Min Height", float) = 1		//最小高度
		[HideInInspector]_GrassMaxHeight("Grass Max Height", float) = 1		//最大高度
		_RotSpeed("Rot Speed",float) = 3					//水平转动频率
		_MaxRotAngle("Max Rot Angle",float) = 0.6			//最大倾倒角度
		_WindRotAngle("Wind Rot Angle",float) = 0.02		//风的倾倒角度
		_WaveStrength("Wave Strength",float) = 1			//草浪幅度
		_WaveSpeed("Wave Speed",float) = 1					//草浪频率
		_WaveLength("Wave Length",float) = 3				//草浪波长
		_AoColor("AO Color",Color) = (0.5,0.5,0.5,1)		//草根的颜色系数
		[HideInInspector]_ViewDistance("View Distance",float) = 50			//视距
		[HideInInspector]_GrassColor("Grass Color",Color) = (1,1,1,1)		//草的颜色,从每种草的DryColor和HealthColor平均而来
		[HideInInspector]_WavingTint("Fade Color", Color) = (.7,.6,.5, 0)	//草的颜色，从地形数据读取
	}
	SubShader
	{
		Tags {
			"Queue" = "Geometry+200"
			"IgnoreProjector"="True"
			"RenderType"="GrassBillboard"
			"LightMode" = "ForwardBase"
		}
		LOD 100

		Pass
		{
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 tangent:TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float2 uv2 : TEXCOORD2;
				float4 vertex : SV_POSITION;
				half4 color:COLOR;
				half3 normal :NORMAL;
			};

			sampler2D _MainTex;
			sampler2D _GrassLightmap;
			float4 _MainTex_ST;
			float _GrassMinWidth;
			float _GrassMaxWidth;
			float _GrassMinHeight;
			float _GrassMaxHeight;
			float _RotSpeed;
			float _MaxRotAngle;
			float _WindRotAngle;
			float4 _WavingTint;
			float _ViewDistance;
			float4 _AoColor;
			float4 _GrassColor;
			float _GrassTime;
			float _WaveStrength;
			float _WaveSpeed;
			float _WaveLength;
			float4 _TerrainOffsetSize;

			float _billboardGrass_AmbientIntensity;
			float _billboardGrass_ShadowIntensity;
			float _billboardGrass_SunIntensity;

			//得到当前摇摆幅度,要与c#代码保持一致
			float Simulate(float4 tangent, float time)
			{
				if (time < tangent.w) {//上升期
					float t = (time - tangent.z) / (tangent.w - tangent.z);
					return lerp(tangent.x, tangent.y, pow(t,0.5));
				}
				else {
					float duration = 4;//下降时长
					float t = (time - tangent.w) / (duration);
					return lerp(tangent.y, 0, t);
				}
			}

			void GrassWave(in half4 worldPos, out half4 vertex, out half4 color, out half3 normal, half4 uv, float intensity)
			{
				half3 viewDir = UNITY_MATRIX_IT_MV[2].xyz;
				half3 normalDir = viewDir;									//垂直草叶方向

				//草叶宽高
				half width = lerp(_GrassMinWidth, _GrassMaxWidth, uv.w);
				float height = lerp(_GrassMinHeight, _GrassMaxHeight, uv.w);

				//水平转动相位
				half leafAngle = uv.z;
				float rotSpeed = _RotSpeed * lerp(1,0.2,uv.w);
				half yaw = _GrassTime * (1 + uv.z) * rotSpeed;//水平旋转相位，即绕Y周转动的角度 

				//倾倒程度的量
				//  |   /
				//  |  /
				//  |p/
				//  |/
				//float intensity = Simulate(tangent, _GrassTime);
				float pitch = sqrt(uv.y) * smoothstep(0, 1, intensity) * _MaxRotAngle + _WindRotAngle;//倾倒的角度

				//大草浪
				float bigWave = 0;//yaw * (1 - uv.w) * 2;//(worldPos.x + sin(worldPos.y)) / _WaveLength + _GrassTime * _WaveSpeed;

				float4 sinR, cosR;//x:叶片创建时角度，y:水平转动的相位，z:摇摆的幅度
				sincos(half4(uv.z, yaw, pitch, bigWave), sinR, cosR);

				//草叶方片
				half3 rightDir = normalize(cross(viewDir, half3(0, 1, 0))); //沿草叶方向
				rightDir = normalDir * sinR.x + rightDir * cosR.x;				//随机转一下角度，防止太整齐
				half3 upDir = normalize(cross(rightDir, normalDir));			//草叶向上的方向，这里采取垂直摄像机的方式
				worldPos.xyz += (uv.x - 0.5f) * rightDir * width;	//沿草叶方向定位坐标，但是先不赋值高度，先都放在草根处，在处理摆动倾角时加上高度

				//处理摆动倾角
				//				区分顶端   ------ 水平旋转------      --------水平旋转-----    ---------倾倒角度--------
				worldPos.xyz += uv.y * (normalDir * sinR.z * sinR.y + rightDir * sinR.z * cosR.y + upDir * cosR.z * height);//绕草根朝dir方向旋转angle角度

				//草浪 
				//float2 _waveMove = float2(1, 0.3) * 3 * _WaveStrength;
				//worldPos.xz += uv.y * sinR.w * _waveMove * uv.w ;


				vertex = mul(UNITY_MATRIX_VP, worldPos);

				//光照，需要贴近原生草，参考：TerrainWaveGrass(...)函数
				//color.rgb = lerp(half3(0.5,0.5,0.5),half3(1.2,1.2,1.2),_WavingTint.rgb);//地形设置的总的颜色
				color.rgb = _GrassColor.rgb * 1.2;//每种草设置的颜色
				color.rgb *= (0.5 + (sinR.y * sinR.z * 0.5)) + unity_AmbientSky;//直射光和环境光
				color.rgb *= lerp(_AoColor.rgb,half3(1,1,1),uv.y * uv.y);//草根处的AO

				//可视距离
				float distance = length(_WorldSpaceCameraPos - worldPos.xyz);
				color.a = saturate((_ViewDistance - distance) / 1.0f);

				normal = normalDir;
			}

			
			v2f vert (appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld,v.vertex);

				float intensity = Simulate(v.tangent, _GrassTime);
				GrassWave(worldPos, o.vertex, o.color, o.normal, v.uv , intensity);
			
				o.color.a += intensity * 0.001;//有个奇怪的bug,这里需要用一下intensity，否则，结果是错的

				o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);
				o.uv2 = (worldPos.xz - _TerrainOffsetSize.xy)/ _TerrainOffsetSize.zw;
			
				UNITY_TRANSFER_FOG(o,o.vertex); 
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{			
				fixed4 col = tex2D(_MainTex, i.uv);

				//fixed4 lmtex = tex2D(_GrassLightmap, i.uv2);
				//float3 c = lmtex.x*lmtex.x*lmtex.z * 5;
				//float3 lmC = _billboardGrass_SunIntensity* lmtex.a + c*_billboardGrass_AmbientIntensity + _billboardGrass_ShadowIntensity;// max(0.5, lmtex.xyz + lmtex.a);
				col.xyz *= i.color.xyz * 1.2;// *lmC;

				half a = col.a * i.color.a;
				clip(a - 0.5);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
