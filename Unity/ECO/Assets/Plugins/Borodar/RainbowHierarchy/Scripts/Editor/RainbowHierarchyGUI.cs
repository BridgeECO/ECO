using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SiblingIndex = Borodar.RainbowHierarchy.HierarchyEditorUtility.SiblingIndex;

namespace Borodar.RainbowHierarchy
{
	[InitializeOnLoad]
	public static class RainbowHierarchyGUI
	{
		private const float DRAW_OFFSET = 28f;

		private static readonly Color ROW_SHADING_COLOR = new Color(0f, 0f, 0f, 0.03f);
		private static readonly List<GameObject> RECURSIVE_OBJECTS = new List<GameObject>();
		private static bool _multiSelection;

		//---------------------------------------------------------------------
		// Ctor
		//---------------------------------------------------------------------

		static RainbowHierarchyGUI()
		{
			EditorApplication.hierarchyChanged += HierarchyWindowChanged;
			EditorApplication.hierarchyWindowItemOnGUI += RainbowHierarchyItemOnGUI;
			HierarchyRulesetV2.OnRulesetChangeCallback += OnRulesetChange;
		}

		//---------------------------------------------------------------------
		// Delegates
		//---------------------------------------------------------------------

		private static void HierarchyWindowChanged()
		{
			HierarchyEditorUtility.UpdateSceneConfigVisibility(!HierarchyPreferences.HideConfig);
			RECURSIVE_OBJECTS.Clear();
		}

		private static void OnRulesetChange()
		{
			RECURSIVE_OBJECTS.Clear();
		}

		private static void RainbowHierarchyItemOnGUI(int instanceId, Rect selectionRect)
		{
			var gameObject = (GameObject)EditorUtility.InstanceIDToObject(instanceId);
			if (gameObject == null) return;

			DrawRowShading(selectionRect);
			DrawFoldouts(gameObject, selectionRect);
			ReplaceHierarchyTextures(instanceId, gameObject, selectionRect);
			DrawEditIcon(gameObject, selectionRect);
		}

		//---------------------------------------------------------------------
		// GUI
		//---------------------------------------------------------------------

		private static void DrawRowShading(Rect selectionRect)
		{
			if (!HierarchyPreferences.DrawRowShading) return;

			var isOdd = Mathf.FloorToInt(((selectionRect.y - 4) / 16) % 2) != 0;
			if (isOdd) return;

			var foldoutRect = new Rect(selectionRect);
			foldoutRect.width += selectionRect.x + 16f;
			foldoutRect.height += 1f;

			foldoutRect.x = DRAW_OFFSET;

			// Background
			EditorGUI.DrawRect(foldoutRect, ROW_SHADING_COLOR);
			// Top line
			foldoutRect.height = 1f;
			EditorGUI.DrawRect(foldoutRect, ROW_SHADING_COLOR);
			// Bottom line
			foldoutRect.y += 16f;
			EditorGUI.DrawRect(foldoutRect, ROW_SHADING_COLOR);
		}

		private static void DrawFoldouts(GameObject gameObject, Rect selectionRect)
		{
			if (!HierarchyPreferences.ShowHierarchyTree || IsShowInSearch(selectionRect)) return;

			const float textureWidth = 128f;

			var fx = Mathf.Max(DRAW_OFFSET, selectionRect.x - textureWidth - 16f);
			var fw = Mathf.Min(textureWidth, selectionRect.x - 16f - DRAW_OFFSET);
			var foldoutRect = new Rect(selectionRect) {width = fw, x = fx};

			var tw = foldoutRect.width / textureWidth;
			var texCoords = new Rect(1 - tw,0,tw,1f);

			GUI.DrawTextureWithTexCoords(foldoutRect, HierarchyEditorUtility.GetFoldoutLevelsIcon(), texCoords);

			var transform = gameObject.transform;
			if (transform.childCount > 0) return;

			var index = GetSiblingIndex(transform);
			foldoutRect.width = 16f;

			foldoutRect.x = selectionRect.x - 16f;

			GUI.DrawTexture(foldoutRect, HierarchyEditorUtility.GetFoldoutIcon(index));
		}

