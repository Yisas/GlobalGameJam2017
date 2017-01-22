using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


internal class TCP2_MaterialInspector_PBS : ShaderGUI
{
	private enum WorkflowMode
	{
		Specular,
		Metallic,
		Dielectric
	}
	
	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,		// Old school alpha-blending mode, fresnel does not affect amount of transparency
		Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
	}
	
	private static class Styles
	{
		public static GUIStyle optionsButton = "PaneOptions";
		public static GUIContent uvSetLabel = new GUIContent("UV Set");
		public static GUIContent[] uvSetOptions = new GUIContent[] { new GUIContent("UV channel 0"), new GUIContent("UV channel 1") };

		public static string emptyTootip = "";
		public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
		public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
		public static GUIContent specularMapText = new GUIContent("Specular", "Specular (RGB) and Smoothness (A)");
		public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
		public static GUIContent smoothnessText = new GUIContent("Smoothness", "");
		public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
		public static GUIContent heightMapText = new GUIContent("Height Map", "Height Map (G)");
		public static GUIContent occlusionText = new GUIContent("Occlusion", "Occlusion (G)");
		public static GUIContent emissionText = new GUIContent("Emission", "Emission (RGB)");
		public static GUIContent detailMaskText = new GUIContent("Detail Mask", "Mask for Secondary Maps (A)");
		public static GUIContent detailAlbedoText = new GUIContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
		public static GUIContent detailNormalMapText = new GUIContent("Normal Map", "Normal Map");

		public static string whiteSpaceString = " ";
		public static string primaryMapsText = "Main Maps";
		public static string secondaryMapsText = "Secondary Maps";
		public static string renderingMode = "Rendering Mode";
		public static GUIContent emissiveWarning = new GUIContent ("Emissive value is animated but the material has not been configured to support emissive. Please make sure the material itself has some amount of emissive.");
		public static GUIContent emissiveColorWarning = new GUIContent ("Ensure emissive color is non-black for emission to have effect.");
		public static readonly string[] blendNames = Enum.GetNames (typeof (BlendMode));

		public static string tcp2_HeaderText = "Toony Colors Pro 2 - Stylization";
		public static string tcp2_highlightColorText = "Highlight Color";
		public static string tcp2_shadowColorText = "Shadow Color";
		public static GUIContent tcp2_rampText = new GUIContent("Ramp Texture", "Ramp 1D Texture (R)");
		public static GUIContent tcp2_rampThresholdText = new GUIContent("Threshold", "Threshold for the separation between shadows and highlights");
		public static GUIContent tcp2_rampSmoothText = new GUIContent("Main Light Smoothing", "Main Light smoothing of the separation between shadows and highlights");
		public static GUIContent tcp2_rampSmoothAddText = new GUIContent("Other Lights Smoothing", "Additional Lights smoothing of the separation between shadows and highlights");
		public static GUIContent tcp2_specSmoothText = new GUIContent("Specular Smoothing", "Stylized Specular smoothing");
		public static GUIContent tcp2_SpecBlendText = new GUIContent("Specular Blend", "Stylized Specular contribution over regular Specular");
		public static GUIContent tcp2_rimStrengthText = new GUIContent("Fresnel Strength", "Stylized Fresnel overall strength");
		public static GUIContent tcp2_rimMinText = new GUIContent("Fresnel Min", "Stylized Fresnel min ramp threshold");
		public static GUIContent tcp2_rimMaxText = new GUIContent("Fresnel Max", "Stylized Fresnel max ramp threshold");
		public static GUIContent tcp2_outlineColorText = new GUIContent("Outline Color", "Color of the outline");
		public static GUIContent tcp2_outlineWidthText = new GUIContent("Outline Width", "Width of the outline");

		public static string tcp2_TexLodText = "Outline Texture LOD";
		public static string tcp2_ZSmoothText = "ZSmooth Value";
		public static string tcp2_Offset1Text = "Offset Factor";
		public static string tcp2_Offset2Text = "Offset Units";
		public static string tcp2_srcBlendOutlineText = "Source Factor";
		public static string tcp2_dstBlendOutlineText = "Destination Factor";
	}

	MaterialProperty blendMode = null;
	MaterialProperty albedoMap = null;
	MaterialProperty albedoColor = null;
	MaterialProperty alphaCutoff = null;
	MaterialProperty specularMap = null;
	MaterialProperty specularColor = null;
	MaterialProperty metallicMap = null;
	MaterialProperty metallic = null;
	MaterialProperty smoothness = null;
	MaterialProperty bumpScale = null;
	MaterialProperty bumpMap = null;
	MaterialProperty occlusionStrength = null;
	MaterialProperty occlusionMap = null;
	MaterialProperty heigtMapScale = null;
	MaterialProperty heightMap = null;
	MaterialProperty emissionColorForRendering = null;
	MaterialProperty emissionMap = null;
	MaterialProperty detailMask = null;
	MaterialProperty detailAlbedoMap = null;
	MaterialProperty detailNormalMapScale = null;
	MaterialProperty detailNormalMap = null;
	MaterialProperty uvSetSecondary = null;

	//TCP2
	MaterialProperty tcp2_highlightColor = null;
	MaterialProperty tcp2_shadowColor = null;
	MaterialProperty tcp2_TCP2_DISABLE_WRAPPED_LIGHT = null;
	MaterialProperty tcp2_TCP2_RAMPTEXT = null;
	MaterialProperty tcp2_ramp = null;
	MaterialProperty tcp2_rampThreshold = null;
	MaterialProperty tcp2_rampSmooth = null;
	MaterialProperty tcp2_rampSmoothAdd = null;
	MaterialProperty tcp2_SPEC_TOON = null;
	MaterialProperty tcp2_specSmooth = null;
	MaterialProperty tcp2_SpecBlend = null;
	MaterialProperty tcp2_STYLIZED_FRESNEL = null;
	MaterialProperty tcp2_rimStrength = null;
	MaterialProperty tcp2_rimMin = null;
	MaterialProperty tcp2_rimMax = null;
	MaterialProperty tcp2_outlineColor = null;
	MaterialProperty tcp2_outlineWidth = null;
	MaterialProperty tcp2_TCP2_OUTLINE_TEXTURED = null;
	MaterialProperty tcp2_TexLod = null;
	MaterialProperty tcp2_TCP2_OUTLINE_CONST_SIZE = null;
	MaterialProperty tcp2_TCP2_ZSMOOTH_ON = null;
	MaterialProperty tcp2_ZSmooth = null;
	MaterialProperty tcp2_Offset1 = null;
	MaterialProperty tcp2_Offset2 = null;
	MaterialProperty tcp2_srcBlendOutline = null;
	MaterialProperty tcp2_dstBlendOutline = null;
	static bool expandStandardProperties = true;
	static bool expandTCP2Properties = true;
	readonly string[] outlineNormalsKeywords = new string[] { "TCP2_NONE", "TCP2_COLORS_AS_NORMALS", "TCP2_TANGENT_AS_NORMALS", "TCP2_UV2_AS_NORMALS" };

	MaterialEditor m_MaterialEditor;
	WorkflowMode m_WorkflowMode = WorkflowMode.Specular;
	readonly ColorPickerHDRConfig m_ColorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1/99f, 3f);

	bool m_FirstTimeApply = true;

	public void FindProperties (MaterialProperty[] props)
	{
		blendMode = FindProperty ("_Mode", props);
		albedoMap = FindProperty ("_MainTex", props);
		albedoColor = FindProperty ("_Color", props);
		alphaCutoff = FindProperty ("_Cutoff", props);
		specularMap = FindProperty ("_SpecGlossMap", props, false);
		specularColor = FindProperty ("_SpecColor", props, false);
		metallicMap = FindProperty ("_MetallicGlossMap", props, false);
		metallic = FindProperty ("_Metallic", props, false);
		if (specularMap != null && specularColor != null)
			m_WorkflowMode = WorkflowMode.Specular;
		else if (metallicMap != null && metallic != null)
			m_WorkflowMode = WorkflowMode.Metallic;
		else
			m_WorkflowMode = WorkflowMode.Dielectric;
		smoothness = FindProperty ("_Glossiness", props);
		bumpScale = FindProperty ("_BumpScale", props);
		bumpMap = FindProperty ("_BumpMap", props);
		heigtMapScale = FindProperty ("_Parallax", props);
		heightMap = FindProperty("_ParallaxMap", props);
		occlusionStrength = FindProperty ("_OcclusionStrength", props);
		occlusionMap = FindProperty ("_OcclusionMap", props);
		emissionColorForRendering = FindProperty ("_EmissionColor", props);
		emissionMap = FindProperty ("_EmissionMap", props);
		detailMask = FindProperty ("_DetailMask", props);
		detailAlbedoMap = FindProperty ("_DetailAlbedoMap", props);
		detailNormalMapScale = FindProperty ("_DetailNormalMapScale", props);
		detailNormalMap = FindProperty ("_DetailNormalMap", props);
		uvSetSecondary = FindProperty ("_UVSec", props);

		//TCP2
		tcp2_highlightColor = FindProperty("_HColor", props);
		tcp2_shadowColor = FindProperty("_SColor", props);

		tcp2_rampThreshold = FindProperty("_RampThreshold", props);
		tcp2_rampSmooth = FindProperty("_RampSmooth", props);
		tcp2_rampSmoothAdd = FindProperty("_RampSmoothAdd", props);
		tcp2_TCP2_DISABLE_WRAPPED_LIGHT = FindProperty("_TCP2_DISABLE_WRAPPED_LIGHT", props);
		tcp2_TCP2_RAMPTEXT = FindProperty("_TCP2_RAMPTEXT", props);
		tcp2_ramp = FindProperty("_Ramp", props);

		tcp2_SPEC_TOON = FindProperty("_TCP2_SPEC_TOON", props);
		tcp2_specSmooth = FindProperty("_SpecSmooth", props);
		tcp2_SpecBlend = FindProperty("_SpecBlend", props);

		tcp2_STYLIZED_FRESNEL = FindProperty("_TCP2_STYLIZED_FRESNEL", props);
		tcp2_rimStrength = FindProperty("_RimStrength", props);
		tcp2_rimMin = FindProperty("_RimMin", props);
		tcp2_rimMax = FindProperty("_RimMax", props);

		tcp2_outlineColor = FindProperty("_OutlineColor", props, false);
		tcp2_outlineWidth = FindProperty("_Outline", props, false);
		tcp2_TCP2_OUTLINE_TEXTURED = FindProperty("_TCP2_OUTLINE_TEXTURED", props, false);
		tcp2_TexLod = FindProperty("_TexLod", props, false);
		tcp2_TCP2_OUTLINE_CONST_SIZE = FindProperty("_TCP2_OUTLINE_CONST_SIZE", props, false);
		tcp2_TCP2_ZSMOOTH_ON = FindProperty("_TCP2_ZSMOOTH_ON", props, false);
		tcp2_ZSmooth = FindProperty("_ZSmooth", props, false);
		tcp2_Offset1 = FindProperty("_Offset1", props, false);
		tcp2_Offset2 = FindProperty("_Offset2", props, false);
		tcp2_srcBlendOutline = FindProperty("_SrcBlendOutline", props, false);
		tcp2_dstBlendOutline = FindProperty("_DstBlendOutline", props, false);
	}

	public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
	{
		FindProperties (props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
		m_MaterialEditor = materialEditor;
		Material material = materialEditor.target as Material;

		ShaderPropertiesGUI (material);

		// Make sure that needed keywords are set up if we're switching some existing
		// material to a standard shader.
		if (m_FirstTimeApply)
		{
			SetMaterialKeywords (material, m_WorkflowMode);
			m_FirstTimeApply = false;
		}
	}

	public void ShaderPropertiesGUI (Material material)
	{
		// Use default labelWidth
		EditorGUIUtility.labelWidth = 0f;

		// Detect any changes to the material
		EditorGUI.BeginChangeCheck();
		{
			BlendModePopup();

			GUILayout.Space(8f);
			expandStandardProperties = GUILayout.Toggle(expandStandardProperties, "STANDARD PROPERTIES", EditorStyles.toolbarButton);
			if (expandStandardProperties)
			{
				//Background
				Rect vertRect = EditorGUILayout.BeginVertical();
				vertRect.xMax += 2;
				vertRect.xMin--;
				GUI.Box(vertRect, "", (GUIStyle)"RL Background");
				GUILayout.Space(4f);

				// Primary properties
				GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
				DoAlbedoArea(material);
				DoSpecularMetallicArea();
				m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap,
				                                           bumpMap.textureValue != null ? bumpScale : null);
				m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap,
				                                           heightMap.textureValue != null ? heigtMapScale : null);
				m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap,
				                                           occlusionMap.textureValue != null ? occlusionStrength : null);
				DoEmissionArea(material);
				m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
				EditorGUI.BeginChangeCheck();
				m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
				if (EditorGUI.EndChangeCheck())
					emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset;
						// Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake

				EditorGUILayout.Space();

				// Secondary properties
				GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
				m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
				m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
				m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
				m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

				GUILayout.Space(8f);
				EditorGUILayout.EndVertical();
			}

			EditorGUILayout.Space();

			//----------------------------------------------------------------
			//    TOONY COLORS PRO 2

			bool useOutline = (m_MaterialEditor.target as Material).shaderKeywords.Contains("OUTLINES");
			bool useOutlineBlended = (m_MaterialEditor.target as Material).shaderKeywords.Contains("OUTLINE_BLENDING");

			bool hasOutlineShader = tcp2_outlineWidth != null;
			bool hasOutlineBlendedShader = tcp2_srcBlendOutline != null;

			bool useOutlineNew = useOutline;
			bool useOutlineBlendedNew = useOutlineBlended;

			expandTCP2Properties = GUILayout.Toggle(expandTCP2Properties, "TOONY COLORS PRO 2", EditorStyles.toolbarButton);
			if (expandTCP2Properties)
			{
				//Background
				Rect vertRect = EditorGUILayout.BeginVertical();
				vertRect.xMax += 2;
				vertRect.xMin--;
				GUI.Box(vertRect, "", (GUIStyle)"RL Background");
				GUILayout.Space(4f);

				GUILayout.Label("Base Properties", EditorStyles.boldLabel);
				m_MaterialEditor.ColorProperty(tcp2_highlightColor, Styles.tcp2_highlightColorText);
				m_MaterialEditor.ColorProperty(tcp2_shadowColor, Styles.tcp2_shadowColorText);

				// Wrapped Lighting
				m_MaterialEditor.ShaderProperty(tcp2_TCP2_DISABLE_WRAPPED_LIGHT, "Disable Wrapped Lighting");

				// Ramp Texture / Threshold
				m_MaterialEditor.ShaderProperty(tcp2_TCP2_RAMPTEXT, "Use Ramp Texture");
				if (tcp2_TCP2_RAMPTEXT.floatValue > 0)
				{
					EditorGUI.indentLevel++;
					m_MaterialEditor.TexturePropertySingleLine(Styles.tcp2_rampText, tcp2_ramp);
					EditorGUI.indentLevel--;
				}
				else
				{
					m_MaterialEditor.ShaderProperty(tcp2_rampThreshold, Styles.tcp2_rampThresholdText.text, 1);
					m_MaterialEditor.ShaderProperty(tcp2_rampSmooth, Styles.tcp2_rampSmoothText.text, 1);
					m_MaterialEditor.ShaderProperty(tcp2_rampSmoothAdd, Styles.tcp2_rampSmoothAddText.text, 1);
				}

				EditorGUILayout.Space();
				GUILayout.Label("Stylization Options", EditorStyles.boldLabel);

				// Stylized Specular
				m_MaterialEditor.ShaderProperty(tcp2_SPEC_TOON, "Stylized Specular");
				if (tcp2_SPEC_TOON.floatValue > 0)
				{
					m_MaterialEditor.ShaderProperty(tcp2_specSmooth, Styles.tcp2_specSmoothText.text, 1);
					m_MaterialEditor.ShaderProperty(tcp2_SpecBlend, Styles.tcp2_SpecBlendText.text, 1);

					EditorGUILayout.Space();
				}

				//Stylized Fresnel
				m_MaterialEditor.ShaderProperty(tcp2_STYLIZED_FRESNEL, "Stylized Fresnel");
				if (tcp2_STYLIZED_FRESNEL.floatValue > 0)
				{
					m_MaterialEditor.ShaderProperty(tcp2_rimStrength, Styles.tcp2_rimStrengthText.text, 1);
					m_MaterialEditor.ShaderProperty(tcp2_rimMin, Styles.tcp2_rimMinText.text, 1);
					m_MaterialEditor.ShaderProperty(tcp2_rimMax, Styles.tcp2_rimMaxText.text, 1);

					EditorGUILayout.Space();
				}

				//Outline
				useOutlineNew = EditorGUILayout.Toggle(new GUIContent("Outline", "Enable mesh-based outline"), useOutline);
				if (useOutline && hasOutlineShader)
				{
					//Outline base props
					m_MaterialEditor.ShaderProperty(tcp2_outlineColor, Styles.tcp2_outlineColorText.text, 1);
					m_MaterialEditor.ShaderProperty(tcp2_outlineWidth, Styles.tcp2_outlineWidthText.text, 1);

					m_MaterialEditor.ShaderProperty(tcp2_TCP2_OUTLINE_TEXTURED, "Textured Outline", 1);
					if (tcp2_TCP2_OUTLINE_TEXTURED.floatValue > 0)
					{
						m_MaterialEditor.ShaderProperty(tcp2_TexLod, Styles.tcp2_TexLodText, 1);
					}

					m_MaterialEditor.ShaderProperty(tcp2_TCP2_OUTLINE_CONST_SIZE, "Constant Screen Size", 1);
					m_MaterialEditor.ShaderProperty(tcp2_TCP2_ZSMOOTH_ON, "Z Smooth", 1);
					if (tcp2_TCP2_ZSMOOTH_ON.floatValue > 0)
					{
						m_MaterialEditor.ShaderProperty(tcp2_ZSmooth, Styles.tcp2_ZSmoothText, 2);
						m_MaterialEditor.ShaderProperty(tcp2_Offset1, Styles.tcp2_Offset1Text, 2);
						m_MaterialEditor.ShaderProperty(tcp2_Offset2, Styles.tcp2_Offset2Text, 2);
					}

					//Blended Outline
					EditorGUI.indentLevel++;
					useOutlineBlendedNew = EditorGUILayout.Toggle(new GUIContent("Blended Outline", "Enable blended outline rather than opaque"), useOutlineBlended);
					if (useOutlineBlended && hasOutlineBlendedShader)
					{
						EditorGUI.indentLevel++;
						UnityEngine.Rendering.BlendMode blendSrc = (UnityEngine.Rendering.BlendMode)tcp2_srcBlendOutline.floatValue;
						UnityEngine.Rendering.BlendMode blendDst = (UnityEngine.Rendering.BlendMode)tcp2_dstBlendOutline.floatValue;
						EditorGUI.BeginChangeCheck();
						blendSrc = (UnityEngine.Rendering.BlendMode)EditorGUILayout.EnumPopup(Styles.tcp2_srcBlendOutlineText, blendSrc);
						blendDst = (UnityEngine.Rendering.BlendMode)EditorGUILayout.EnumPopup(Styles.tcp2_dstBlendOutlineText, blendDst);
						if (EditorGUI.EndChangeCheck())
						{
							tcp2_srcBlendOutline.floatValue = (float)blendSrc;
							tcp2_dstBlendOutline.floatValue = (float)blendDst;
						}
						EditorGUI.indentLevel--;
					}
					EditorGUI.indentLevel--;

					//Outline Normals
					int onIndex = GetOutlineNormalsIndex();
					int newIndex = onIndex;
					EditorGUI.indentLevel++;
					if (Screen.width < 390f)
					{
						newIndex = TCP2_Utils.ShaderKeywordRadioGeneric("Outline Normals", newIndex, new GUIContent[]
						{
							new GUIContent("R", "Use regular vertex normals"),
							new GUIContent("VC", "Use vertex colors as normals (with smoothed mesh)"),
							new GUIContent("T", "Use tangents as normals (with smoothed mesh)"),
							new GUIContent("UV2", "Use second texture coordinates as normals (with smoothed mesh)"),
						});
					}
					else if (Screen.width < 560f)
					{
						newIndex = TCP2_Utils.ShaderKeywordRadioGeneric("Outline Normals", newIndex, new GUIContent[]
						{
							new GUIContent("Regular", "Use regular vertex normals"),
							new GUIContent("VColors", "Use vertex colors as normals (with smoothed mesh)"),
							new GUIContent("Tangents", "Use tangents as normals (with smoothed mesh)"),
							new GUIContent("UV2", "Use second texture coordinates as normals (with smoothed mesh)"),
						});
					}
					else
					{
						newIndex = TCP2_Utils.ShaderKeywordRadioGeneric("Outline Normals", newIndex, new GUIContent[]
						{
							new GUIContent("Regular", "Use regular vertex normals"),
							new GUIContent("Vertex Colors", "Use vertex colors as normals (with smoothed mesh)"),
							new GUIContent("Tangents", "Use tangents as normals (with smoothed mesh)"),
							new GUIContent("UV2", "Use second texture coordinates as normals (with smoothed mesh)"),
						});
					}
					EditorGUI.indentLevel--;
					if (newIndex != onIndex)
					{
						UpdateOutlineNormalsKeyword(newIndex);
					}
				}

				GUILayout.Space(8f);
				GUILayout.EndVertical();

				// TCP2 End
				//----------------------------------------------------------------
			}

			GUILayout.Space(10f);

			//TCP2: set correct shader based on outline properties
			if (useOutline != useOutlineNew || useOutlineBlended != useOutlineBlendedNew)
			{
				SetTCP2Shader(useOutlineNew, useOutlineBlendedNew);
			}
			else if (useOutline != hasOutlineShader || useOutlineBlended != hasOutlineBlendedShader)
			{
				SetTCP2Shader(useOutline, useOutlineBlended);
			}
		}
		if (EditorGUI.EndChangeCheck())
		{
			foreach (var obj in blendMode.targets)
				MaterialChanged((Material)obj, m_WorkflowMode);
		}
	}

	void UpdateOutlineNormalsKeyword(int index)
	{
		string selectedKeyword = outlineNormalsKeywords[index];

		foreach (var obj in m_MaterialEditor.targets)
		{
			if (obj is Material)
			{
				Material m = obj as Material;
				foreach (var kw in outlineNormalsKeywords)
					m.DisableKeyword(kw);
				m.EnableKeyword(selectedKeyword);
			}
		}
	}

	internal void DetermineWorkflow(MaterialProperty[] props)
	{
		if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
			m_WorkflowMode = WorkflowMode.Specular;
		else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
			m_WorkflowMode = WorkflowMode.Metallic;
		else
			m_WorkflowMode = WorkflowMode.Dielectric;
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader)
	{
		// _Emission property is lost after assigning Standard shader to the material
		// thus transfer it before assigning the new shader
		if (material.HasProperty("_Emission"))
		{
			material.SetColor("_EmissionColor", material.GetColor("_Emission"));
		}

		base.AssignNewShaderToMaterial(material, oldShader, newShader);

		if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
			return;

		BlendMode blendMode = BlendMode.Opaque;
		if (oldShader.name.Contains("/Transparent/Cutout/"))
		{
			blendMode = BlendMode.Cutout;
		}
		else if (oldShader.name.Contains("/Transparent/"))
		{
			// NOTE: legacy shaders did not provide physically based transparency
			// therefore Fade mode
			blendMode = BlendMode.Fade;
		}
		material.SetFloat("_Mode", (float)blendMode);

		DetermineWorkflow( MaterialEditor.GetMaterialProperties (new Material[] { material }) );
		MaterialChanged(material, m_WorkflowMode);
	}

	void BlendModePopup()
	{
		EditorGUI.showMixedValue = blendMode.hasMixedValue;
		var mode = (BlendMode)blendMode.floatValue;

		EditorGUI.BeginChangeCheck();
		mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
		if (EditorGUI.EndChangeCheck())
		{
			m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
			blendMode.floatValue = (float)mode;
		}

		EditorGUI.showMixedValue = false;
	}

	void DoAlbedoArea(Material material)
	{
		m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
		if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
		{
			m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel+1);
		}
	}

	void DoEmissionArea(Material material)
	{
		float brightness = emissionColorForRendering.colorValue.maxColorComponent;
		bool showHelpBox = !HasValidEmissiveKeyword(material);
		bool showEmissionColorAndGIControls = brightness > 0.0f;
		
		bool hadEmissionTexture = emissionMap.textureValue != null;

		// Texture and HDR color controls
		m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, m_ColorPickerHDRConfig, false);

		// If texture was assigned and color was black set color to white
		if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
			emissionColorForRendering.colorValue = Color.white;

		// Dynamic Lightmapping mode
		if (showEmissionColorAndGIControls)
		{
			bool shouldEmissionBeEnabled = ShouldEmissionBeEnabled(emissionColorForRendering.colorValue);
			EditorGUI.BeginDisabledGroup(!shouldEmissionBeEnabled);

			m_MaterialEditor.LightmapEmissionProperty (MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);

			EditorGUI.EndDisabledGroup();
		}

		if (showHelpBox)
		{
			EditorGUILayout.HelpBox(Styles.emissiveWarning.text, MessageType.Warning);
		}
	}

	void DoSpecularMetallicArea()
	{
		if (m_WorkflowMode == WorkflowMode.Specular)
		{
			if (specularMap.textureValue == null)
				m_MaterialEditor.TexturePropertyTwoLines(Styles.specularMapText, specularMap, specularColor, Styles.smoothnessText, smoothness);
			else
				m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap);

		}
		else if (m_WorkflowMode == WorkflowMode.Metallic)
		{
			if (metallicMap.textureValue == null)
				m_MaterialEditor.TexturePropertyTwoLines(Styles.metallicMapText, metallicMap, metallic, Styles.smoothnessText, smoothness);
			else
				m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap);
		}
	}

	public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				material.SetOverrideTag("RenderType", "TransparentCutout");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2450;
				break;
			case BlendMode.Fade:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
		}
	}
	
	static bool ShouldEmissionBeEnabled (Color color)
	{
		return color.maxColorComponent > (0.1f / 255.0f);
	}

	static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
	{
		// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
		// (MaterialProperty value might come from renderer material property block)
		SetKeyword (material, "_NORMALMAP", material.GetTexture ("_BumpMap") || material.GetTexture ("_DetailNormalMap"));
		if (workflowMode == WorkflowMode.Specular)
			SetKeyword (material, "_SPECGLOSSMAP", material.GetTexture ("_SpecGlossMap"));
		else if (workflowMode == WorkflowMode.Metallic)
			SetKeyword (material, "_METALLICGLOSSMAP", material.GetTexture ("_MetallicGlossMap"));
		SetKeyword (material, "_PARALLAXMAP", material.GetTexture ("_ParallaxMap"));
		SetKeyword (material, "_DETAIL_MULX2", material.GetTexture ("_DetailAlbedoMap") || material.GetTexture ("_DetailNormalMap"));

		bool shouldEmissionBeEnabled = ShouldEmissionBeEnabled (material.GetColor("_EmissionColor"));
		SetKeyword (material, "_EMISSION", shouldEmissionBeEnabled);

		// Setup lightmap emissive flags
		MaterialGlobalIlluminationFlags flags = material.globalIlluminationFlags;
		if ((flags & (MaterialGlobalIlluminationFlags.BakedEmissive | MaterialGlobalIlluminationFlags.RealtimeEmissive)) != 0)
		{
			flags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
			if (!shouldEmissionBeEnabled)
				flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;

			material.globalIlluminationFlags = flags;
		}
	}

	bool HasValidEmissiveKeyword (Material material)
	{
		// Material animation might be out of sync with the material keyword.
		// So if the emission support is disabled on the material, but the property blocks have a value that requires it, then we need to show a warning.
		// (note: (Renderer MaterialPropertyBlock applies its values to emissionColorForRendering))
		bool hasEmissionKeyword = material.IsKeywordEnabled ("_EMISSION");
		if (!hasEmissionKeyword && ShouldEmissionBeEnabled (emissionColorForRendering.colorValue))
			return false;
		else
			return true;
	}

	static void MaterialChanged(Material material, WorkflowMode workflowMode)
	{
		SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

		SetMaterialKeywords(material, workflowMode);
	}

	static void SetKeyword(Material m, string keyword, bool state)
	{
		if (state)
			m.EnableKeyword (keyword);
		else
			m.DisableKeyword (keyword);
	}

	//TCP2 Tools

	int GetOutlineNormalsIndex()
	{
		if (m_MaterialEditor.target == null || !(m_MaterialEditor.target is Material))
			return 0;

		for (int i = 0; i < outlineNormalsKeywords.Length; i++)
		{
			if ((m_MaterialEditor.target as Material).IsKeywordEnabled(outlineNormalsKeywords[i]))
				return i;
		}
		return 0;
	}

	void SetTCP2Shader( bool useOutline, bool blendedOutline )
	{
		bool specular = m_WorkflowMode == WorkflowMode.Specular;
		string shaderPath = null;

		if (!useOutline)
		{
			if(specular)
				shaderPath = "Toony Colors Pro 2/Standard PBS (Specular)";
			else
				shaderPath = "Toony Colors Pro 2/Standard PBS";
		}
		else if (blendedOutline)
		{
			if (specular)
				shaderPath = "Hidden/Toony Colors Pro 2/Standard PBS Outline Blended (Specular)";
			else
				shaderPath = "Hidden/Toony Colors Pro 2/Standard PBS Outline Blended";
		}
		else
		{
			if (specular)
				shaderPath = "Hidden/Toony Colors Pro 2/Standard PBS Outline (Specular)";
			else
				shaderPath = "Hidden/Toony Colors Pro 2/Standard PBS Outline";
		}

		Shader shader = Shader.Find(shaderPath);
		if (shader != null)
		{
			if ((m_MaterialEditor.target as Material).shader != shader)
			{
				m_MaterialEditor.SetShader(shader, false);
			}

			foreach (var obj in m_MaterialEditor.targets)
			{
				if (obj is Material)
				{
					if (blendedOutline)
						(obj as Material).EnableKeyword("OUTLINE_BLENDING");
					else
						(obj as Material).DisableKeyword("OUTLINE_BLENDING");

					if (useOutline)
						(obj as Material).EnableKeyword("OUTLINES");
					else
						(obj as Material).DisableKeyword("OUTLINES");
				}
			}

			m_MaterialEditor.Repaint();
			SceneView.RepaintAll();
		}
		else
		{
			EditorApplication.Beep();
			Debug.LogError("Toony Colors Pro 2: Couldn't find the following shader:\n\""+shaderPath+"\"");
		}
	}
}
