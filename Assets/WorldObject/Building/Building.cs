using System.Collections;
using System.Collections.Generic;
using RTS;
using UnityEngine;

public class Building : WorldObject {
    public float maxBuildProgress;
    protected Queue<string> buildQueue;

    public IEnumerable<string> GetBuildQueueValues() {
        return buildQueue;
    }

    public float GetBuildPercentage() {
        return currentBuildProgress / maxBuildProgress;
    }
    
    protected override void Awake() {
        base.Awake();

        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
        float spawnZ = selectionBounds.center.z + transform.forward.z * selectionBounds.extents.z + transform.forward.z * 10;
        spawnPoint = new Vector3(spawnX, 0f, spawnZ);
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
    }

    protected void CreateUnit(string unitName) {
        buildQueue.Enqueue(unitName);
    }

    protected void ProcessBuildQueue() {
        if (buildQueue.Count == 0) return;

        currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
        if (currentBuildProgress > maxBuildProgress && player) {
            player.AddUnit(buildQueue.Dequeue(), spawnPoint, transform.rotation);
            currentBuildProgress = 0f;
        }
    }

    private float currentBuildProgress = 0f;
    private Vector3 spawnPoint;
}
