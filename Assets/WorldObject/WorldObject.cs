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
    public float WeaponRange = 10f;
    public float WeaponRechargeTime = 1f;
    public float WeaponAimSpeed = 1f;

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
            if (hoverObject.name != "Ground") {
                Player owner = hoverObject.transform.root.GetComponent<Player>();
                Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
                Building building = hoverObject.transform.parent.GetComponent<Building>();

                if (owner) {
                    if (owner.Username == player.Username)
                        player.Hud.SetCursorState(CursorState.Select);
                    else if (CanAttack())
                        player.Hud.SetCursorState(CursorState.Attack);
                    else
                        player.Hud.SetCursorState(CursorState.Select);
                } else if (unit || building && CanAttack()) {
                    player.Hud.SetCursorState(CursorState.Attack);
                } else {
                    player.Hud.SetCursorState(CursorState.Select);
                }
            }
        }
    }

    public virtual bool CanAttack() {
        // Default behavior to be overridden by children
        return false;
    }
    
    public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
        // Only handle input if currently selected
        if (currentlySelected && hitObject && hitObject.name != "Ground") {
            WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();

            // Another selectable object was clicked
            if (worldObject) {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();

                if (resource && resource.IsEmpty())
                    return;

                Player owner = hitObject.transform.root.GetComponent<Player>();
                if (owner) {
                    if (player && player.Human) {
                        if (player.Username != owner.Username && CanAttack())
                            BeginAttack(worldObject);
                        else
                            ChangeSelection(worldObject, controller);
                    } else {
                        ChangeSelection(worldObject, controller);
                    }
                } else {
                    ChangeSelection(worldObject, controller);
                }
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

    public void TakeDamage(int damage) {
        HitPoints -= damage;
        if (HitPoints <= 0)
            Destroy(gameObject);
    }

    protected virtual void Awake() {
        selectionBounds = ResourceManager.InvalidBounds;
        CalculateBounds();
    }

	protected virtual void Start() {
        SetPlayer();

        if (player)
            SetTeamColor();
	}
	
	protected virtual void Update() {
        currentWeaponChargeTime += Time.deltaTime;
        if (isAttacking && !isMovingIntoPosition && !isAiming)
            PerformAttack();
	}

    protected virtual void OnGUI() {
        if (currentlySelected)
            DrawSelection();
    }

    protected virtual void BeginAttack(WorldObject target) {
        this.target = target;
        if (TargetInRange()) {
            isAttacking = true;
            PerformAttack();
        } else {
            AdjustPosition();
        }
    }

    protected void SetTeamColor() {
        TeamColor[] teamColors = GetComponentsInChildren<TeamColor>();
        foreach (TeamColor teamColor in teamColors)
            teamColor.renderer.material.color = player.TeamColor;
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

    protected virtual void AimAtTarget() {
        isAiming = true;
        // This behavior needs to be specified by an object
    }

    protected virtual void UseWeapon() {
        currentWeaponChargeTime = 0f;
    }

    protected Player player;
    protected string[] actions = {};
    protected bool currentlySelected = false;
    protected Bounds selectionBounds;
    protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
    protected GUIStyle healthStyle = new GUIStyle();
    protected float healthPercentage = 1f;
    protected WorldObject target = null;
    protected bool isAttacking = false;
    protected bool isMovingIntoPosition = false;
    protected bool isAiming = false;

    private bool TargetInRange() {
        Vector3 targetLocation = target.transform.position;
        Vector3 direction = targetLocation - transform.position;
        if (direction.sqrMagnitude < WeaponRange * WeaponRange) {
            return true;
        }
        return false;
    }

    private void AdjustPosition() {
        Unit self = this as Unit;
        if (self) {
            isMovingIntoPosition = true;
            Vector3 attackPosition = FindNearestAttackPosition();
            self.StartMove(attackPosition);
            isAttacking = true;
        } else {
            isAttacking = false;
        }
    }

    private Vector3 FindNearestAttackPosition() {
        Vector3 targetLocation = target.transform.position;
        Vector3 direction = targetLocation - transform.position;
        float targetDistance = direction.magnitude;
        float distanceToTravel = targetDistance - 0.9f * WeaponRange;

        return Vector3.Lerp(transform.position, targetLocation, distanceToTravel / targetDistance);
    }

    private void PerformAttack() {
        if (!target) {
            isAttacking = false;
            return;
        }

        if (!TargetInRange())
            AdjustPosition();
        else if (!TargetInFrontOfWeapon())
            AimAtTarget();
        else if (IsReadyToFire())
            UseWeapon();
    }

    private bool IsReadyToFire() {
        if (currentWeaponChargeTime >= WeaponRechargeTime)
            return true;
        return false;
    }

    private bool TargetInFrontOfWeapon() {
        Vector3 targetLocation = target.transform.position;
        Vector3 direction = targetLocation - transform.position;

        if (direction.normalized == transform.forward.normalized)
            return true;
        return false;
    }

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
    private float currentWeaponChargeTime;
}
