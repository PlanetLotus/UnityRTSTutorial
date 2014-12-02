﻿using UnityEngine;
using System.Collections;
using RTS;

public class HUD : MonoBehaviour {
    public GUISkin resourceSkin;
    public GUISkin ordersSkin;
    public GUISkin selectBoxSkin;
    public GUISkin mouseCursorSkin;
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;

	// Use this for initialization
	void Start() {
        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin);
        SetCursorState(CursorState.Select);
	}
	
	void OnGUI() {
	    if (player && player.Human) {
            DrawOrdersBar();
            DrawResourceBar();
            DrawMouseCursor();
        }
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
        switch(newState) {
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
        GUI.BeginGroup(new Rect(Screen.width - ordersBarWidth, resourceBarHeight, ordersBarWidth, Screen.height - resourceBarHeight));

        GUI.Box(new Rect(0, 0, ordersBarWidth, Screen.height - resourceBarHeight), "");

        string selectionName = null;
        if (player.SelectedObject)
            selectionName = player.SelectedObject.ObjectName;

        if (selectionName != null)
            GUI.Label(new Rect(0, 10, ordersBarWidth, selectionNameHeight), selectionName);

        GUI.EndGroup();
    }

    private void DrawResourceBar() {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, resourceBarHeight));
        GUI.Box(new Rect(0, 0, Screen.width, resourceBarHeight), "");
        GUI.EndGroup();
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
        } else if(activeCursorState == CursorState.Attack) {
            currentFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentFrame];
        } else if(activeCursorState == CursorState.Harvest) {
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

    private const int ordersBarWidth = 150;
    private const int resourceBarHeight = 40;
    private const int selectionNameHeight = 15;

    private Player player;
    private CursorState activeCursorState;
    private int currentFrame = 0;
}
