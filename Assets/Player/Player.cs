using RTS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public HUD Hud;
	public string Username;
	public bool Human;
    public WorldObject SelectedObject { get; set; }
    public int startMoney;
    public int startMoneyLimit;
    public int startPower;
    public int startPowerLimit;
    public Material notAllowedMaterial, allowedMaterial;
    public Color TeamColor;

    public void AddResource(ResourceType type, int amount) {
        resources[type] += amount;
    }

    public void IncrementResourceLimit(ResourceType type, int amount) {
        resourceLimits[type] += amount;
    }

    public void AddUnit(string unitName, Vector3 spawnPoint, Quaternion rotation, Vector3 rallyPoint, Building creator) {
        Units units = GetComponentInChildren<Units>();
        GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
        newUnit.transform.parent = units.transform;

        Unit unitObject = newUnit.GetComponent<Unit>();
        if (unitObject) {
            unitObject.SetBuilding(creator);
            if (spawnPoint != rallyPoint)
                unitObject.StartMove(rallyPoint);
        }
    }

    public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
        GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
        tempBuilding = newBuilding.GetComponent<Building>();
        if (tempBuilding) {
            tempCreator = creator;
            findingPlacement = true;
            tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
            tempBuilding.SetColliders(false);
            tempBuilding.SetPlayingArea(playingArea);
        } else {
            Destroy(newBuilding);
        }
    }

    public bool IsFindingBuildingLocation() {
        return findingPlacement;
    }

    public void FindBuildingLocation() {
        Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
        newLocation.y = 0;
        tempBuilding.transform.position = newLocation;
    }

    public bool CanPlaceBuilding() {
        bool canPlace = true;

        Bounds placeBounds = tempBuilding.SelectionBounds;
        float cx = placeBounds.center.x;
        float cy = placeBounds.center.y;
        float cz = placeBounds.center.z;
        float ex = placeBounds.extents.x;
        float ey = placeBounds.extents.y;
        float ez = placeBounds.extents.z;

        List<Vector3> corners = new List<Vector3> {
            Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)),
            Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)),
        };

        foreach (Vector3 corner in corners) {
            GameObject hitObject = WorkManager.FindHitObject(corner);
            if (hitObject && hitObject.name != "Ground") {
                WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
                if (worldObject && placeBounds.Intersects(worldObject.SelectionBounds))
                    canPlace = false;
            }
        }
        return canPlace;
    }

    public void StartConstruction() {
        findingPlacement = false;
        Buildings buildings = GetComponentInChildren<Buildings>();
        if (buildings)
            tempBuilding.transform.parent = buildings.transform;

        tempBuilding.SetPlayer();
        tempBuilding.SetColliders(true);
        tempCreator.SetBuilding(tempBuilding);
        tempBuilding.StartConstruction();
    }

    public void CancelBuildingPlacement() {
        findingPlacement = false;
        Destroy(tempBuilding.gameObject);
        tempBuilding = null;
        tempCreator = null;
    }

	private void Start() {
        Hud = GetComponentInChildren<HUD>();
        AddStartResources();
        AddStartResourceLimits();
	}
	
	private void Update() {
        if (Human) {
            Hud.SetResourceValues(resources, resourceLimits);
        }

        if (findingPlacement) {
            tempBuilding.CalculateBounds();
            if (CanPlaceBuilding())
                tempBuilding.SetTransparentMaterial(allowedMaterial, false);
            else
                tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
        }
	}

    private void Awake() {
        resources = InitResourceList();
        resourceLimits = InitResourceList();
    }

    private Dictionary<ResourceType, int> InitResourceList() {
        return new Dictionary<ResourceType, int> {
            { ResourceType.Money, 0 },
            { ResourceType.Power, 0 }
        };
    }

    private void AddStartResources() {
        AddResource(ResourceType.Money, startMoney);
        AddResource(ResourceType.Power, startPower);
    }

    private void AddStartResourceLimits() {
        IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
        IncrementResourceLimit(ResourceType.Power, startPowerLimit);
    }

    private Dictionary<ResourceType, int> resources;
    private Dictionary<ResourceType, int> resourceLimits;
    private Building tempBuilding;
    private Unit tempCreator;
    private bool findingPlacement = false;
}
