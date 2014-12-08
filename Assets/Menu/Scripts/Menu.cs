using UnityEngine;
using System.Collections;
using RTS;

public class Menu : MonoBehaviour {
    public GUISkin skin;
    public Texture2D header;

    protected string[] buttons;

    protected virtual void Start() {
        SetButtons();
    }

    protected virtual void OnGUI() {
        DrawMenu();
    }

    protected virtual void DrawMenu() {
        GUI.skin = skin;
        float menuHeight = GetMenuHeight();

        float groupLeft = Screen.width / 2 - ResourceManager.MenuWidth / 2;
        float groupTop = Screen.height / 2 - menuHeight / 2;
        GUI.BeginGroup(new Rect(groupLeft, groupTop, ResourceManager.MenuWidth, menuHeight));

        GUI.Box(new Rect(0, 0, ResourceManager.MenuWidth, ResourceManager.PauseMenuHeight), "");
        GUI.DrawTexture(new Rect(ResourceManager.Padding, ResourceManager.Padding, ResourceManager.HeaderWidth, ResourceManager.HeaderHeight), header);

        if (buttons != null) {
            float leftPos = ResourceManager.MenuWidth / 2 - ResourceManager.ButtonWidth / 2;
            float topPos = 2 * ResourceManager.Padding + header.height;

            for (int i = 0; i < buttons.Length; i++) {
                if (i > 0)
                    topPos += ResourceManager.ButtonHeight + ResourceManager.Padding;

                if (GUI.Button(new Rect(leftPos, topPos, ResourceManager.ButtonWidth, ResourceManager.ButtonHeight), buttons[i])) {
                    HandleButton(buttons[i]);
                }
            }
        }

        GUI.EndGroup();
    }

    protected virtual void SetButtons() {
    }

    protected virtual void HandleButton(string text) {
    }

    protected virtual float GetMenuHeight() {
        float buttonHeight = 0f;
        float paddingHeight = 2 * ResourceManager.Padding;

        if (buttons != null) {
            buttonHeight = buttons.Length * ResourceManager.ButtonHeight;
            paddingHeight += buttons.Length * ResourceManager.Padding;
        }

        return ResourceManager.HeaderHeight + buttonHeight + paddingHeight;
    }

    protected void ExitGame() {
        Application.Quit();
    }
}
