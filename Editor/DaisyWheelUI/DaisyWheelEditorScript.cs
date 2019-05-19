using DaisyWheelUI.Controllers;
using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Daisy wheel UI editor namespace
/// </summary>
namespace DaisyWheelUI.Editor
{
    [CustomEditor(typeof(DaisyWheel))]
    public class DaisyWheelEditorScript : UnityEditor.Editor
    {
        [MenuItem("GameObject/UI/Daisy Wheel")]
        public static void CreateDaisyWheel()
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
                GameObject canvas_asset = Resources.Load<GameObject>("DaisyWheelUI/Canvas");
                GameObject event_system_asset = Resources.Load<GameObject>("DaisyWheelUI/EventSystem");
                if ((canvas_asset != null) && (event_system_asset != null))
                {
                    GameObject go = Instantiate(canvas_asset);
                    if (go != null)
                    {
                        go.name = "Canvas";
                        canvas_rect_transform = go.GetComponent<RectTransform>();
                        if (canvas_rect_transform != null)
                        {
                            go = Instantiate(event_system_asset);
                            if (go != null)
                            {
                                go.name = "EventSystem";
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
                GameObject daisy_wheel_asset = Resources.Load<GameObject>("DaisyWheelUI/DaisyWheel");
                if (daisy_wheel_asset != null)
                {
                    GameObject daisy_wheel_game_object = Instantiate(daisy_wheel_asset);
                    if (daisy_wheel_game_object != null)
                    {
                        daisy_wheel_game_object.name = "Daisy Wheel";
                        RectTransform daisy_wheel_rect_transform = daisy_wheel_game_object.GetComponent<RectTransform>();
                        if (daisy_wheel_rect_transform != null)
                        {
                            daisy_wheel_rect_transform.SetParent(canvas_rect_transform, true);
                            daisy_wheel_rect_transform.anchoredPosition = Vector2.zero;
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
}
