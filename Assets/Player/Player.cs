using RTS;
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

    public void AddResource(ResourceType type, int amount) {
        resources[type] += amount;
    }

    public void IncrementResourceLimit(ResourceType type, int amount) {
        resourceLimits[type] += amount;
    }

	// Use this for initialization
	private void Start() {
        Hud = GetComponentInChildren<HUD>();
        AddStartResources();
        AddStartResourceLimits();
	}
	
	// Update is called once per frame
	private void Update() {
        if (Human) {
            Hud.SetResourceValues(resources, resourceLimits);
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
}
