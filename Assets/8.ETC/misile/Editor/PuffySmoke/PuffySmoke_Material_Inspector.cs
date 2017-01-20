using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PuffySmoke_Material_Inspector : MaterialEditor  {

	Dictionary<string,int> properties;
	List<string> keywords;

	Dictionary<string,MaterialProperty> material_properties;

	public override void OnInspectorGUI ()
    {
		Material targetMat = target as Material;
        keywords = targetMat.shaderKeywords.ToList();
		Shader theShader = targetMat.shader;

		properties = new Dictionary<string,int>();
		int count = ShaderUtil.GetPropertyCount(theShader);
		for(int i=0;i<count;i++){
			properties.Add(ShaderUtil.GetPropertyName(theShader,i),i);
		}


		MaterialProperty[] matProperties = MaterialEditor.GetMaterialProperties(this.targets);
		material_properties = new Dictionary<string,MaterialProperty>();
		count = matProperties.Length;
		for(int i=0;i<count;i++){
			material_properties.Add(matProperties[i].name,matProperties[i]);
		}
		MaterialProperty prop;
		GUI.changed = false;

		EditorGUIUtility.LookLikeControls(Screen.width-200,64);

		
		// shadow color
		prop = material_properties["_ShadowColor"];
		prop.colorValue = EditorGUILayout.ColorField(prop.displayName,prop.colorValue);

		// Volumetric texture
		EditorGUILayout.BeginHorizontal();
		//SetTexture("_MainTex", TextureProperty(MaterialEditor.GetMaterialProperty(this.targets,"_MainTex"), "Particle Texture"));
		prop = material_properties["_MainTex"];
		prop.textureValue = EditorGUILayout.ObjectField(prop.displayName,prop.textureValue,typeof(Texture),false) as Texture;

		EditorGUILayout.EndHorizontal();

		// Start Details
		bool use_start_details = keywords.Contains ("START_DETAILS_ON");

		EditorGUI.BeginChangeCheck();
		use_start_details = EditorGUILayout.Toggle ("Use start details", use_start_details);

		if (EditorGUI.EndChangeCheck())
        {

			targetMat.shaderKeywords = keywords.ToArray ();
			ToggleKeyword(use_start_details,"START_DETAILS_ON","START_DETAILS_OFF",targetMat);

            //EditorUtility.SetDirty (targetMat);
        }

		Vector2 tiling,offset;

		if(use_start_details){
			EditorGUILayout.Separator();
			//SetTexture("_DetailTex", TextureProperty(MaterialEditor.GetMaterialProperty(this.targets,"_DetailTex"), "Particle Texture"));

			prop = material_properties["_DetailTex"];
			prop.textureValue = EditorGUILayout.ObjectField(prop.displayName,prop.textureValue,typeof(Texture),false) as Texture;

			tiling = EditorGUILayout.Vector2Field("Tiling",new Vector2(prop.textureScaleAndOffset.y,prop.textureScaleAndOffset.x));
			offset = EditorGUILayout.Vector2Field("Offset",new Vector2(prop.textureScaleAndOffset.w,prop.textureScaleAndOffset.z));
			
			prop.textureScaleAndOffset = new Vector4(tiling.y,tiling.x,offset.y,offset.x);

			FloatSlider("_Sharpness","Sharpness",targetMat);
			FloatSlider("_DetailsHSpeed","Horizontal Speed",targetMat);
			FloatSlider("_DetailsVSpeed","Vertical Speed",targetMat);
			EditorGUILayout.Separator();
		}

		// End Details
		bool use_end_details = keywords.Contains ("END_DETAILS_ON");

		EditorGUI.BeginChangeCheck();
		use_end_details = EditorGUILayout.Toggle ("Use end details", use_end_details);

		if (EditorGUI.EndChangeCheck())
        {
			targetMat.shaderKeywords = keywords.ToArray ();
			ToggleKeyword(use_end_details,"END_DETAILS_ON","END_DETAILS_OFF",targetMat);

            //EditorUtility.SetDirty (targetMat);
        }

		if(use_end_details){
			EditorGUILayout.Separator();
			//SetTexture("_DetailTex2", TextureProperty(MaterialEditor.GetMaterialProperty(this.targets,"_DetailTex2"), "Particle Texture"));

			prop = material_properties["_DetailTex2"];
			prop.textureValue = EditorGUILayout.ObjectField(prop.displayName,prop.textureValue,typeof(Texture),false) as Texture;
			
			tiling = EditorGUILayout.Vector2Field("Tiling",new Vector2(prop.textureScaleAndOffset.y,prop.textureScaleAndOffset.x));
			offset = EditorGUILayout.Vector2Field("Offset",new Vector2(prop.textureScaleAndOffset.w,prop.textureScaleAndOffset.z));
			
			prop.textureScaleAndOffset = new Vector4(tiling.y,tiling.x,offset.y,offset.x);

			FloatSlider("_SharpnessSmall","Sharpness",targetMat);
			FloatSlider("_DetailsHSpeed2","Horizontal Speed",targetMat);
			FloatSlider("_DetailsVSpeed2","Vertical Speed",targetMat);
			EditorGUILayout.Separator();
		}

		if(use_end_details || use_start_details){
			EditorGUILayout.Separator();
			FloatSlider("_Density","Details Density",targetMat);
			FloatSlider("_EmissiveDensity","Details Emissive density",targetMat);
		}

		EditorGUILayout.Separator();
		// Cloud mode


		// End Details
		bool cloud_mode = keywords.Contains ("CLOUDS_ON");
		
		EditorGUI.BeginChangeCheck();
		cloud_mode = EditorGUILayout.Toggle ("Clouds mode", cloud_mode);
		
		if (EditorGUI.EndChangeCheck())
		{
			targetMat.shaderKeywords = keywords.ToArray ();
			ToggleKeyword(cloud_mode,"CLOUDS_ON","CLOUDS_OFF",targetMat);
			
			//EditorUtility.SetDirty (targetMat);
		}
		/*
		prop = material_properties["_CloudMode"];
		bool cloud_mode = EditorGUILayout.Toggle ("Cloud mode", prop.floatValue>0);
		prop.floatValue = cloud_mode?1:0;
		*/

		if(cloud_mode){
			FloatSlider("_DetailsFading","Details cross fading",targetMat);
			FloatSlider("_DetailsFadingLimit","Cross fading limit",targetMat);
			EditorGUILayout.Separator();
		}else{
			FloatSlider("_FadeIn","Fade in",targetMat);
		}
		FloatSlider("_Opacity","Opacity",targetMat);

		// light parameters
		EditorGUILayout.Separator();
		FloatSlider("_Scattering","Light scattering",targetMat);

		bool use_extra_lights = keywords.Contains ("EXTRA_LIGHTS_ON");

		EditorGUI.BeginChangeCheck();
		use_extra_lights = EditorGUILayout.Toggle ("Use extra lights", use_extra_lights);

		if (EditorGUI.EndChangeCheck())
        {
			targetMat.shaderKeywords = keywords.ToArray ();
			ToggleKeyword(use_extra_lights,"EXTRA_LIGHTS_ON","EXTRA_LIGHTS_OFF",targetMat);

            //EditorUtility.SetDirty (targetMat);
        }

		if(use_extra_lights) FloatSlider("_ExtraLightsIntensity","Extra Lights Intensity",targetMat);

		if(GUI.changed){
			EditorUtility.SetDirty (targetMat);
		}
	}

	private void FloatSlider(string propertyname,string label, Material material){

		EditorGUI.BeginChangeCheck();
		int index = properties[propertyname];

		if(index >= 0){
			float val = material.GetFloat(propertyname);
			float min = ShaderUtil.GetRangeLimits(material.shader,index,1);
			float max = ShaderUtil.GetRangeLimits(material.shader,index,2);

			val = (float)EditorGUILayout.Slider(label,val,min,max,null);

			if (EditorGUI.EndChangeCheck()){
				material.SetFloat(propertyname,val);
			}
		}else{
			Debug.LogError("Property "+propertyname+" not found in shader "+material.shader);
		}
	}

	private void ToggleKeyword(bool state, string on, string off, Material material){
		if(state && keywords.Contains(off)) keywords.Remove(off);
		if(!state && keywords.Contains(on)) keywords.Remove(on);

		if(state){
			material.EnableKeyword(on);
			material.DisableKeyword(off);
		}else{
			material.DisableKeyword(on);
			material.EnableKeyword(off);
		}
	}

}
