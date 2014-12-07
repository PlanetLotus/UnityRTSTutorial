using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class WorldObject : MonoBehaviour {
    public string ObjectName;
    public Texture2D BuildImage;
    public int Cost;
    public int SellValue;
    public int HitPoints;
    public int MaxHitPoints;

    public Bounds SelectionBounds { get { return selectionBounds; } }

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
            if (worldObject) {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();

                if (!resource || !resource.IsEmpty())
                    ChangeSelection(worldObject, controller);
            }
        }
    }

    public void CalculateBounds() {
        selectionBounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            selectionBounds.Encapsulate(r.bounds);
    }

    public void SetColliders(bool isEnabled) {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
            collider.enabled = isEnabled;
    }

    public void SetTransparentMaterial(Material material, bool storeExistingMaterial) {
        if (storeExistingMaterial)
            oldMaterials.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            if (storeExistingMaterial)
                oldMaterials.Add(renderer.material);
            renderer.material = material;
        }
    }

    public void RestoreMaterials() {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (oldMaterials.Count == renderers.Length) {
            for (int i = 0; i < renderers.Length; i++) {
                renderers[i].material = oldMaterials[i];
            }
        }
    }

    public void SetPlayingArea(Rect playingArea) {
        this.playingArea = playingArea;
    }

    public void SetPlayer() {
        player = transform.root.GetComponentInChildren<Player>();
    }

    protected virtual void Awake() {
        selectionBounds = ResourceManager.InvalidBounds;
        CalculateBounds();
    }

	protected virtual void Start() {
        SetPlayer();
	}
	
	protected virtual void Update() {
	
	}

    protected virtual void OnGUI() {
        if (currentlySelected)
            DrawSelection();
    }

    protected virtual void DrawSelectionBox(Rect selectBox) {
        GUI.Box(selectBox, "");
        CalculateCurrentHealth(0.35f, 0.65f);
        GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), "", healthStyle);
    }

    protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit) {
        healthPercentage = (float)HitPoints / (float)MaxHitPoints;

        if (healthPercentage > highSplit)
            healthStyle.normal.background = ResourceManager.HealthyTexture;
        else if (healthPercentage > lowSplit)
            healthStyle.normal.background = ResourceManager.DamagedTexture;
        else
            healthStyle.normal.background = ResourceManager.CriticalTexture;
    }

    protected void DrawHealthBar(Rect selectBox, string label) {
        healthStyle.padding.top = -20;
        healthStyle.fontStyle = FontStyle.Bold;
        GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
    }

    protected Player player;
    protected string[] actions = {};
    protected bool currentlySelected = false;
    protected Bounds selectionBounds;
    protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
    protected GUIStyle healthStyle = new GUIStyle();
    protected float healthPercentage = 1f;

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

    private List<Material> oldMaterials = new List<Material>();
}
