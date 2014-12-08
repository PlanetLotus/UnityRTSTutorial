using UnityEngine;
using RTS;

public class PauseMenu : Menu {
    protected override void SetButtons() {
        buttons = new string[] { "Resume", "Exit Game" };
    }

    protected override void HandleButton(string text) {
        switch (text) {
            case "Resume": Resume(); break;
            case "Exit Game": ReturnToMainMenu(); break;
            default: break;
        }
    }

    protected override void Start() {
        base.Start();
        player = transform.root.GetComponent<Player>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
            Resume();
    }

    private void Resume() {
        Time.timeScale = 1f;
        GetComponent<PauseMenu>().enabled = false;
        if (player)
            player.GetComponent<UserInput>().enabled = true;
        Screen.showCursor = false;
        ResourceManager.MenuOpen = false;
    }

    private void ReturnToMainMenu() {
        Application.LoadLevel("MainMenu");
        Screen.showCursor = true;
    }

    private Player player;
}
