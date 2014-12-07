using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine;

public class Building : WorldObject {
    public float maxBuildProgress;
    public Texture2D rallyPointImage;
    public Texture2D sellImage;
    protected Queue<string> buildQueue;
    protected Vector3 rallyPoint;

    public IEnumerable<string> GetBuildQueueValues() {
        return buildQueue;
    }

    public float GetBuildPercentage() {
        return currentBuildProgress / maxBuildProgress;
    }

    public void Sell() {
        if (player)
            player.AddResource(ResourceType.Money, SellValue);

        if (currentlySelected)
            SetSelection(false, playingArea);

        Destroy(this.gameObject);
    }

    public override void SetSelection(bool selected, Rect playingArea) {
        base.SetSelection(selected, playingArea);

        if (player) {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();

            if (selected) {
                if (flag && player.Human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition) {
                    flag.transform.localPosition = rallyPoint;
                    flag.transform.forward = transform.forward;
                    flag.Enable();
                }
            } else {
                if (flag && player.Human)
                    flag.Disable();
            }
        }
    }

    public bool HasSpawnPoint() {
        return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
    }

    public override void SetHoverState(GameObject hoverObject) {
        base.SetHoverState(hoverObject);

        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected && hoverObject.name == "Ground" && player.Hud.GetPreviousCursorState() == CursorState.RallyPoint)
            player.Hud.SetCursorState(CursorState.RallyPoint);
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        base.MouseClick(hitObject, hitPoint, controller);

        // Only handle input if owned by a human player and currently selected
        if (player && player.Human && currentlySelected && hitObject.name == "Ground") {
            if ((player.Hud.GetCursorState() == CursorState.RallyPoint || player.Hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition) {
                SetRallyPoint(hitPoint);
            }
        }
    }

    public void SetRallyPoint(Vector3 position) {
        rallyPoint = position;

        if (player && player.Human && currentlySelected) {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
            if (flag)
                flag.transform.localPosition = rallyPoint;
        }
    }

    public void StartConstruction() {
        CalculateBounds();
        needsBuilding = true;
        HitPoints = 0;
    }

    public bool UnderConstruction() {
        return needsBuilding;
    }

    public void Construct(int amount) {
        HitPoints += amount;
        if (HitPoints >= MaxHitPoints) {
            HitPoints = MaxHitPoints;
            needsBuilding = false;
            RestoreMaterials();
            SetTeamColor();
        }
    }
    
    protected override void Awake() {
        base.Awake();

        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0f, spawnZ);
        rallyPoint = spawnPoint;
    }
    
    protected override void Start () {
        base.Start();
    }
    
    protected override void Update () {
        base.Update();

        ProcessBuildQueue();
    }
    
    protected override void OnGUI() {
        base.OnGUI();

        if (needsBuilding)
            DrawBuildProgress();
    }

    protected void CreateUnit(string unitName) {
        buildQueue.Enqueue(unitName);
    }

    protected void ProcessBuildQueue() {
        if (buildQueue.Count == 0) return;

        currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
        if (currentBuildProgress > maxBuildProgress && player) {
            player.AddUnit(buildQueue.Dequeue(), spawnPoint, transform.rotation, rallyPoint, this);
            currentBuildProgress = 0f;
        }
    }

    private void DrawBuildProgress() {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);

        // Draw selection box around currently selected object
        GUI.BeginGroup(playingArea);
        CalculateCurrentHealth(0.5f, 0.99f);
        DrawHealthBar(selectBox, "Building ...");
        GUI.EndGroup();
    }

    private float currentBuildProgress = 0f;
    private Vector3 spawnPoint;
    private bool needsBuilding = false;
}
