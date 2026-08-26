// Assets/Editor/ConvertMaterialToUrpComplexLit.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace gamesolids
{
	/// <summary>
	/// Displays a conversion window for changing selected materials to the URP Complex Lit shader.
	/// </summary>
	/// <remarks>
	/// This tool collects selected material assets, previews them in an editor window,
	/// and converts them while preserving common texture, color, scale, and offset data.
	/// This will continue to be less useful as URP development/adoption increases, but
	/// still useful for converting older assets.
	/// </remarks>
	public class ConvertMaterialToUrpComplexLit : EditorWindow
	{
		private List<Material> materials = new List<Material>();
		private Vector2 scroll;
		private const string TargetShaderName = "Universal Render Pipeline/Complex Lit";

		/// <summary>
		/// Opens the converter window using the currently selected material assets.
		/// </summary>
		[MenuItem("Tools/GameSolids/Convert Selected To URP Complex Lit")]
		public static void ConvertSelectedMenu()
		{
			var selectedMats = new List<Material>();

			foreach (var obj in Selection.objects)
			{
				if (obj is Material mat)
					selectedMats.Add(mat);
			}

			if (selectedMats.Count == 0)
			{
				EditorUtility.DisplayDialog(
					"No Materials Selected",
					"Select one or more Material assets in the Project window.",
					"OK"
				);
				return;
			}

			var window = GetWindow<ConvertMaterialToUrpComplexLit>("URP Material Converter");
			window.materials = selectedMats;
			window.Show();
		}

		/// <summary>
		/// Draws the converter window user interface.
		/// </summary>
		private void OnGUI()
		{
			EditorGUILayout.LabelField("Convert Material To URP/Complex Lit", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			if (materials == null || materials.Count == 0)
			{
				EditorGUILayout.HelpBox("Select material assets, then use Tools > Materials > Convert Selected To URP Complex Lit.", MessageType.Info);
				return;
			}

			EditorGUILayout.LabelField($"Materials queued: {materials.Count}");
			EditorGUILayout.Space();

			scroll = EditorGUILayout.BeginScrollView(scroll);
			foreach (var mat in materials)
			{
				EditorGUILayout.ObjectField(mat, typeof(Material), false);
			}
			EditorGUILayout.EndScrollView();

			EditorGUILayout.Space();

			if (GUILayout.Button("Convert"))
			{
				ConvertMaterials(materials);
			}
		}

		/// <summary>
		/// Converts a list of materials to the target URP Complex Lit shader.
		/// </summary>
		/// <param name="mats">The materials to convert.</param>
		private static void ConvertMaterials(List<Material> mats)
		{
			Shader targetShader = Shader.Find(TargetShaderName);
			if (targetShader == null)
			{
				EditorUtility.DisplayDialog(
					"Shader Not Found",
					$"Could not find shader: {TargetShaderName}\nMake sure URP is installed.",
					"OK"
				);
				return;
			}

			int converted = 0;

			AssetDatabase.StartAssetEditing();
			try
			{
				foreach (var mat in mats)
				{
					if (mat == null)
						continue;

					ConvertOneMaterial(mat, targetShader);
					converted++;
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			EditorUtility.DisplayDialog(
				"Conversion Complete",
				$"Converted {converted} material(s) to {TargetShaderName}.",
				"OK"
			);
		}

		/// <summary>
		/// Converts a single material to the target shader while preserving common texture and color data.
		/// </summary>
		/// <param name="mat">The material to convert.</param>
		/// <param name="targetShader">The shader to assign to the material.</param>
		private static void ConvertOneMaterial(Material mat, Shader targetShader)
		{
			Undo.RecordObject(mat, "Convert Material To URP Complex Lit");

			// Capture likely source textures before changing shader.
			Texture diffuseTex = FindFirstTexture(mat, new[]
			{
			"_BaseMap",       // already URP-like
            "_MainTex",       // common built-in / older shaders
            "_BaseColorMap",  // HDRP / some SRP workflows
            "_AlbedoMap",     // custom shaders
            "_DiffuseMap"     // custom shaders
			});

			Texture normalTex = FindFirstTexture(mat, new[]
			{
				"_BumpMap",
				"_NormalMap"
			});

			Texture metallicTex = FindFirstTexture(mat, new[]
			{
				"_MetallicGlossMap",
				"_MetallicMap"
			});

			Texture occlusionTex = FindFirstTexture(mat, new[]
			{
				"_OcclusionMap"
			});

			Texture heightTex = FindFirstTexture(mat, new[]
			{
				"_ParallaxMap",
				"_HeightMap"
			});

			Color baseColor = FindFirstColor(mat, new[]
			{
				"_BaseColor",
				"_Color"
			}, Color.white);

			Vector2 texScale = FindFirstTextureScale(mat, new[]
			{
				"_BaseMap",
				"_MainTex",
				"_BaseColorMap"
			}, Vector2.one);

			Vector2 texOffset = FindFirstTextureOffset(mat, new[]
			{
				"_BaseMap",
				"_MainTex",
				"_BaseColorMap"
			}, Vector2.zero);

			// Keep a temp copy so matching properties can still be copied after shader swap.
			Material oldSnapshot = new Material(mat);

			// Switch shader first.
			mat.shader = targetShader;

			// Copy any properties that still match by name between old and new shaders.
			mat.CopyMatchingPropertiesFromMaterial(oldSnapshot);

			// Manual remap for diffuse/albedo -> URP Base Map.
			if (diffuseTex != null && mat.HasProperty("_BaseMap"))
			{
				mat.SetTexture("_BaseMap", diffuseTex);
				mat.SetTextureScale("_BaseMap", texScale);
				mat.SetTextureOffset("_BaseMap", texOffset);
			}

			if (mat.HasProperty("_BaseColor"))
			{
				mat.SetColor("_BaseColor", baseColor);
			}

			// Re-apply common maps in case the copy missed them.
			if (normalTex != null && mat.HasProperty("_BumpMap"))
			{
				mat.SetTexture("_BumpMap", normalTex);
				mat.EnableKeyword("_NORMALMAP");
			}

			if (metallicTex != null && mat.HasProperty("_MetallicGlossMap"))
			{
				mat.SetTexture("_MetallicGlossMap", metallicTex);
			}

			if (mat.HasProperty("_SmoothnessTextureChannel"))
			{
				mat.SetFloat("_SmoothnessTextureChannel", 1f); // Albedo Alpha
			}

			if (occlusionTex != null && mat.HasProperty("_OcclusionMap"))
			{
				mat.SetTexture("_OcclusionMap", occlusionTex);
			}

			if (heightTex != null && mat.HasProperty("_ParallaxMap"))
			{
				mat.SetTexture("_ParallaxMap", heightTex);
			}

			EditorUtility.SetDirty(mat);

			Object.DestroyImmediate(oldSnapshot);
		}

		/// <summary>
		/// Returns the first non-null texture found on the material from the provided property names.
		/// </summary>
		/// <param name="mat">The material to inspect.</param>
		/// <param name="propertyNames">The shader property names to check in order.</param>
		/// <returns>The first matching texture, or <see langword="null"/> if none is found.</returns>
		private static Texture FindFirstTexture(Material mat, string[] propertyNames)
		{
			foreach (string prop in propertyNames)
			{
				if (mat.HasProperty(prop))
				{
					Texture tex = mat.GetTexture(prop);
					if (tex != null)
						return tex;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the first available color value from the provided material property names.
		/// </summary>
		/// <param name="mat">The material to inspect.</param>
		/// <param name="propertyNames">The shader property names to check in order.</param>
		/// <param name="fallback">The fallback value if no matching property exists.</param>
		/// <returns>The first matching color, or the fallback value.</returns>
		private static Color FindFirstColor(Material mat, string[] propertyNames, Color fallback)
		{
			foreach (string prop in propertyNames)
			{
				if (mat.HasProperty(prop))
					return mat.GetColor(prop);
			}
			return fallback;
		}

		/// <summary>
		/// Returns the first available texture scale from the provided material property names.
		/// </summary>
		/// <param name="mat">The material to inspect.</param>
		/// <param name="propertyNames">The shader property names to check in order.</param>
		/// <param name="fallback">The fallback value if no matching property exists.</param>
		/// <returns>The first matching texture scale, or the fallback value.</returns>
		private static Vector2 FindFirstTextureScale(Material mat, string[] propertyNames, Vector2 fallback)
		{
			foreach (string prop in propertyNames)
			{
				if (mat.HasProperty(prop))
					return mat.GetTextureScale(prop);
			}
			return fallback;
		}

		/// <summary>
		/// Returns the first available texture offset from the provided material property names.
		/// </summary>
		/// <param name="mat">The material to inspect.</param>
		/// <param name="propertyNames">The shader property names to check in order.</param>
		/// <param name="fallback">The fallback value if no matching property exists.</param>
		/// <returns>The first matching texture offset, or the fallback value.</returns>
		private static Vector2 FindFirstTextureOffset(Material mat, string[] propertyNames, Vector2 fallback)
		{
			foreach (string prop in propertyNames)
			{
				if (mat.HasProperty(prop))
					return mat.GetTextureOffset(prop);
			}
			return fallback;
		}
	}
}