using UnityEngine;
using System.Collections;
using RTS;

public class WorldObject : MonoBehaviour {
    public string ObjectName;
    public Texture2D BuildImage;
    public int Cost;
    public int SellValue;
    public int HitPoints;
    public int MaxHitPoints;

    public string[] GetActions() {
        return actions;
    }

    public virtual void PerformAction(string action) {
    }

    public bool IsOwnedBy(Player owner) {
        return player && player.Equals(owner);
    }

    public virtual void SetSelection(bool selected, Rect playingArea) {
        currentlySelected = selected;
        if (selected)
            this.playingArea = playingArea;
    }

    public virtual void SetHoverState(GameObject hoverObject) {
        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected) {
            if (hoverObject.name != "Ground")
                player.Hud.SetCursorState(CursorState.Select);
        }
    }
    
    public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        // Only handle input if currently selected
        if (currentlySelected && hitObject && hitObject.name != "Ground") {
            WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();

            // Another selectable object was clicked
            if (worldObject)
                ChangeSelection(worldObject, controller);
        }
    }

    public void CalculateBounds() {
        selectionBounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            selectionBounds.Encapsulate(r.bounds);
    }

    protected virtual void Awake() {
        selectionBounds = ResourceManager.InvalidBounds;
        CalculateBounds();
    }

	// Use this for initialization
	protected virtual void Start() {
        player = transform.root.GetComponentInChildren<Player>();
	}
	
	// Update is called once per frame
	protected virtual void Update() {
	
	}

    protected virtual void OnGUI() {
        if (currentlySelected)
            DrawSelection();
    }

    protected virtual void DrawSelectionBox(Rect selectBox) {
        GUI.Box(selectBox, "");
    }

    protected Player player;
    protected string[] actions = {};
    protected bool currentlySelected = false;
    protected Bounds selectionBounds;
    protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

    private void ChangeSelection(WorldObject worldObject, Player controller) {
        SetSelection(false, playingArea);
        if (controller.SelectedObject)
            controller.SelectedObject.SetSelection(false, playingArea);

        controller.SelectedObject = worldObject;
        worldObject.SetSelection(true, controller.Hud.GetPlayingArea());
    }

    private void DrawSelection() {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);

        // Draw selection box around selected object, within bounds of playing area
        GUI.BeginGroup(playingArea);
        DrawSelectionBox(selectBox);
        GUI.EndGroup();
    }
}
