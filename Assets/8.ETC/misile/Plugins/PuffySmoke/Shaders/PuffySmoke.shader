// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Puffy_Smoke/Smoke" {

	Properties {
		
		_CloudMode("Cloud mode",Range(0,1)) = 0
		
		_ShadowColor ("Shadow Color", Color) = (0.0,0.5,0.5,1)
		_MainTex ("Particle Texture", 2D) = "white" {}
		
		_DetailTex ("Start Details", 2D) = "white" {}
		_Sharpness ("Start Details Sharpness", Range(0.0,10.0)) = 0
		_DetailsHSpeed ("Start Details Horizontal Speed", Range(-1,1)) = 0.0
		_DetailsVSpeed ("Start Details Vertical Speed", Range(-1,1)) = 0.2
		
		_DetailTex2 ("End Details", 2D) = "white" {}
		_SharpnessSmall ("End Details Sharpness", Range(0.0,10.0)) = 0
		_DetailsHSpeed2 ("End Details Horizontal Speed", Range(-1,1)) = 0.0
		_DetailsVSpeed2 ("End Details Vertical Speed", Range(-1,1)) = 0.2
		
		_DetailsFading ("Details cross fading", Range(0.0,1.0)) = 0.5
		_DetailsFadingLimit ("Cross fading limit", Range(1.0,50.0)) = 2
		
		_FadeIn ("Fade in", Range(0.0,1.0)) = 0.1
		_Opacity ("Opacity", Range(0.0,1.0)) = 0.5

		_Density ("Density", Range(0.0,1.0)) = 0.0
		_EmissiveDensity ("Emissive Density", Range(0.0,1.0)) = 0.1

		_Scattering ("Light Scattering", Range(0.0,1.0)) = 1
		_ExtraLightsIntensity ("Extra Lights Intensity", Range(0.0,1.0)) = 1.0
		
		_ElapsedTime ("ElapsedTime",Float) = 1.0
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off
		
		// ---- Fragment program cards
		SubShader {
			Pass {
			Tags { "LightMode" = "ForwardBase" } 
			 
				CGPROGRAM
		 		#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				
		 		#pragma target 2.0
		 		#pragma glsl

				#pragma noforwardadd
				#pragma multi_compile_fwdbase
				#pragma multi_compile START_DETAILS_ON START_DETAILS_OFF
				#pragma multi_compile END_DETAILS_ON END_DETAILS_OFF
				#pragma multi_compile EXTRA_LIGHTS_ON EXTRA_LIGHTS_OFF
				#pragma multi_compile CLOUDS_ON CLOUDS_OFF

		 		// very important on mobile opengl es 2.0
				#pragma glsl_no_auto_normalization 
				//#pragma exclude_renderers gles
				
		        #pragma vertex vert  
		        #pragma fragment frag 
		        #pragma fragmentoption ARB_precision_hint_fastest
				
				
				
				
				sampler2D _MainTex;
		     	sampler2D _DetailTex;
		     	sampler2D _DetailTex2;
		     	
		     	uniform fixed4 _ShadowColor;
		     	uniform fixed4 _AmbientColor;
		     	
		     	uniform fixed4 _LightColor; // Main Directional light color
		     	
		     	uniform fixed4 _MainTex_ST;
		     	uniform fixed4 _DetailTex_ST;
		     	uniform fixed4 _DetailTex2_ST;
		     	
				uniform fixed _FadeIn;
		     	uniform fixed _Density;
		     	
        		uniform fixed _DetailsHSpeed;
        		uniform fixed _DetailsVSpeed;
        		uniform fixed _Sharpness;
        		
        		uniform fixed _DetailsHSpeed2;
        		uniform fixed _DetailsVSpeed2;
        		uniform fixed _SharpnessSmall;
        		
        		uniform fixed _DetailsFading;
        		uniform fixed _DetailsFadingLimit;
        		
		        uniform fixed _Opacity;
		        
		        uniform fixed _LightIntensity; // Main Directional light intensity
		        
		       	uniform fixed _Scattering;
		       	uniform fixed _EmissiveDensity;
		       	uniform fixed _ExtraLightsIntensity;
			
				uniform fixed _ElapsedTime;
						       			       			       			       			       			       			       			       			       			       			       	
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
					fixed2 uv2 : TEXCOORD2;
					fixed4 CombinedData : TEXCOORD3;
					fixed4 ShadowColor : TEXCOORD4;
					fixed4 AmbientColor : TEXCOORD5;
					fixed4 vertexLighting : TEXCOORD6;
				};
		
				vertexOutput vert (vertexInput input)
				{
					vertexOutput output; 
					output.vertex = mul(UNITY_MATRIX_MVP, input.vertex);
					
					fixed ageRatio = input.normal.x;
					fixed emissive = input.normal.y;
					fixed particleSize = max(0.0001,input.normal.z);
					fixed4 particleColor = input.color;
					
					output.texcoord = TRANSFORM_TEX(input.texcoord,_MainTex);
					
					//output.texcoord1 = TRANSFORM_TEX(input.texcoord1,_DetailTex) + float2(-_DetailsVSpeed,_DetailsHSpeed) * _ElapsedTime / particleSize;
					//output.texcoord1 = input.texcoord1 * _DetailTex_ST.xy + (_DetailTex_ST.zw + float2(_DetailsVSpeed , -_DetailsHSpeed) * _ElapsedTime ) / particleSize;

					output.texcoord1 = (input.texcoord1 + (float2(_DetailsVSpeed , -_DetailsHSpeed) * _ElapsedTime )) * _DetailTex_ST.xy + (_DetailTex_ST.zw / particleSize);
																																																																																																									
					//output.uv2 = TRANSFORM_TEX(input.texcoord1,_DetailTex2) + float2(-_DetailsVSpeed2,_DetailsHSpeed2) * _ElapsedTime / particleSize;
					//output.uv2 = input.texcoord1 * _DetailTex2_ST.xy + (_DetailTex2_ST.zw + float2(_DetailsVSpeed2 , -_DetailsHSpeed2 ) * _ElapsedTime ) / particleSize;

					output.uv2 = (input.texcoord1 + (float2(_DetailsVSpeed2 , -_DetailsHSpeed2) * _ElapsedTime )) * _DetailTex2_ST.xy + (_DetailTex2_ST.zw / particleSize);
																														
					// compute extra vertex lights
					output.vertexLighting = float4(0.0, 0.0, 0.0,0.0);
					
					#ifdef EXTRA_LIGHTS_ON
					
					//LightMode = Vertex pass: 4 lights are set up, always sorted from brightest to dimmest,
					//            in unity_LightColor[n], unity_LightPosition[n], unity_LightAtten[n].
					//            So fetching first index always gives you the brightest light.
					
					//LightMode = ForwardBase pass: _LightColor0 is the color of the main directional light
					//            that this pass is supposed to apply.
					
					//LightMode = ForwardAdd pass: the same; it's the color of the per-pixel light
					//            that this pass is applying.


					if(_ExtraLightsIntensity > 0){
						fixed4x4 modelMatrix = unity_ObjectToWorld;
						fixed4x4 modelMatrixInverse = unity_WorldToObject; 
						fixed3 posWorld = mul(modelMatrix, input.vertex).xyz;
						
						for (int index = 0; index < 4; index++){
							fixed3 lightPosition = fixed3(unity_4LightPosX0[index],unity_4LightPosY0[index],unity_4LightPosZ0[index]);
              				
							fixed3 vertexToLightSource = lightPosition - posWorld;
							fixed squaredDistance = dot(vertexToLightSource, vertexToLightSource);
							fixed attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * squaredDistance);
							
							attenuation *= attenuation;
			           		attenuation = max(0,attenuation - emissive);
			           		   
							output.vertexLighting.xyz += attenuation * 2 * unity_LightColor[index].xyz;
			            }
			            			            
			            output.vertexLighting.xyz *= _ExtraLightsIntensity;
		            }
		            #endif
		           	
		           	// particle size
		           	output.vertexLighting.w = particleSize;
		           				
					// mix smoke color and light color
					particleColor.x *= (max(0, _LightColor.r - emissive) * _LightIntensity) + emissive;
					particleColor.y *= (max(0, _LightColor.g - emissive) * _LightIntensity) + emissive;
					particleColor.z *= (max(0, _LightColor.b - emissive) * _LightIntensity) + emissive;	

					output.color = particleColor;
					
					// emissive particles are less transparent
					fixed AlphaTemp = input.color.a * (1 + emissive*emissive*2);
					
					#ifdef CLOUDS_ON
						// particles are immortal, do not fade with age
						AlphaTemp *= _Opacity;
						output.CombinedData = fixed4(emissive, AlphaTemp, ageRatio, emissive * _EmissiveDensity);
						
						// light scattering effect on faded particles only
						output.ShadowColor = lerp(_ShadowColor, output.color , output.CombinedData.y * _Scattering);
					#else
						// use particle age to fade out particles
						AlphaTemp *= _Opacity * (1 - ageRatio) * (_FadeIn > 0 ? clamp(ageRatio / _FadeIn, 0, 1):1);
						output.CombinedData = fixed4(emissive, AlphaTemp , ageRatio, emissive * _EmissiveDensity);
						
						// light scattering effect on older particles only
						output.ShadowColor = lerp(_ShadowColor, output.color , ageRatio * _Scattering);
					#endif
					
					output.AmbientColor = (_AmbientColor * (1 - emissive));
										
					return output;
				}

				fixed4 frag (vertexOutput o) : COLOR
				{
				
					// get main texture color and alpha
					fixed4 _sampled = tex2D(_MainTex, o.texcoord);

					// mix light color and shadow color
					fixed4 _finalcolor = lerp(o.ShadowColor, o.color , clamp(_sampled.r * _LightIntensity + o.CombinedData.x, 0, 1) ) + o.AmbientColor ;
					
					// animated noise - mix 2 details textures
					fixed _details = 1.0;
					fixed _detailsBig = 1.0;
					fixed _detailsSmall = 1.0;

					#ifdef START_DETAILS_ON
					_detailsBig = tex2D(_DetailTex, o.texcoord1 ).r;
					_detailsBig = clamp(lerp((0.5 + _detailsBig) * 0.5, _detailsBig , _Sharpness) + _Density,0,1);
					_detailsSmall = _detailsBig;
					#endif

					#ifdef END_DETAILS_ON
					_detailsSmall = tex2D(_DetailTex2, o.uv2 ).r;
					_detailsSmall = clamp(lerp((0.5 + _detailsSmall) * 0.5, _detailsSmall , _SharpnessSmall) + _Density,0,1);
					#endif
					
					#ifdef CLOUDS_ON
						_details = lerp(_detailsBig , _detailsSmall , clamp((_DetailsFading*_DetailsFadingLimit) / o.vertexLighting.w,0,1));
					#else
						_details = lerp(_detailsBig , _detailsSmall , o.CombinedData.z);
					#endif																																																																																													
					
					// main alpha
					_finalcolor.a = _sampled.a * (_details + o.CombinedData.w) * o.CombinedData.y;

					#ifdef EXTRA_LIGHTS_ON
					// extra lights
					_finalcolor.rgb += o.vertexLighting.xyz;
					#endif

					return _finalcolor;
				
				}
				
				ENDCG 
			}	
		}
		
	}
	FallBack "Diffuse"
    CustomEditor "PuffySmoke_Material_Inspector"
}