using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
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
    public class DaisyWheel : Selectable, IPointerClickHandler, ISubmitHandler
    {
        /// <summary>
        /// Are items in clockwise order
        /// </summary>
        [SerializeField]
        private bool areItemsInClockwiseOrder = true;

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
        private RectTransform cursorRectangleTransform = default;

        /// <summary>
        /// Content rectangle transform
        /// </summary>
        [SerializeField]
        private RectTransform contentRectangleTransform = default;

        /// <summary>
        /// Pivot rectangle transform
        /// </summary>
        [SerializeField]
        private RectTransform pivotRectangleTransform = default;

        /// <summary>
        /// On selected event
        /// </summary>
        [SerializeField]
        private UnityEvent<int> onSelected = default;

        /// <summary>
        /// On deselected event
        /// </summary>
        [SerializeField]
        private UnityEvent<int> onDeselected = default;

        /// <summary>
        /// On clicked event
        /// </summary>
        [SerializeField]
        private UnityEvent<int> onClicked = default;

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
        /// Is clicking
        /// </summary>
        private bool isClicking = false;

        /// <summary>
        /// Item graphics
        /// </summary>
        private List<Graphic> itemGraphics = new List<Graphic>();

        /// <summary>
        /// Are items in clockwise order
        /// </summary>
        public bool AreItemsInClockwiseOrder
        {
            get => areItemsInClockwiseOrder;
            set => areItemsInClockwiseOrder = value;
        }

        /// <summary>
        /// Item spacing
        /// </summary>
        public float ItemSpacing
        {
            get => itemSpacing;
            set => itemSpacing = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        /// Item content height
        /// </summary>
        public float ItemContentHeight
        {
            get => itemContentHeight;
            set => itemContentHeight = Mathf.Max(value, 0.0f);
        }

        /// <summary>
        /// Item content rotation mode
        /// </summary>
        public EDaisyWheelItemContentRotationMode ItemContentRotationMode
        {
            get => itemContentRotationMode;
            set => itemContentRotationMode = value;
        }

        /// <summary>
        /// Input dead zone
        /// </summary>
        public float InputDeadZone
        {
            get => inputDeadZone;
            set => inputDeadZone = Mathf.Clamp(value, 0.0f, 1.0f);
        }

        /// <summary>
        /// Cursor rectangle transform
        /// </summary>
        public RectTransform CursorRectangleTransform
        {
            get => cursorRectangleTransform;
            set => cursorRectangleTransform = value;
        }

        /// <summary>
        /// Content rectangle transform
        /// </summary>
        public RectTransform ContentRectangleTransform
        {
            get => contentRectangleTransform;
            set => contentRectangleTransform = value;
        }

        /// <summary>
        /// Pivot rectangle transform
        /// </summary>
        public RectTransform PivotRectangleTransform
        {
            get => pivotRectangleTransform;
            set => pivotRectangleTransform = value;
        }

        /// <summary>
        /// Target graphic
        /// </summary>
        public Graphic TargetGraphic
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
        /// On selected event
        /// </summary>
        public event SelectedDelegate OnSelected;

        /// <summary>
        /// On deselected event
        /// </summary>
        public event DeselectedDelegate OnDeselected;

        /// <summary>
        /// On clicked event
        /// </summary>
        public event ClickedDelegate OnClicked;

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
            if (contentRectangleTransform != null)
            {
                List<RectTransform> child_rect_transforms = new List<RectTransform>();
                foreach (RectTransform child in contentRectangleTransform)
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
                                child_rect_transform.localRotation = Quaternion.Euler(0.0f, 0.0f, (itemRotation * (areItemsInClockwiseOrder ? (child_rect_transforms.Count - (i)) : i)) + item_rotation_offset);
                            }
                        }
                        if (cursorRectangleTransform != null)
                        {
                            if (cursorImage == null)
                            {
                                cursorImage = cursorRectangleTransform.GetComponent<Image>();
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
                if (pivotRectangleTransform != null)
                {
                    pivotRectangleTransform.localScale = Vector2.one * inputDeadZone;
                }
            }
        }

        /// <summary>
        /// On validate
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            itemSpacing = Mathf.Clamp(itemSpacing, 0.0f, 1.0f);
            itemContentHeight = Mathf.Max(itemContentHeight, 0.0f);
            inputDeadZone = Mathf.Clamp(inputDeadZone, 0.0f, 1.0f);
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
            //bool touch_selected = false;
            ResizeItems();
            if ((numItems > 0U) && (canvasRectTransform != null) && (contentRectangleTransform != null))
            {
                if ((currentSelectionState == SelectionState.Pressed) || (currentSelectionState == SelectionState.Selected))
                {
                    Vector2 canvas_size = canvasRectTransform.rect.size;
                    Vector2 content_size = contentRectangleTransform.rect.size;
                    if ((canvas_size.sqrMagnitude > float.Epsilon) && (content_size.sqrMagnitude > float.Epsilon))
                    {
                        //int touch_count = 0;
                        Vector2 selection_direction = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
                        bool is_mouse_present = Mouse.current != null;
                        Vector2 horizontal_vertical = (Gamepad.current == null) ? Vector2.zero : UnityEngine.InputSystem.Gamepad.current.leftStick.ReadValue();
#else
                        bool is_mouse_present = Input.mousePresent;
                        Vector2 horizontal_vertical = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
                        if (is_mouse_present && (horizontal_vertical.sqrMagnitude <= (inputDeadZone * inputDeadZone)))
                        {
#if ENABLE_INPUT_SYSTEM
                            Vector2 mouse_position = Mouse.current.position.ReadValue();
#else
                            Vector2 mouse_position = Input.mousePosition;
#endif
                            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mouse_position, null, out selection_direction);
                            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, new Vector2(contentRectangleTransform.position.x, contentRectangleTransform.position.y), null, out Vector2 content_position);
                            selection_direction = new Vector2((selection_direction.x - content_position.x) / content_size.x, (selection_direction.y - content_position.y) / content_size.y);
                        }
                        else
                        {
                            selection_direction = horizontal_vertical;
                        }
                        if (selection_direction.sqrMagnitude > (inputDeadZone * inputDeadZone))
                        {
                            selection_direction.Normalize();
                            not_selected = false;
                            float cursor_rotation_offset = GetImageRotationOffset(cursorImage);
                            float selection_angle = Loop(Vector2.SignedAngle(Vector2.up, selection_direction) + cursor_rotation_offset, 0.0f, 360.0f);
                            cursorRectangleTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, selection_angle - (cursor_rotation_offset * itemSpacing));
                            int selection_index = 0;
                            while (selection_angle >= itemRotation)
                            {
                                selection_angle -= itemRotation;
                                ++selection_index;
                            }
                            if (areItemsInClockwiseOrder)
                            {
                                selection_index = (int)((numItems - selection_index) % numItems);
                            }
                            if (selection_index != selectedIndex)
                            {
                                if (onDeselected != null)
                                {
                                    onDeselected.Invoke(selectedIndex);
                                }
                                OnDeselected?.Invoke(selectedIndex);
                                selectedIndex = selection_index;
                                if (onSelected != null)
                                {
                                    onSelected.Invoke(selectedIndex);
                                }
                                OnSelected?.Invoke(selectedIndex);
                            }
                        }
                    }
                }
            }
            if (not_selected)
            {
                if (selectedIndex >= 0)
                {
                    if (onDeselected != null)
                    {
                        onDeselected.Invoke(selectedIndex);
                    }
                    OnDeselected?.Invoke(selectedIndex);
                    selectedIndex = -1;
                }
            }
            else if (isClicking)
            {
                if (onClicked != null)
                {
                    onClicked.Invoke(selectedIndex);
                }
                OnClicked?.Invoke(selectedIndex);
            }
            TargetGraphic = ((selectedIndex < 0) || (selectedIndex >= itemGraphics.Count)) ? null : itemGraphics[selectedIndex];
            isClicking = false;
        }

        /// <summary>
        /// Clicks on daisy wheel 
        /// </summary>
        private void Click()
        {
            if (IsActive() && IsInteractable())
            {
                isClicking = true;
            }
        }

        /// <summary>
        /// On pointer click
        /// </summary>
        /// <param name="eventData">Pointer event data</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Click();
            }
        }

        /// <summary>
        /// On submit
        /// </summary>
        /// <param name="eventData">Event data</param>
        public void OnSubmit(BaseEventData eventData) => Click();
    }
}
