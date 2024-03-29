// Toony Colors Pro+Mobile 2
// (c) 2014-2016 Jean Moreno

//Enable this to display the default Inspector (in case the custom Inspector is broken)
//#define SHOW_DEFAULT_INSPECTOR

//Enable this to show Debug infos
//#define DEBUG_INFO

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// Custom Unified Inspector that will select the correct shaders depending on the settings defined.

public class TCP2_MaterialInspector : ShaderGUI
{
	//Constants
	private const string BASE_SHADER_PATH = "Toony Colors Pro 2/";
	private const string VARIANT_SHADER_PATH = "Hidden/Toony Colors Pro 2/Variants/";
	private const string BASE_SHADER_NAME = "Desktop";
	private const string BASE_SHADER_NAME_MOB = "Mobile";
	
	//Properties
	private Material targetMaterial { get { return (mMaterialEditor == null) ? null : mMaterialEditor.target as Material; } }
	private MaterialEditor mMaterialEditor;
	private List<string> mShaderFeatures;
	private bool isGeneratedShader;
	private bool isMobileShader;
	private bool mJustChangedShader;
	private string mVariantError;

	//Shader Variants 
	private List<string> ShaderVariants = new List<string>()
	{
		{ "Specular" },
		{ "Reflection" },
		{ "Matcap" },
		{ "Rim" },
		{ "RimOutline" },
		{ "Outline" },
		{ "OutlineBlending" },
		{ "Sketch" },
		{ "Alpha" },
		{ "Cutout" },
	};
	private List<bool> ShaderVariantsEnabled = new List<bool>()
	{
		{ false },
		{ false },
		{ false },
		{ false },
		{ false },
		{ false },
		{ false },
		{ false },
		{ false },
		{ false },
	};

	//--------------------------------------------------------------------------------------------------

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
	{
		base.AssignNewShaderToMaterial (material, oldShader, newShader);

		//Detect if User Shader (from Shader Generator)
		isGeneratedShader = false;
		mShaderFeatures = null;
		ShaderImporter shaderImporter = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(newShader)) as ShaderImporter;
		if(shaderImporter != null)
		{
			TCP2_ShaderGeneratorUtils.ParseUserData(shaderImporter, out mShaderFeatures);
			if(mShaderFeatures.Count > 0 && mShaderFeatures[0] == "USER")
			{
				isGeneratedShader = true;
			}
		}
	}

	private void UpdateFeaturesFromShader()
	{
		if(targetMaterial != null && targetMaterial.shader != null)
		{
			string name = targetMaterial.shader.name;
			if(name.Contains("Mobile"))
				isMobileShader = true;
			else
				isMobileShader = false;
			List<string> nameFeatures = new List<string>(name.Split(' '));
			for(int i = 0; i < ShaderVariants.Count; i++)
			{
				ShaderVariantsEnabled[i] = nameFeatures.Contains(ShaderVariants[i]);
			}
			//Get flags for compiled shader to hide certain parts of the UI
			ShaderImporter shaderImporter = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(targetMaterial.shader)) as ShaderImporter;
			if(shaderImporter != null)
			{
//				mShaderFeatures = new List<string>(shaderImporter.userData.Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries));
				TCP2_ShaderGeneratorUtils.ParseUserData(shaderImporter, out mShaderFeatures);
				if(mShaderFeatures.Count > 0 && mShaderFeatures[0] == "USER")
				{
					isGeneratedShader = true;
				}
			}
		}
	}

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		mMaterialEditor = materialEditor;

#if SHOW_DEFAULT_INSPECTOR
		base.OnGUI();
		return;
