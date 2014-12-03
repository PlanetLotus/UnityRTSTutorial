using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTS;

public class GameObjectList : MonoBehaviour {
    public GameObject[] buildings;
    public GameObject[] units;
    public GameObject[] worldObjects;
    public GameObject player;

    public GameObject GetBuilding(string name) {
        for (int i = 0; i < buildings.Length; i++) {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name)
                return buildings[i];
        }
        return null;
    }

    public GameObject GetUnit(string name) {
        for (int i = 0; i < units.Length; i++) {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit && unit.name == name)
                return units[i];
        }
        return null;
    }

    public GameObject GetWorldObject(string name) {
        foreach (GameObject worldObject in worldObjects) {
            if (worldObject.name == name)
                return worldObject;
        }
        return null;
    }

    public GameObject GetPlayerObject() {
        return player;
    }

    public Texture2D GetBuildImage(string name) {
        for (int i = 0; i < buildings.Length; i++) {
            Building building = buildings[i].GetComponent<Building>();
            if (building && building.name == name) return building.BuildImage;
        }

        for (int i = 0; i < units.Length; i++) {
            Unit unit = units[i].GetComponent<Unit>();
            if (unit && unit.name == name) return unit.BuildImage;
        }
        return null;
    }

    void Start() {

    }

    void Update() {

    }

    private void Awake() {
        if (!created) {
            DontDestroyOnLoad(transform.gameObject);
            ResourceManager.SetGameObjectList(this);
            created = true;
        } else {
            Destroy(this.gameObject);
        }
    }

    private static bool created = false;
}
