using DaisyWheelUI.Controllers;
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Daisy wheel UI editor namespace
/// </summary>
namespace DaisyWheelUI.Editor
{
    /// <summary>
    /// Daisy wheel editor script class
    /// </summary>
    [CustomEditor(typeof(DaisyWheel))]
    public class DaisyWheelEditorScript : UnityEditor.Editor
    {
        /// <summary>
        /// Creates a new daisy wheel
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        private static void CreateDaisyWheel(string resourceName)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }
            DaisyWheelAssetsObjectScript assets = Resources.Load<DaisyWheelAssetsObjectScript>(resourceName);
            if (assets != null)
            {
                RectTransform canvas_rect_transform = null;
                GameObject[] game_objects = Selection.gameObjects;
                if (game_objects != null)
                {
                    foreach (GameObject game_object in game_objects)
                    {
                        if (game_object != null)
                        {
                            Canvas c = game_object.GetComponent<Canvas>();
                            if (c != null)
                            {
                                canvas_rect_transform = game_object.GetComponent<RectTransform>();
                                if (canvas_rect_transform != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                if (canvas_rect_transform == null)
                {
                    if ((assets.CanvasAsset != null) && (assets.EventSystemAsset != null))
                    {
                        GameObject go = Instantiate(assets.CanvasAsset);
                        if (go != null)
                        {
                            go.name = "Canvas";
                            canvas_rect_transform = go.GetComponent<RectTransform>();
                            if (canvas_rect_transform != null)
                            {
                                Undo.RegisterCreatedObjectUndo(go, "Canvas");
                                go = Instantiate(assets.EventSystemAsset);
                                if (go != null)
                                {
                                    go.name = "EventSystem";
                                    Undo.RegisterCreatedObjectUndo(go, "EventSystem");
                                }
                                else
                                {
                                    DestroyImmediate(canvas_rect_transform.gameObject);
                                    canvas_rect_transform = null;
                                }
                            }
                            else
                            {
                                DestroyImmediate(go);
                            }
                        }
                        go.AddComponent<Canvas>();
                    }
                }
                if (canvas_rect_transform != null)
                {
                    if (assets.DaisyWheelAsset != null)
                    {
                        GameObject daisy_wheel_game_object = Instantiate(assets.DaisyWheelAsset);
                        if (daisy_wheel_game_object != null)
                        {
                            daisy_wheel_game_object.name = "Daisy Wheel";
                            RectTransform daisy_wheel_rect_transform = daisy_wheel_game_object.GetComponent<RectTransform>();
                            if (daisy_wheel_rect_transform != null)
                            {
                                daisy_wheel_rect_transform.SetParent(canvas_rect_transform, true);
                                daisy_wheel_rect_transform.anchoredPosition = Vector2.zero;
                                daisy_wheel_rect_transform.localScale = Vector3.one;
                                Undo.RegisterCreatedObjectUndo(daisy_wheel_game_object, "Daisy Wheel");
                            }
                            else
                            {
                                DestroyImmediate(daisy_wheel_game_object);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new daisy wheel
        /// </summary>
#if ENABLE_INPUT_SYSTEM
        [MenuItem("GameObject/UI/Daisy Wheel")]
        public static void CreateDaisyWheel() => CreateDaisyWheel("DaisyWheelAssets");
#endif

        /// <summary>
        /// Creates a new daisy wheel
        /// </summary>
#if ENABLE_INPUT_SYSTEM
        [MenuItem("GameObject/UI/Daisy Wheel (legacy)")]
#else
        [MenuItem("GameObject/UI/Daisy Wheel")]
#endif
        public static void CreateLegacyDaisyWheel() => CreateDaisyWheel("LegacyDaisyWheelAssets");
    }
}