		private static void ReplaceHierarchyTextures(int instanceId, GameObject gameObject, Rect selectionRect)
		{
			if (EditorSceneManager.IsPreviewSceneObject(gameObject)) return;

			var currentScene = gameObject.scene;
			var ruleset = HierarchyRulesetV2.GetRulesetByScene(currentScene);
			if (ruleset == null || ruleset.Rules.Count == 0) return;

			var normalItem = ruleset.GetRule(gameObject);
			var recursiveItem = GetRecursiveItemByObject(gameObject, ruleset);

			if (normalItem != null)
			{
				UpdateRecursiveObjectsList(ruleset, normalItem, gameObject);

				if (recursiveItem != null)
				{
					DrawItemWithBiggerPriority(instanceId, selectionRect, ruleset, normalItem, recursiveItem);
				}
				else
				{
					DrawNormalItem(instanceId, selectionRect, normalItem);
				}
			}
			else if (recursiveItem != null)
			{
				DrawRecursiveItem(instanceId, selectionRect, recursiveItem);
			}
		}

		private static void DrawEditIcon(GameObject gameObject, Rect selectionRect)
		{
			if (!HierarchyPreferences.IsEditModifierPressed(Event.current))
			{
				_multiSelection = false;
				return;
			}

			if (EditorSceneManager.IsPreviewSceneObject(gameObject)) return;

			var isMouseOver = selectionRect.Contains(Event.current.mousePosition);
			var isSelected = IsSelected(gameObject);
			_multiSelection = isSelected ? isMouseOver || _multiSelection : !isMouseOver && _multiSelection;

			// if mouse is not over current object icon or selected group
			if (!isMouseOver && (!isSelected || !_multiSelection)) return;

			var editIcon = HierarchyEditorUtility.GetEditIcon();
			DrawCustomIcon(editIcon, selectionRect);

			if (GUI.Button(selectionRect, GUIContent.none, GUIStyle.none))
			{
				ShowPopupWindow(selectionRect, gameObject);
			}

			EditorApplication.RepaintHierarchyWindow();
		}

		//---------------------------------------------------------------------
		// Helpers
		//---------------------------------------------------------------------

		private static void DrawItemWithBiggerPriority(int instanceId, Rect selectionRect, HierarchyRulesetV2 conf, HierarchyRule normalRule, HierarchyRule recursiveRule)
		{
			if (recursiveRule.Priority > normalRule.Priority)
			{
				DrawRecursiveItem(instanceId, selectionRect, recursiveRule);
			}
			else if (recursiveRule.Priority == normalRule.Priority)
			{
				DrawItemWithGreaterOrdinal(instanceId, selectionRect, conf, normalRule, recursiveRule);
			}
			else
			{
				DrawNormalItem(instanceId, selectionRect, normalRule);
			}
		}

		private static void DrawItemWithGreaterOrdinal(int instanceId, Rect selectionRect, HierarchyRulesetV2 conf, HierarchyRule normalRule, HierarchyRule recursiveRule)
		{
			if (recursiveRule.Ordinal > normalRule.Ordinal)
			{
				DrawRecursiveItem(instanceId, selectionRect, recursiveRule);
			}
			else
			{
				DrawNormalItem(instanceId, selectionRect, normalRule);
			}
		}

		private static void DrawNormalItem(int instanceId, Rect selectionRect, HierarchyRule normalRule)
		{
			ReplaceIcon(instanceId, normalRule);
			DrawCustomBackground(normalRule, selectionRect);
		}

		private static void DrawRecursiveItem(int instanceId, Rect selectionRect, HierarchyRule recursiveRule)
		{
			if (recursiveRule.IsIconRecursive) ReplaceIcon(instanceId, recursiveRule);
			if (recursiveRule.IsBackgroundRecursive) DrawCustomBackground(recursiveRule, selectionRect);
		}

		private static void ReplaceIcon(int instanceId, HierarchyRule rule)
		{
			if (rule == null || !rule.HasIcon()) return;
			var icon = rule.HasCustomIcon()
				? rule.IconTexture
				: HierarchyIconsStorage.GetIcon(rule.IconType);
			HierarchyWindowAdapter.ApplyIconByInstanceId(instanceId, icon);
		}

