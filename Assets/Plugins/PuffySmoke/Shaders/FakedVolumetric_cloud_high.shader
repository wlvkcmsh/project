Shader "Puffy_Smoke/Puffy Cloud High" {

Properties {

	_ShadowColor ("Shadow Color", Color) = (0.0,0.5,0.5,1)
	
	_MainTex ("Particle Texture", 2D) = "white" {}
	
	_DetailTex ("Big Details", 2D) = "white" {}
	_Sharpness ("Big Details Sharpness", Range(0.0,10)) = 0
	_DetailsSpeed ("Big Details Speed", Range(-0.1,0.1)) = 0.01
	
	_DetailTex2 ("Small Details", 2D) = "white" {}
	_SharpnessSmall ("Small Details Sharpness", Range(0.0,10)) = 0
	_DetailsSpeedSmall ("Small Details Speed", Range(-0.1,0.1)) = 0.01
	
	_DetailsFading ("Details cross fading", Range(1,4)) = 2
	
	_Opacity ("Opacity", Range(0.0,1.0)) = 0.5
	
	_Density ("Density", Range(0.0,1.0)) = 0
	_EmissiveDensity ("Emissive Density", Range(0.0,1.0)) = 0.1
	
	_ExtraLightsIntensity ("Extra Lights Intensity", Range(0.0,1.0)) = 1.0
}


Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater 0.01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "texcoord", texcoord0
		Bind "texcoord1", texcoord1
	}

	// ---- Fragment program cards
	SubShader {
	
		Pass {
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
	 		#pragma target 2.0
	 		#pragma glsl

	 		// very important on mobile opengl es 2.0
			#pragma glsl_no_auto_normalization 
			
			//#pragma exclude_renderers gles
			
	        #pragma vertex vert  
	        #pragma fragment frag 
	        #pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			#pragma multi_compile_fwdbase
			#include "AutoLight.cginc"
				
			sampler2D _MainTex;
			sampler2D _DetailTex;
			sampler2D _DetailTex2;
			
			sampler2D _CameraDepthTexture;
			
	      	uniform fixed4 _ShadowColor;
	      	uniform fixed4 _AmbientColor;
	      	uniform fixed4 _LightColor;
	      	uniform fixed4 _MainTex_ST;
			uniform fixed4 _DetailTex_ST;
			uniform fixed4 _DetailTex2_ST;
			
			uniform fixed _Density;
        	uniform fixed _DetailsSpeed;
        	uniform fixed _DetailsSpeedSmall;
        	uniform fixed _Sharpness;
        	uniform fixed _SharpnessSmall;
        	uniform fixed _Scattering;
        	uniform fixed _Opacity;
        	uniform fixed _details;
        	uniform fixed _LightIntensity;
        	uniform fixed _InvFade;
			uniform fixed _EmissiveDensity;
			uniform fixed _ExtraLightsIntensity;
			
			uniform fixed _DetailsFading;
			
			//uniform fixed3 _BillboardsNormal;
			
			struct vertexInput  {
				fixed4 vertex : POSITION;
				fixed4 color : COLOR;
				fixed3 normal : NORMAL;
				fixed2 texcoord : TEXCOORD0;
				fixed2 texcoord1 : TEXCOORD1;
			};

			struct vertexOutput {
				fixed4 vertex : POSITION;
				fixed4 color : COLOR;
				fixed2 texcoord : TEXCOORD0;
				fixed2 texcoord1 : TEXCOORD1;
				fixed2 uv2 : TEXCOORD7;
				fixed3 CombinedData : TEXCOORD2;
				fixed4 ShadowColor : TEXCOORD3;
				fixed4 AmbientColor : TEXCOORD4;
				fixed2 CombinedData2 : TEXCOORD5;
				fixed4 vertexLighting : TEXCOORD6;
			};

			vertexOutput vert (vertexInput input)
			{
				vertexOutput output; 
				output.vertex = mul(UNITY_MATRIX_MVP, input.vertex);
				
				fixed _old = input.normal.x;
				fixed _emissive = input.normal.y;

				fixed4 _color = input.color;
				
				output.texcoord = TRANSFORM_TEX(input.texcoord,_MainTex);
				output.texcoord1 = TRANSFORM_TEX(input.texcoord1,_DetailTex) + (_old * _DetailsSpeed);
				output.uv2 = TRANSFORM_TEX(input.texcoord1,_DetailTex2) + (_old * _DetailsSpeedSmall);
				
				output.vertexLighting = float4(0.0, 0.0, 0.0, 0.0);
					
				// compute extra vertex lights
				#ifdef VERTEXLIGHT_ON
				if(_ExtraLightsIntensity > 0){
					fixed4x4 modelMatrix = _Object2World;
					fixed4x4 modelMatrixInverse = _World2Object; 
					fixed3 posWorld = mul(modelMatrix, input.vertex).xyz;
					
					for (int index = 0; index < 4; index++)
		            {    
						fixed4 lightPosition = float4(unity_4LightPosX0[index], 
							unity_4LightPosY0[index], 
							unity_4LightPosZ0[index], 1.0);
		 
						fixed3 vertexToLightSource = lightPosition.xyz - posWorld;											
						fixed squaredDistance = dot(vertexToLightSource, vertexToLightSource);
																
						fixed attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * squaredDistance);
						attenuation *= attenuation;
		           		attenuation = max(0,attenuation - _emissive);   

						output.vertexLighting.xyz += attenuation * 2 * unity_LightColor[index].xyz;
		            }
		            output.vertexLighting.xyz *= _ExtraLightsIntensity;
	            }
	            #endif
				
				output.vertexLighting.w = input.normal.z;
				
				// mix smoke color and light color
				_color.x *= (max(0, _LightColor.r - _emissive) * _LightIntensity) + _emissive;
				_color.y *= (max(0, _LightColor.g - _emissive) * _LightIntensity) + _emissive;
				_color.z *= (max(0, _LightColor.b - _emissive) * _LightIntensity) + _emissive;	

				output.color = _color;
				
				// scattering effect
				output.ShadowColor = _ShadowColor; //lerp(_ShadowColor, output.color + _AmbientColor , _old * _Scattering);
				
				// emissive particles are less transparent
				fixed AlphaTemp = input.color.a * (1 + _emissive*_emissive*2);
				
				output.CombinedData = fixed3(_emissive, _Opacity*AlphaTemp, _old);
				output.AmbientColor = (_AmbientColor * (1 - _emissive));
				output.CombinedData2 = fixed2(_old * _Sharpness , _emissive * _EmissiveDensity);
				
				return output;
			}

			fixed4 frag (vertexOutput i) : COLOR
			{
				// get main texture color and alpha
				fixed4 _sampled = tex2D(_MainTex, i.texcoord);
				
				// mix smoke color and shadow color
				fixed4 _finalcolor = lerp(i.ShadowColor , i.color , clamp(_sampled.r * _LightIntensity, 0, 1) ) + i.AmbientColor;
				
				// animated noise - 1 details texture
				//fixed _details = lerp(tex2D(_DetailTex, i.texcoord1 ).r , tex2D(_DetailTex2, i.uv2 ).r,clamp(_DetailsFading - i.vertexLighting.w,0,1) );
				fixed _details = tex2D(_DetailTex, i.texcoord1 ).r;
				_details = clamp(lerp((0.5 + _details) * 0.5, _details , _Sharpness) + _Density,0,1);
				
				fixed _detailsSmall = tex2D(_DetailTex2, i.uv2 ).r;
				_detailsSmall = clamp(lerp((0.5 + _detailsSmall) * 0.5, _detailsSmall , _SharpnessSmall) + _Density,0,1);
				
				_details = lerp(_details , _detailsSmall ,clamp(_DetailsFading - i.vertexLighting.w,0,1));
					
				// main alpha
				_finalcolor.a = i.CombinedData.y * _sampled.a * (_details + i.CombinedData2.y);
				_finalcolor.rgb += i.vertexLighting.xyz;
								
				return _finalcolor;
				
			}
			
			
			
			ENDCG 
		}	
		
		
	} 	
}

}
