using RTS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {
    public GUISkin resourceSkin;
    public GUISkin ordersSkin;
    public GUISkin selectBoxSkin;
    public GUISkin mouseCursorSkin;
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;
    public Texture2D[] resources;
    public Texture2D buttonHover, buttonClick;
    public Texture2D buildFrame, buildMask;

    // Use this for initialization
    private void Start() {
        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin);
        SetCursorState(CursorState.Select);

        resourceImages = new Dictionary<ResourceType, Texture2D>();
        resourceValues = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();

        foreach (Texture2D resource in resources) {
            ResourceType resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), resource.name);

            resourceImages.Add(resourceType, resource);
            resourceValues.Add(resourceType, 0);
            resourceLimits.Add(resourceType, 0);
        }

        buildAreaHeight = Screen.height - resourceBarHeight - selectionNameHeight - 2 * buttonSpacing;
    }

    private void OnGUI() {
        if (player && player.Human) {
            DrawOrdersBar();
            DrawResourceBar();
            DrawMouseCursor();
        }
    }

    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

    public bool MouseInBounds() {
        // In bounds of screen? That makes no sense. This is the HUD class. Should really be in bounds of the HUD!!
        // Screen coords start at bottom left corner
        Vector3 mousePos = Input.mousePosition;
        bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ordersBarWidth;
        bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - resourceBarHeight;
        return insideWidth && insideHeight;
    }

    public Rect GetPlayingArea() {
        return new Rect(0, resourceBarHeight, Screen.width - ordersBarWidth, Screen.height - resourceBarHeight);
    }

    public void SetCursorState(CursorState newState) {
        activeCursorState = newState;
        switch (newState) {
            case CursorState.Select:
                activeCursor = selectCursor;
                break;
            case CursorState.Attack:
                currentFrame = (int)Time.time % attackCursors.Length;
                activeCursor = attackCursors[currentFrame];
                break;
            case CursorState.Harvest:
                currentFrame = (int)Time.time % harvestCursors.Length;
                activeCursor = harvestCursors[currentFrame];
                break;
            case CursorState.Move:
                currentFrame = (int)Time.time % moveCursors.Length;
                activeCursor = moveCursors[currentFrame];
                break;
            case CursorState.PanLeft:
                activeCursor = leftCursor;
                break;
            case CursorState.PanRight:
                activeCursor = rightCursor;
                break;
            case CursorState.PanUp:
                activeCursor = upCursor;
                break;
            case CursorState.PanDown:
                activeCursor = downCursor;
                break;
            default:
                break;
        }
    }

    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(Screen.width - ordersBarWidth - buildImageWidth, resourceBarHeight, ordersBarWidth + buildImageWidth, Screen.height - resourceBarHeight));

        GUI.Box(new Rect(buildImageWidth + scrollBarWidth, 0, ordersBarWidth, Screen.height - resourceBarHeight), "");

        string selectionName = null;
        if (player.SelectedObject) {
            selectionName = player.SelectedObject.ObjectName;

            if (player.SelectedObject.IsOwnedBy(player)) {
                // Reset slider value if the selected object has changed
                if (lastSelection && lastSelection != player.SelectedObject)
                    sliderValue = 0f;

                DrawActions(player.SelectedObject.GetActions());
                lastSelection = player.SelectedObject;

                Building selectedBuilding = lastSelection.GetComponent<Building>();
                if (selectedBuilding) {
                    DrawBuildQueue(selectedBuilding.GetBuildQueueValues(), selectedBuilding.GetBuildPercentage());
                }
            }
        }

        if (selectionName != null)
            GUI.Label(new Rect(0, 10, ordersBarWidth, selectionNameHeight), selectionName);

        if (selectionName != "") {
            int leftPos = buildImageWidth + scrollBarWidth / 2;
            int topPos = buildAreaHeight + buttonSpacing;
            GUI.Label(new Rect(leftPos, topPos, ordersBarWidth, selectionNameHeight), selectionName);
        }

        GUI.EndGroup();
    }

    private void DrawResourceBar() {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, resourceBarHeight));

        GUI.Box(new Rect(0, 0, Screen.width, resourceBarHeight), "");
        int topPos = 4, iconLeft = 4, textLeft = 20;
        DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
        iconLeft += textWidth;
        textLeft += textWidth;
        DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);

        GUI.EndGroup();
    }

    private void DrawBuildQueue(IEnumerable<string> buildQ, float buildPercentage) {
        List<string> buildQueue = new List<string>(buildQ);

        for (int i = 0; i < buildQueue.Count; i++) {
            float topPos = i * buildImageHeight - (i + 1) * buildImagePadding;
            Rect buildPos = new Rect(buildImagePadding, topPos, buildImageWidth, buildImageHeight);
            GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
            GUI.DrawTexture(buildPos, buildFrame);
            topPos += buildImagePadding;

            float width = buildImageWidth - 2 * buildImagePadding;
            float height = buildImageHeight - 2 * buildImagePadding;

            if (i == 0) {
                // Shrink build mask on item being built to give an idea of progress
                topPos += height * buildPercentage;
                height *= (1 - buildPercentage);
            }

            GUI.DrawTexture(new Rect(2 * buildImagePadding, topPos, width, height), buildMask);
        }
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
        Texture2D icon = resourceImages[type];
        string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();

        GUI.DrawTexture(new Rect(iconLeft, topPos, iconWidth, iconHeight), icon);
        GUI.Label(new Rect(textLeft, topPos, textWidth, textHeight), text);
    }

    private void DrawMouseCursor() {
        bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;

        if (mouseOverHud) {
            Screen.showCursor = true;
        } else {
            Screen.showCursor = false;
            GUI.skin = mouseCursorSkin;
            GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
            UpdateCursorAnimation();
            Rect cursorPosition = GetCursorDrawPosition();
            GUI.Label(cursorPosition, activeCursor);
            GUI.EndGroup();
        }
    }

    private void UpdateCursorAnimation() {
        if (activeCursorState == CursorState.Move) {
            currentFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentFrame];
        } else if (activeCursorState == CursorState.Attack) {
            currentFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentFrame];
        } else if (activeCursorState == CursorState.Harvest) {
            currentFrame = (int)Time.time % harvestCursors.Length;
            activeCursor = harvestCursors[currentFrame];
        }
    }

    private Rect GetCursorDrawPosition() {
        // Set base position for custom cursor image
        float leftPos = Input.mousePosition.x;
        float topPos = Screen.height - Input.mousePosition.y;

        // Adjust position based on type of cursor being shown
        if (activeCursorState == CursorState.PanRight)
            leftPos = Screen.width - activeCursor.width;
        else if (activeCursorState == CursorState.PanDown)
            topPos = Screen.height - activeCursor.height;
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }

        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    private void DrawActions(string[] actions) {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;
        int numActions = actions.Length;

        GUI.BeginGroup(new Rect(buildImageWidth, 0, ordersBarWidth, buildAreaHeight));

        if (numActions >= MaxNumRows(buildAreaHeight))
            DrawSlider(buildAreaHeight, numActions / 2.0f);

        for (int i = 0; i < numActions; i++) {
            int column = i % 2;
            int row = i / 2;
            Rect pos = GetButtonPos(row, column);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);

            if (action && GUI.Button(pos, action) && player.SelectedObject)
                player.SelectedObject.PerformAction(actions[i]);
        }

        GUI.EndGroup();
    }

    private int MaxNumRows(int areaHeight) {
        return areaHeight / buildImageHeight;
    }

    private Rect GetButtonPos(int row, int column) {
        int left = scrollBarWidth + column * buildImageWidth;
        float top = row * buildImageHeight - sliderValue * buildImageHeight;
        return new Rect(left, top, buildImageWidth, buildImageHeight);
    }

    private void DrawSlider(int groupHeight, float numRows) {
        sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0f, numRows - MaxNumRows(groupHeight));
    }

    private Rect GetScrollPos(int groupHeight) {
        return new Rect(buttonSpacing, buttonSpacing, scrollBarWidth, groupHeight - 2 * buttonSpacing);
    }

    private const int ordersBarWidth = 150;
    private const int resourceBarHeight = 40;
    private const int selectionNameHeight = 15;
    private const int iconWidth = 32;
    private const int iconHeight = 32;
    private const int textWidth = 128;
    private const int textHeight = 32;
    private const int buildImageWidth = 64;
    private const int buildImageHeight = 64;
    private const int buttonSpacing = 7;
    private const int scrollBarWidth = 22;
    private const int buildImagePadding = 8;
    private Dictionary<ResourceType, int> resourceValues;
    private Dictionary<ResourceType, int> resourceLimits;
    private Dictionary<ResourceType, Texture2D> resourceImages;
    private WorldObject lastSelection;
    private float sliderValue;
    private int buildAreaHeight = 0;

    private Player player;
    private CursorState activeCursorState;
    private int currentFrame = 0;
}
