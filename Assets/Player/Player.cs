using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public HUD Hud;
	public string Username;
	public bool Human;
    public WorldObject SelectedObject { get; set; }

	// Use this for initialization
	void Start () {
        Hud = GetComponentInChildren<HUD>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
