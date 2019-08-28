using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Daisy wheel UI controllers namespace
/// </summary>
namespace DaisyWheelUI.Controllers
{
    /// <summary>
    /// Daisy wheel UI controller script class
    /// </summary>
    [ExecuteInEditMode]
    public class DaisyWheel : Selectable
    {
        /// <summary>
        /// Are items clockwise
        /// </summary>
        [SerializeField]
        private bool clockwise = true;

        /// <summary>
        /// Item spacing
        /// </summary>
        [SerializeField, Range(0.0f, 1.0f)]
        private float itemSpacing = default;

        /// <summary>
        /// Item content height
        /// </summary>
        [SerializeField]
        private float itemContentHeight = default;

        /// <summary>
        /// Item content rotation mode
        /// </summary>
        [SerializeField]
        private EDaisyWheelItemContentRotationMode itemContentRotationMode = EDaisyWheelItemContentRotationMode.FixedToView;

        /// <summary>
        /// Input dead zone
        /// </summary>
        [SerializeField, Range(0.0f, 1.0f)]
        private float inputDeadZone = 0.0625f;

        /// <summary>
        /// Cursor rectangle transform
        /// </summary>
        [SerializeField]
        private RectTransform cursorRectTransform = default;

        /// <summary>
        /// Content rectangle transform
        /// </summary>
        [SerializeField]
        private RectTransform contentRectTransform = default;

        /// <summary>
        /// Pivot rectangle transform
        /// </summary>
        [SerializeField]
        private RectTransform pivotRectTransform = default;

        /// <summary>
        /// On select event
        /// </summary>
        [SerializeField]
        private UnityEvent onSelect = default;

        /// <summary>
        /// On deselect event
        /// </summary>
        [SerializeField]
        private UnityEvent onDeselect = default;

        /// <summary>
        /// On click event
        /// </summary>
        [SerializeField]
        private UnityEvent onClick = default;

        /// <summary>
        /// Tau
        /// </summary>
        private const float tau = 2.0f * Mathf.PI;

        /// <summary>
        /// Cursor image
        /// </summary>
        private Image cursorImage;

        /// <summary>
        /// Canvas rectangle transform
        /// </summary>
        private RectTransform canvasRectTransform;

        /// <summary>
        /// Number of items
        /// </summary>
        private uint numItems;

        /// <summary>
        /// Item rotation
        /// </summary>
        private float itemRotation;

        /// <summary>
        /// Selected index
        /// </summary>
        private int selectedIndex = -1;

        /// <summary>
        /// Item graphics
        /// </summary>
        private List<Graphic> itemGraphics = new List<Graphic>();

        /// <summary>
        /// Target graphic
        /// </summary>
        private Graphic TargetGraphic
        {
            get => targetGraphic;
            set
            {
                if (targetGraphic != value)
                {
                    DoStateTransition((IsActive() && !IsInteractable()) ? SelectionState.Disabled : SelectionState.Normal, false);
                    targetGraphic = value;
                }
            }
        }

        /// <summary>
        /// Loop value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="minimum">Minimum</param>
        /// <param name="maximum">Maximum</param>
        /// <returns>Looped value</returns>
        private static float Loop(float value, float minimum, float maximum)
        {
            float ret = value;
            float delta = maximum - minimum;
            if (delta > float.Epsilon)
            {
                while (ret >= maximum)
                {
                    ret -= delta;
                }
                while (ret < minimum)
                {
                    ret += delta;
                }
            }
            else
            {
                ret = minimum;
            }
            return ret;
        }