		private static void DrawCustomIcon(Texture icon, Rect selectionRect)
		{
			var iconRect = new Rect(selectionRect) {width = 16f};
			GUI.DrawTexture(iconRect, icon);
		}

		private static void DrawCustomBackground(HierarchyRule rule, Rect selectionRect)
		{
			if (rule == null || !rule.HasBackground()) return;

			selectionRect.x += 17f;

			var background = rule.HasCustomBackground()
				? rule.BackgroundTexture
				: HierarchyBackgroundsStorage.GetBackground(rule.BackgroundType);
			GUI.DrawTexture(selectionRect, background);
		}
		
		private static void ShowPopupWindow(Rect selectionRect, GameObject currentObject)
		{
			var window = HierarchyPopupWindow.GetDraggableWindow();
			var position = GUIUtility.GUIToScreenPoint(selectionRect.position + new Vector2(0, selectionRect.height + 2));

			if (_multiSelection)
			{
				var gameObjects = Selection.gameObjects.ToList();
				var index = gameObjects.IndexOf(currentObject);
				window.ShowWithParams(position, gameObjects, index);
			}
			else
			{
				window.ShowWithParams(position, new List<GameObject> {currentObject}, 0);
			}
		}

		private static HierarchyRule GetRecursiveItemByObject(GameObject gameObject, HierarchyRulesetV2 ruleset)
		{
			for (var i = RECURSIVE_OBJECTS.Count - 1; i >= 0; i--)
			{
				if (RECURSIVE_OBJECTS[i] == null)
				{
					RECURSIVE_OBJECTS.RemoveAt(i);
					continue;
				}

				if (!gameObject.transform.IsChildOf(RECURSIVE_OBJECTS[i].transform))
				{
					continue;
				}

				var parentRule = ruleset.GetRule(RECURSIVE_OBJECTS[i]);
				if (parentRule == null)
				{
					RECURSIVE_OBJECTS.RemoveAt(i);
					continue;
				}

				if (parentRule.IsRecursive())
				{
					return parentRule;
				}
			}

			return null;
		}
		
		[SuppressMessage("ReSharper", "InvertIf")]
		private static void UpdateRecursiveObjectsList(HierarchyRulesetV2 conf, HierarchyRule currentRule, GameObject currentObject)
		{
			if (currentRule.IsRecursive())
			{
				if (!RECURSIVE_OBJECTS.Contains(currentObject))
				{
					for (var index = 0; index < RECURSIVE_OBJECTS.Count; index++)
					{
						var listObject = RECURSIVE_OBJECTS[index];
						if (listObject == null) continue;

						var listItem = conf.GetRule(listObject);
						if (listItem == null) continue;

						if (currentRule.Priority < listItem.Priority)
						{
							RECURSIVE_OBJECTS.Insert(index, currentObject);
							return;
						}

						if (currentRule.Priority == listItem.Priority)
						{
							var currentOrdinal = conf.Rules.IndexOf(currentRule);
							var listOrdinal = conf.Rules.IndexOf(listItem);

							if (currentOrdinal < listOrdinal)
							{
								RECURSIVE_OBJECTS.Insert(index, currentObject);
								return;
							}
						}
					}

					RECURSIVE_OBJECTS.Add(currentObject);
				}
			}
			else
			{
				RECURSIVE_OBJECTS.Remove(currentObject);
			}
		}

		private static bool IsSelected(GameObject gameObject)
		{
			return Selection.gameObjects.Contains(gameObject);
		}

		private static bool IsShowInSearch(Rect iconRect)
		{
			return iconRect.x == 16f;
		}

		[SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
		private static SiblingIndex GetSiblingIndex(Transform transform)
		{
			var parent = transform.parent;
			var parentChildCount = (parent != null) ? parent.childCount : transform.gameObject.scene.rootCount;
			var siblingIndex = transform.GetSiblingIndex();

			if (siblingIndex == 0) return SiblingIndex.First;
			if (siblingIndex == parentChildCount - 1) return SiblingIndex.Last;

			return SiblingIndex.Middle;
		}
	}
}