#else

		//Wait one frame to avoid GUI errors
		if(mJustChangedShader && Event.current.type != EventType.Layout)
		{
			mJustChangedShader = false;
			mVariantError = null;
			Event.current.Use();		//Avoid layout mismatch error
			SceneView.RepaintAll();
		}

		UpdateFeaturesFromShader();

		//Get material keywords
		List<string> keywordsList = new List<string>(targetMaterial.shaderKeywords);
		bool updateKeywords = false;
		bool updateVariant = false;
		
		//Header
		EditorGUILayout.BeginHorizontal();
		TCP2_GUI.HeaderBig("TOONY COLORS PRO 2 - INSPECTOR");
		if(isGeneratedShader && TCP2_GUI.Button(TCP2_GUI.CogIcon, "O", "Open in Shader Generator"))
		{
			if(targetMaterial.shader != null)
			{
				TCP2_ShaderGenerator.OpenWithShader(targetMaterial.shader);
			}
		}
		TCP2_GUI.HelpButton("Unified Shader");
		EditorGUILayout.EndHorizontal();
		TCP2_GUI.Separator();

		if(!string.IsNullOrEmpty(mVariantError))
		{
			EditorGUILayout.HelpBox(mVariantError, MessageType.Error);

			EditorGUILayout.HelpBox("Some of the shaders are packed to avoid super long loading times when you import Toony Colors Pro 2 into Unity.\n\n"+
			                        "You can unpack them by category in the menu:\n\"Tools > Toony Colors Pro 2 > Unpack Shaders > ...\"",
			                        MessageType.Info);
		}

		//Iterate Shader properties
		materialEditor.serializedObject.Update();
		SerializedProperty mShader = materialEditor.serializedObject.FindProperty("m_Shader");
		if(materialEditor.isVisible && !mShader.hasMultipleDifferentValues && mShader.objectReferenceValue != null)
		{
			EditorGUIUtility.labelWidth = Screen.width - 120f;
			EditorGUIUtility.fieldWidth = 64f;

			EditorGUI.BeginChangeCheck();

			MaterialProperty[] props = properties;

			//UNFILTERED PARAMETERS ==============================================================

			TCP2_GUI.HeaderAndHelp("BASE", "Base Properties");
			if(ShowFilteredProperties(null, props))
			{
				if(!isGeneratedShader)
					TCP2_Utils.ShaderKeywordToggle("TCP2_DISABLE_WRAPPED_LIGHT", "Disable Wrapped Lighting", "Disable wrapped lighting, reducing intensity received from lights", keywordsList, ref updateKeywords, "Disable Wrapped Lighting");

				TCP2_GUI.Separator();
			}

			//FILTERED PARAMETERS ================================================================

			//RAMP TYPE --------------------------------------------------------------------------

			if(CategoryFilter("TEXTURE_RAMP"))
			{
				if(isGeneratedShader)
				{
					ShowFilteredProperties("#RAMPT#", props);
				}
				else
				{
					if( TCP2_Utils.ShaderKeywordToggle("TCP2_RAMPTEXT", "Texture Toon Ramp", "Make the toon ramp based on a texture", keywordsList, ref updateKeywords, "Ramp Style") )
					{
						ShowFilteredProperties("#RAMPT#", props);
					}
					else
					{
						ShowFilteredProperties("#RAMPF#", props);
					}
				}
			}
			else
			{
				ShowFilteredProperties("#RAMPF#", props);
			}

			TCP2_GUI.Separator();
			
			//BUMP/NORMAL MAPPING ----------------------------------------------------------------

			if(CategoryFilter("BUMP"))
			{
				if(isGeneratedShader)
				{
					TCP2_GUI.HeaderAndHelp("BUMP/NORMAL MAPPING", "Normal/Bump map");

					ShowFilteredProperties("#NORM#", props);
					ShowFilteredProperties("#PLLX#", props);
				}
				else
				{
					if( TCP2_Utils.ShaderKeywordToggle("TCP2_BUMP", "BUMP/NORMAL MAPPING", "Enable bump mapping using normal maps", keywordsList, ref updateKeywords, "Normal/Bump map") )
					{
						ShowFilteredProperties("#NORM#", props);
					}
				}

				TCP2_GUI.Separator();
			}

			//SPECULAR ---------------------------------------------------------------------------

			if(CategoryFilter("SPECULAR", "SPECULAR_ANISOTROPIC"))
			{
				if(isGeneratedShader)
				{
					TCP2_GUI.HeaderAndHelp("SPECULAR", "Specular");
					ShowFilteredProperties("#SPEC#", props);
					if(HasFlags("SPECULAR_ANISOTROPIC"))
						ShowFilteredProperties("#SPECA#", props);
					if(HasFlags("SPECULAR_TOON"))
						ShowFilteredProperties("#SPECT#", props);
				}
				else
				{
					bool specular = TCP2_Utils.HasKeywords(keywordsList, "TCP2_SPEC", "TCP2_SPEC_TOON");
					TCP2_Utils.ShaderVariantUpdate("Specular", ShaderVariants, ShaderVariantsEnabled, specular, ref updateVariant);

					specular |= TCP2_Utils.ShaderKeywordRadio("SPECULAR", new string[]{"TCP2_SPEC_OFF","TCP2_SPEC","TCP2_SPEC_TOON"}, new GUIContent[]
					{
						new GUIContent("Off", "No Specular"),
						new GUIContent("Regular", "Default Blinn-Phong Specular"),
						new GUIContent("Cartoon", "Specular with smoothness control")
					},
					keywordsList, ref updateKeywords);

					if( specular )
					{
						ShowFilteredProperties("#SPEC#", props);

						bool specr = TCP2_Utils.HasKeywords(keywordsList, "TCP2_SPEC_TOON");
						if(specr)
						{
							ShowFilteredProperties("#SPECT#", props);
						}
					}
				}

				TCP2_GUI.Separator();
			}

			//REFLECTION -------------------------------------------------------------------------
			
			if(CategoryFilter("REFLECTION") && !isMobileShader)
			{
				if(isGeneratedShader)
				{
					TCP2_GUI.HeaderAndHelp("REFLECTION", "Reflection");
					
					ShowFilteredProperties("#REFL#", props);
#if UNITY_5
					if(HasFlags("U5_REFLPROBE"))
						ShowFilteredProperties("#REFL_U5#", props);
#endif
					if(HasFlags("REFL_COLOR"))
						ShowFilteredProperties("#REFLC#", props);
					if(HasFlags("REFL_ROUGH"))
					{
						ShowFilteredProperties("#REFLR#", props);
						EditorGUILayout.HelpBox("Cubemap Texture needs to have MipMaps enabled for Roughness to work!", MessageType.Info);
					}
				}
				else
				{
					bool reflection = TCP2_Utils.HasKeywords(keywordsList, "TCP2_REFLECTION", "TCP2_REFLECTION_MASKED");
					TCP2_Utils.ShaderVariantUpdate("Reflection", ShaderVariants, ShaderVariantsEnabled, reflection, ref updateVariant);
					
					reflection |= TCP2_Utils.ShaderKeywordRadio("REFLECTION", new string[]{"TCP2_REFLECTION_OFF","TCP2_REFLECTION","TCP2_REFLECTION_MASKED"}, new GUIContent[]
					{
						new GUIContent("Off", "No Cubemap Reflection"),
						new GUIContent("Global", "Global Cubemap Reflection"),
						new GUIContent("Masked", "Masked Cubemap Reflection (using the main texture's alpha channel)")
					},
					keywordsList, ref updateKeywords);
					
					if( reflection )
					{
#if UNITY_5
						//Reflection Probes toggle
						if( TCP2_Utils.ShaderKeywordToggle("TCP2_U5_REFLPROBE", "Use Reflection Probes", "Use Unity 5's Reflection Probes", keywordsList, ref updateKeywords) )
						{
							ShowFilteredProperties("#REFL_U5#", props);
						}
#endif
						ShowFilteredProperties("#REFL#", props);
					}
				}
				
				TCP2_GUI.Separator();
			}

			//MATCAP -----------------------------------------------------------------------------
			
			if(CategoryFilter("MATCAP"))
			{
				if(isGeneratedShader)
				{
					TCP2_GUI.Header("MATCAP");
					ShowFilteredProperties("#MC#", props);

					TCP2_GUI.Separator();
				}
				else if(isMobileShader)
				{
					bool matcap = TCP2_Utils.HasKeywords(keywordsList, "TCP2_MC", "TCP2_MCMASK");
					TCP2_Utils.ShaderVariantUpdate("Matcap", ShaderVariants, ShaderVariantsEnabled, matcap, ref updateVariant);
					
					matcap |= TCP2_Utils.ShaderKeywordRadio("MATCAP", new string[]{"TCP2_MC_OFF","TCP2_MC","TCP2_MCMASK"}, new GUIContent[]
					{
						new GUIContent("Off", "No MatCap reflection"),
						new GUIContent("Global", "Global additive MatCap"),
						new GUIContent("Masked", "Masked additive MatCap (using the main texture's alpha channel)")
					},
					keywordsList, ref updateKeywords);
					
					if( matcap )
					{
						ShowFilteredProperties("#MC#", props);
					}
					
					TCP2_GUI.Separator();
				}
				
			}

			//RIM --------------------------------------------------------------------------------

			if(CategoryFilter("RIM", "RIM_OUTLINE"))
			{
				if(isGeneratedShader)
				{
					TCP2_GUI.HeaderAndHelp("RIM", "Rim");
					
					ShowFilteredProperties("#RIM#", props);

					if(HasFlags("RIMDIR"))
					{
						ShowFilteredProperties("#RIMDIR#", props);

						if(HasFlags("PARALLAX"))
						{
							EditorGUILayout.HelpBox("Because it affects the view direction vector, Rim Direction may distort Parallax effect.", MessageType.Warning);
						}
					}
				}
				else
				{
					bool rim = TCP2_Utils.HasKeywords(keywordsList, "TCP2_RIM");
					bool rimOutline = TCP2_Utils.HasKeywords(keywordsList, "TCP2_RIMO");

					TCP2_Utils.ShaderVariantUpdate("Rim", ShaderVariants, ShaderVariantsEnabled, rim, ref updateVariant);
					TCP2_Utils.ShaderVariantUpdate("RimOutline", ShaderVariants, ShaderVariantsEnabled, rimOutline, ref updateVariant);
					
					rim |= rimOutline |= TCP2_Utils.ShaderKeywordRadio("RIM", new string[]{"TCP2_RIM_OFF","TCP2_RIM","TCP2_RIMO"}, new GUIContent[]
					{
						new GUIContent("Off", "No Rim effect"),
						new GUIContent("Lighting", "Rim lighting (additive)"),
						new GUIContent("Outline", "Rim outline (blended)")
					},
					keywordsList, ref updateKeywords);
					
					if( rim || rimOutline )
					{
						ShowFilteredProperties("#RIM#", props);
						
						if(CategoryFilter("RIMDIR"))
						{
							if( TCP2_Utils.ShaderKeywordToggle("TCP2_RIMDIR", "Directional Rim", "Enable directional rim control (rim calculation is approximated if enabled)", keywordsList, ref updateKeywords) )
							{
								ShowFilteredProperties("#RIMDIR#", props);
							}
						}
					}
				}

				TCP2_GUI.Separator();
			}

			//CUBEMAP AMBIENT --------------------------------------------------------------------
			
			if(CategoryFilter("CUBE_AMBIENT") && isGeneratedShader)
			{
				TCP2_GUI.HeaderAndHelp("CUBEMAP AMBIENT", "Cubemap Ambient");
				
				ShowFilteredProperties("#CUBEAMB#", props);
				
				TCP2_GUI.Separator();
			}

			//DIRECTIONAL AMBIENT --------------------------------------------------------------------

			if(CategoryFilter("DIRAMBIENT") && isGeneratedShader)
			{
				TCP2_GUI.HeaderAndHelp("DIRECTIONAL AMBIENT", "Directional Ambient");

				//TODO Special Inspector for DirAmb
				DirectionalAmbientGUI("#DAMB#", props);
//				ShowFilteredProperties("#DAMB#", props);
				
				TCP2_GUI.Separator();
			}

			//SKETCH --------------------------------------------------------------------------------
			
			if(CategoryFilter("SKETCH", "SKETCH_GRADIENT") && isGeneratedShader)
			{
				TCP2_GUI.HeaderAndHelp("SKETCH", "Sketch");
				
				bool sketch = HasFlags("SKETCH");
				bool sketchG = HasFlags("SKETCH_GRADIENT");
				
				if(sketch || sketchG)
					ShowFilteredProperties("#SKETCH#", props);
				
				if(sketchG)
					ShowFilteredProperties("#SKETCHG#", props);
				
				TCP2_GUI.Separator();
			}

			//OUTLINE --------------------------------------------------------------------------------

			if(CategoryFilter("OUTLINE", "OUTLINE_BLENDING"))
			{
				bool hasOutlineOpaque = false;
				bool hasOutlineBlending = false;
				bool hasOutline = false;

				if(isGeneratedShader)
				{
					TCP2_GUI.HeaderAndHelp("OUTLINE", "Outline");
					
					hasOutlineOpaque = HasFlags("OUTLINE");
					hasOutlineBlending = HasFlags("OUTLINE_BLENDING");
					hasOutline = hasOutlineOpaque || hasOutlineBlending;
				}
				else
				{
					hasOutlineOpaque = TCP2_Utils.HasKeywords(keywordsList, "OUTLINES");
					hasOutlineBlending = TCP2_Utils.HasKeywords(keywordsList, "OUTLINE_BLENDING");
					hasOutline = hasOutlineOpaque || hasOutlineBlending;

					TCP2_Utils.ShaderVariantUpdate("Outline", ShaderVariants, ShaderVariantsEnabled, hasOutlineOpaque, ref updateVariant);
					TCP2_Utils.ShaderVariantUpdate("OutlineBlending", ShaderVariants, ShaderVariantsEnabled, hasOutlineBlending, ref updateVariant);
					
					hasOutline |= TCP2_Utils.ShaderKeywordRadio("OUTLINE", new string[]{"OUTLINE_OFF","OUTLINES","OUTLINE_BLENDING"}, new GUIContent[]
					{
						new GUIContent("Off", "No Outline"),
						new GUIContent("Opaque", "Opaque Outline"),
						new GUIContent("Blended", "Allows transparent Outline and other effects")
					},
					keywordsList, ref updateKeywords);
				}

				if( hasOutline )
				{
					EditorGUI.indentLevel++;

					//Outline Type ---------------------------------------------------------------------------
					ShowFilteredProperties("#OUTLINE#", props, false);
					if(!isMobileShader && !HasFlags("FORCE_SM2"))
					{
						bool outlineTextured = TCP2_Utils.ShaderKeywordToggle("TCP2_OUTLINE_TEXTURED", "Outline Color from Texture", "If enabled, outline will take an averaged color from the main texture multiplied by Outline Color", keywordsList, ref updateKeywords);
						if(outlineTextured)
						{
							ShowFilteredProperties("#OUTLINETEX#", props);
						}
					}
					TCP2_Utils.ShaderKeywordToggle("TCP2_OUTLINE_CONST_SIZE", "Constant Size Outline", "If enabled, outline will have a constant size independently from camera distance", keywordsList, ref updateKeywords);
					if( TCP2_Utils.ShaderKeywordToggle("TCP2_ZSMOOTH_ON", "Correct Z Artefacts", "Enable the outline z-correction to try to hide artefacts from complex models", keywordsList, ref updateKeywords) )
					{
						ShowFilteredProperties("#OUTLINEZ#", props);
					}
					
					//Smoothed Normals -----------------------------------------------------------------------
					EditorGUI.indentLevel--;
					TCP2_GUI.Header("OUTLINE NORMALS", "Defines where to take the vertex normals from to draw the outline.\nChange this when using a smoothed mesh to fill the gaps shown in hard-edged meshes.");
					EditorGUI.indentLevel++;
					TCP2_Utils.ShaderKeywordRadio(null, new string[]{"TCP2_NONE", "TCP2_COLORS_AS_NORMALS", "TCP2_TANGENT_AS_NORMALS", "TCP2_UV2_AS_NORMALS"}, new GUIContent[]
					{
						new GUIContent("Regular", "Use regular vertex normals"),
						new GUIContent("Vertex Colors", "Use vertex colors as normals (with smoothed mesh)"),
						new GUIContent("Tangents", "Use tangents as normals (with smoothed mesh)"),
						new GUIContent("UV2", "Use second texture coordinates as normals (with smoothed mesh)"),
					},
					keywordsList, ref updateKeywords);
					EditorGUI.indentLevel--;

					//Outline Blending -----------------------------------------------------------------------

					if(hasOutlineBlending)
					{
						MaterialProperty[] blendProps = GetFilteredProperties("#BLEND#", props);

						if(blendProps.Length != 2)
						{
							EditorGUILayout.HelpBox("Couldn't find Blending properties!", MessageType.Error);
						}
						else
						{
							TCP2_GUI.Header("OUTLINE BLENDING", "BLENDING EXAMPLES:\nAlpha Transparency: SrcAlpha / OneMinusSrcAlpha\nMultiply: DstColor / Zero\nAdd: One / One\nSoft Add: OneMinusDstColor / One");

							UnityEngine.Rendering.BlendMode blendSrc = (UnityEngine.Rendering.BlendMode)blendProps[0].floatValue;
							UnityEngine.Rendering.BlendMode blendDst = (UnityEngine.Rendering.BlendMode)blendProps[1].floatValue;

							EditorGUI.BeginChangeCheck();
							float f = EditorGUIUtility.fieldWidth;
							float l = EditorGUIUtility.labelWidth;
							EditorGUIUtility.fieldWidth = 110f;
							EditorGUIUtility.labelWidth -= Mathf.Abs(f - EditorGUIUtility.fieldWidth);
							blendSrc = (UnityEngine.Rendering.BlendMode)EditorGUILayout.EnumPopup("Source Factor", blendSrc);
							blendDst = (UnityEngine.Rendering.BlendMode)EditorGUILayout.EnumPopup("Destination Factor", blendDst);
							EditorGUIUtility.fieldWidth = f;
							EditorGUIUtility.labelWidth = l;
							if(EditorGUI.EndChangeCheck())
							{
								blendProps[0].floatValue = (float)blendSrc;
								blendProps[1].floatValue = (float)blendDst;
							}
						}
					}
				}

				TCP2_GUI.Separator();
			}

			//LIGHTMAP --------------------------------------------------------------------------------

#if UNITY_4_5
			if(CategoryFilter("LIGHTMAP") && !isGeneratedShader)
			{
				TCP2_Utils.ShaderKeywordRadio("LIGHTMAP", new string[]{"TCP2_LIGHTMAP_OFF","TCP2_LIGHTMAP"}, new GUIContent[]{
					new GUIContent("Unity", "Use Unity's built-in lightmap decoding"),
					new GUIContent("Toony Colors Pro 2", "Use TCP2's lightmap decoding (lightmaps will be affected by ramp and color settings)")
				}, keywordsList, ref updateKeywords);
			}
#endif

			//TRANSPARENCY --------------------------------------------------------------------------------
			
			if(CategoryFilter("ALPHA", "CUTOUT") && isGeneratedShader)
			{
				bool alpha = false;
				bool cutout = false;

				if(isGeneratedShader)
				{
					TCP2_GUI.Header("TRANSPARENCY");

					alpha = HasFlags("ALPHA");
					cutout = HasFlags("CUTOUT");
				}

				if( alpha )
				{
					MaterialProperty[] blendProps = GetFilteredProperties("#ALPHA#", props);
					if(blendProps.Length != 2)
					{
						EditorGUILayout.HelpBox("Couldn't find Blending properties!", MessageType.Error);
					}
					else
					{
						TCP2_GUI.Header("BLENDING", "BLENDING EXAMPLES:\nAlpha Transparency: SrcAlpha / OneMinusSrcAlpha\nMultiply: DstColor / Zero\nAdd: One / One\nSoft Add: OneMinusDstColor / One");
						
						UnityEngine.Rendering.BlendMode blendSrc = (UnityEngine.Rendering.BlendMode)blendProps[0].floatValue;
						UnityEngine.Rendering.BlendMode blendDst = (UnityEngine.Rendering.BlendMode)blendProps[1].floatValue;
						
						EditorGUI.BeginChangeCheck();
						float f = EditorGUIUtility.fieldWidth;
						float l = EditorGUIUtility.labelWidth;
						EditorGUIUtility.fieldWidth = 110f;
						EditorGUIUtility.labelWidth -= Mathf.Abs(f - EditorGUIUtility.fieldWidth);
						blendSrc = (UnityEngine.Rendering.BlendMode)EditorGUILayout.EnumPopup("Source Factor", blendSrc);
						blendDst = (UnityEngine.Rendering.BlendMode)EditorGUILayout.EnumPopup("Destination Factor", blendDst);
						EditorGUIUtility.fieldWidth = f;
						EditorGUIUtility.labelWidth = l;
						if(EditorGUI.EndChangeCheck())
						{
							blendProps[0].floatValue = (float)blendSrc;
							blendProps[1].floatValue = (float)blendDst;
						}
					}
				}

				if( cutout )
				{
					ShowFilteredProperties("#CUTOUT#", props);
				}
			}
			
#if DEBUG_INFO
			//--------------------------------------------------------------------------------------
			//DEBUG --------------------------------------------------------------------------------

			TCP2_GUI.SeparatorBig();
			
			TCP2_GUI.Header("DEBUG");

			//Clear Keywords
			if(GUILayout.Button("Clear Keywords", EditorStyles.miniButton))
			{
				keywordsList.Clear();
				updateKeywords = true;
			}

			//Shader Flags
			GUILayout.Label("Features", EditorStyles.boldLabel);
			string features = "";
			if(mShaderFeatures != null)
			{
				foreach(string flag in mShaderFeatures)
				{
					features += flag + ", ";
				}
			}
			if(features.Length > 0)
				features = features.Substring(0, features.Length-2);

			GUILayout.Label(features, EditorStyles.wordWrappedMiniLabel);

			//Shader Keywords
			GUILayout.Label("Keywords", EditorStyles.boldLabel);
			string keywords = "";
			foreach(string keyword in keywordsList)
			{
				keywords += keyword + ", ";
			}
			if(keywords.Length > 0)
				keywords = keywords.Substring(0, keywords.Length-2);

			GUILayout.Label(keywords, EditorStyles.wordWrappedMiniLabel);
#endif
			//--------------------------------------------------------------------------------------

			if(EditorGUI.EndChangeCheck())
			{
				materialEditor.PropertiesChanged();
			}
		}

		//Update Keywords
		if(updateKeywords)
		{
			if(materialEditor.targets != null && materialEditor.targets.Length > 0)
			{
				foreach(Object t in materialEditor.targets)
				{
					(t as Material).shaderKeywords = keywordsList.ToArray();
					EditorUtility.SetDirty(t);
				}
			}
			else
			{
				targetMaterial.shaderKeywords = keywordsList.ToArray();
				EditorUtility.SetDirty(targetMaterial);
			}
		}

		//Update Variant
		if(updateVariant && !isGeneratedShader)
		{
			string baseName = isMobileShader ? BASE_SHADER_NAME_MOB : BASE_SHADER_NAME;

			string newShader = baseName;
			for(int i = 0; i < ShaderVariants.Count; i++)
			{
				if(ShaderVariantsEnabled[i])
				{
					newShader += " " + ShaderVariants[i];
				}
			}
			newShader = newShader.TrimEnd();

			//If variant shader
			string basePath = BASE_SHADER_PATH;
			if(newShader != baseName)
			{
				basePath = VARIANT_SHADER_PATH;
			}

			Shader shader = Shader.Find(basePath + newShader);
			if(shader != null)
			{
				materialEditor.SetShader(shader, false);

				mJustChangedShader = true;
			}
			else
			{
				if(Event.current.type != EventType.Layout)
				{
					mVariantError = "Can't find shader variant:\n" + basePath + newShader;
				}
				materialEditor.Repaint();
			}
		}
		else if(!string.IsNullOrEmpty(mVariantError) && Event.current.type != EventType.Layout)
		{
			mVariantError = null;
			materialEditor.Repaint();
		}