        /// <summary>
        /// Get image rotation offset
        /// </summary>
        /// <param name="image">Image</param>
        /// <returns>Image rotation offset</returns>
        private float GetImageRotationOffset(Image image)
        {
            float ret = 0.0f;
            if (image != null)
            {
                if ((image.type == Image.Type.Filled) && (image.fillMethod == Image.FillMethod.Radial360))
                {
                    ret = itemRotation * (image.fillClockwise ? 0.5f : -0.5f);
                    switch (image.fillOrigin)
                    {
                        case 0: // Bottom
                            ret += (image.fillClockwise ? 180.0f : -180.0f);
                            break;
                        case 1: // Right
                            ret += (image.fillClockwise ? 270.0f : -270.0f);
                            break;
                        case 2: // Top
                            break;
                        case 3: // Left
                            ret += (image.fillClockwise ? 90.0f : -90.0f);
                            break;
                    }
                }
            }
            return Loop(ret, 0.0f, 360.0f);
        }

        /// <summary>
        /// Resize items
        /// </summary>
        private void ResizeItems()
        {
            numItems = 0U;
            itemRotation = 360.0f;
            itemGraphics.Clear();
            if (contentRectTransform != null)
            {
                List<RectTransform> child_rect_transforms = new List<RectTransform>();
                foreach (RectTransform child in contentRectTransform)
                {
                    child_rect_transforms.Add(child);
                }
                if (child_rect_transforms != null)
                {
                    numItems = (uint)(child_rect_transforms.Count);
                    if (child_rect_transforms.Count > 0)
                    {
                        float fill_amount = (1.0f - itemSpacing) / child_rect_transforms.Count;
                        itemRotation = 360.0f / child_rect_transforms.Count;
                        for (int i = 0; i < child_rect_transforms.Count; i++)
                        {
                            RectTransform child_rect_transform = child_rect_transforms[i];
                            if (child_rect_transform != null)
                            {
                                Image image = child_rect_transform.GetComponent<Image>();
                                float item_rotation_offset = 0.0f;
                                if (image != null)
                                {
                                    item_rotation_offset = GetImageRotationOffset(image);
                                    item_rotation_offset = Loop(item_rotation_offset - (item_rotation_offset * itemSpacing), 0.0f, 360.0f);
                                    if ((image.type == Image.Type.Filled) && (image.fillMethod == Image.FillMethod.Radial360))
                                    {
                                        image.fillAmount = fill_amount;
                                    }
                                    itemGraphics.Add(image);
                                }
                                float item_rotation_offset_rad = (item_rotation_offset * Mathf.PI) / 180.0f;
                                Vector2 up = new Vector2(Mathf.Sin(item_rotation_offset_rad), Mathf.Cos(item_rotation_offset_rad));
                                foreach (RectTransform child_rect_transform_child_rect_transform in child_rect_transform)
                                {
                                    if (child_rect_transform_child_rect_transform != null)
                                    {
                                        switch (itemContentRotationMode)
                                        {
                                            case EDaisyWheelItemContentRotationMode.Free:
                                                break;
                                            case EDaisyWheelItemContentRotationMode.FixedToView:
                                                child_rect_transform_child_rect_transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                                                break;
                                            case EDaisyWheelItemContentRotationMode.FixedToItem:
                                                child_rect_transform_child_rect_transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -item_rotation_offset);
                                                break;
                                                
                                        }
                                        child_rect_transform_child_rect_transform.localPosition = new Vector3(up.x * itemContentHeight, up.y * itemContentHeight, 0.0f);
                                    }
                                }
                                child_rect_transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (itemRotation * (clockwise ? (child_rect_transforms.Count - (i)) : i)) + item_rotation_offset);
                            }
                        }
                        if (cursorRectTransform != null)
                        {
                            if (cursorImage == null)
                            {
                                cursorImage = cursorRectTransform.GetComponent<Image>();
                            }
                            if (cursorImage != null)
                            {
                                if ((cursorImage.type == Image.Type.Filled) && (cursorImage.fillMethod == Image.FillMethod.Radial360))
                                {
                                    cursorImage.fillAmount = fill_amount;
                                }
                            }
                        }
                    }
                }
                child_rect_transforms.Clear();
                if (pivotRectTransform != null)
                {
                    pivotRectTransform.localScale = Vector2.one * inputDeadZone;
                }
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRectTransform = canvas.GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// Update
        /// </summary>
        private void Update()
        {
            bool not_selected = true;
            bool touch_selected = false;
            ResizeItems();
            if ((numItems > 0U) && (canvasRectTransform != null) && (contentRectTransform != null))
            {
                if ((currentSelectionState == SelectionState.Pressed) || (currentSelectionState == SelectionState.Selected))
                {
                    Vector2 canvas_size = canvasRectTransform.rect.size;
                    Vector2 content_size = contentRectTransform.rect.size;
                    if ((canvas_size.sqrMagnitude > float.Epsilon) && (content_size.sqrMagnitude > float.Epsilon))
                    {
                        int touch_count = 0;
                        Vector2 selection_direction = Vector2.zero;
                        if (Input.touchCount > 0)
                        {
                            Vector2 touch_positions_added = Vector2.zero;
                            for (int i = 0; i < Input.touchCount; i++)
                            {
                                Touch touch = Input.GetTouch(i);
                                if ((touch.phase != TouchPhase.Canceled) || (touch.phase != TouchPhase.Ended))
                                {
                                    touch_positions_added += touch.position;
                                    ++touch_count;
                                }
                                if (touch.phase == TouchPhase.Ended)
                                {
                                    touch_selected = true;
                                }
                            }
                            if (touch_count > 0U)
                            {
                                Vector2 content_position;
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, touch_positions_added / touch_count, null, out selection_direction);
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, new Vector2(contentRectTransform.position.x, contentRectTransform.position.y), null, out content_position);
                                selection_direction = new Vector2((selection_direction.x - content_position.x) / content_size.x, (selection_direction.y - content_position.y) / content_size.y);
                            }
                        }
                        if (touch_count <= 0)
                        {
                            float horizontal = Input.GetAxisRaw("Horizontal");
                            float vertical = Input.GetAxisRaw("Vertical");
                            if (Input.mousePresent && (((horizontal * horizontal) + (vertical * vertical)) <= (inputDeadZone * inputDeadZone)))
                            {
                                Vector2 content_position;
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, Input.mousePosition, null, out selection_direction);
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, new Vector2(contentRectTransform.position.x, contentRectTransform.position.y), null, out content_position);
                                selection_direction = new Vector2((selection_direction.x - content_position.x) / content_size.x, (selection_direction.y - content_position.y) / content_size.y);
                            }
                            else
                            {
                                selection_direction = new Vector2(horizontal, vertical);
                            }
                        }
                        if (selection_direction.sqrMagnitude > (inputDeadZone * inputDeadZone))
                        {
                            selection_direction.Normalize();
                            not_selected = false;
                            float cursor_rotation_offset = GetImageRotationOffset(cursorImage);
                            float selection_angle = Loop(Vector2.SignedAngle(Vector2.up, selection_direction) + cursor_rotation_offset, 0.0f, 360.0f);
                            cursorRectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, selection_angle - (cursor_rotation_offset * itemSpacing));
                            int selection_index = 0;
                            while (selection_angle >= itemRotation)
                            {
                                selection_angle -= itemRotation;
                                ++selection_index;
                            }
                            if (clockwise)
                            {
                                selection_index = (int)((numItems - selection_index) % numItems);
                            }
                            if (selection_index != selectedIndex)
                            {
                                selectedIndex = selection_index;
                                onDeselect?.Invoke();
                                onSelect?.Invoke();
                            }
                        }
                    }
                }
            }
            if (not_selected)
            {
                if (selectedIndex >= 0)
                {
                    selectedIndex = -1;
                    onDeselect?.Invoke();
                }
            }
            else if (touch_selected || Input.GetMouseButtonUp(0) || Input.GetButtonUp("Submit"))
            {
                onClick?.Invoke();
            }
            TargetGraphic = ((selectedIndex < 0) || (selectedIndex >= itemGraphics.Count)) ? null : itemGraphics[selectedIndex];
        }
    }
}