#endif
	}

	//--------------------------------------------------------------------------------------------------
	// Properties GUI

	//Hide parts of the GUI if the shader is compiled
	private bool CategoryFilter(params string[] filters)
	{
		if(!isGeneratedShader)
		{
			return true;
		}

		foreach(string filter in filters)
		{
			if(mShaderFeatures.Contains(filter))
			   return true;
		}

		return false;
	}

	private bool HasFlags(params string[] flags)
	{
		foreach(string flag in flags)
		{
			if(mShaderFeatures.Contains(flag))
				return true;
		}

		return false;
	}

	private bool ShowFilteredProperties(string filter, MaterialProperty[] properties, bool indent = true)
	{
		if(indent)
			EditorGUI.indentLevel++;

		bool propertiesShown = false;
		foreach (MaterialProperty p in properties)
		{
			if ((p.flags & (MaterialProperty.PropFlags.PerRendererData | MaterialProperty.PropFlags.HideInInspector)) == MaterialProperty.PropFlags.None)
				propertiesShown |= ShaderMaterialPropertyImpl(p, filter);
		}

		if(indent)
			EditorGUI.indentLevel--;

		return propertiesShown;
	}

	private MaterialProperty[] GetFilteredProperties(string filter, MaterialProperty[] properties, bool indent = true)
	{
		List<MaterialProperty> propList = new List<MaterialProperty>();

		foreach(MaterialProperty p in properties)
		{
			if(p.displayName.Contains(filter))
				propList.Add(p);
		}

		return propList.ToArray();
	}

	private bool ShaderMaterialPropertyImpl(MaterialProperty property, string filter = null)
	{
		//Filter
		string displayName = property.displayName;
		if(filter != null)
		{
			if(!displayName.Contains(filter))
				return false;

			displayName = displayName.Remove(displayName.IndexOf(filter), filter.Length+1);
		}
		else if(displayName.Contains("#"))
		{
			return false;
		}

		//GUI
		switch(property.type)
		{
		case MaterialProperty.PropType.Color:
			mMaterialEditor.ColorProperty(property, displayName);
			break;

		case MaterialProperty.PropType.Float:
			mMaterialEditor.FloatProperty(property, displayName);
			break;

		case MaterialProperty.PropType.Range:
			EditorGUILayout.BeginHorizontal();

			//Add float field to Range parameters
#if UNITY_4 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			float value = RangeProperty(property, displayName);
			Rect r = GUILayoutUtility.GetLastRect();
			r.x = r.width - 160f;
			r.width = 65f;
			value = EditorGUI.FloatField(r, value);
			if(property.floatValue != value)
			{
				property.floatValue = value;
			}
#else
			mMaterialEditor.RangeProperty(property, displayName);
#endif
			EditorGUILayout.EndHorizontal();
			break;

		case MaterialProperty.PropType.Texture:
			string nameLower = displayName.ToLower();
			bool showOffset = !nameLower.Contains("mask");
			showOffset &= !nameLower.Contains("matcap");
			if(!showOffset)
			{
				if(nameLower.Contains("mask 1"))		showOffset = mShaderFeatures.Contains("UVMASK1");
				else if(nameLower.Contains("mask 2"))	showOffset = mShaderFeatures.Contains("UVMASK2");
				else if(nameLower.Contains("mask 3"))	showOffset = mShaderFeatures.Contains("UVMASK3");
			}
			if(!isGeneratedShader)
			{
				showOffset = !nameLower.Contains("cubemap") && !nameLower.Contains("matcap (rgb)");
			}

			mMaterialEditor.TextureProperty(property, displayName, showOffset);
			break;

		case MaterialProperty.PropType.Vector:
			mMaterialEditor.VectorProperty(property, displayName);
			break;

		default:
			EditorGUILayout.LabelField("Unknown Material Property Type: " + property.type.ToString());
			break;
		}

		return true;
	}
	
	private void DirectionalAmbientGUI(string filter, MaterialProperty[] properties)
	{
		float width = (EditorGUIUtility.currentViewWidth-20)/6;
		EditorGUILayout.BeginHorizontal();
		foreach(MaterialProperty p in properties)
		{
			//Filter
			string displayName = p.displayName;
			if(filter != null)
			{
				if(!displayName.Contains(filter))
					continue;
				displayName = displayName.Remove(displayName.IndexOf(filter), filter.Length+1);
			}
			else if(displayName.Contains("#"))
				continue;

			GUILayout.Label(displayName, GUILayout.Width(width));
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		foreach(MaterialProperty p in properties)
		{
			//Filter
			string displayName = p.displayName;
			if(filter != null)
			{
				if(!displayName.Contains(filter))
					continue;
				displayName = displayName.Remove(displayName.IndexOf(filter), filter.Length+1);
			}
			else if(displayName.Contains("#"))
				continue;
			
			DirAmbientColorProperty(p, displayName, width);
		}
		EditorGUILayout.EndHorizontal();
	}

	private Color DirAmbientColorProperty(MaterialProperty prop, string label, float width)
	{
		EditorGUI.BeginChangeCheck();
		EditorGUI.showMixedValue = prop.hasMixedValue;
		Color colorValue = EditorGUILayout.ColorField(prop.colorValue, GUILayout.Width(width));
		EditorGUI.showMixedValue = false;
		if(EditorGUI.EndChangeCheck())
		{
			prop.colorValue = colorValue;
		}
		return prop.colorValue;
	}
}
