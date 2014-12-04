using RTS;
using UnityEngine;

public class OreDeposit : Resource {
    protected override void Start() {
        base.Start();
        numBlocks = GetComponentsInChildren<Ore>().Length;
        resourceType = ResourceType.Ore;
    }

    protected override void Update() {
        base.Update();
        float percentLeft = (float)amountLeft / (float)Capacity;

        // This should not be possible
        if (percentLeft < 0)
            percentLeft = 0;

        int numBlocksToShow = (int)(percentLeft * numBlocks);
        Ore[] blocks = GetComponentsInChildren<Ore>();

        if (numBlocksToShow >= 0 && numBlocksToShow < blocks.Length) {
            Ore[] sortedBlocks = new Ore[blocks.Length];

            // Sort list from highest to lowest
            foreach (Ore ore in blocks)
                sortedBlocks[blocks.Length - int.Parse(ore.name)] = ore;

            for (int i = numBlocksToShow; i < sortedBlocks.Length; i++)
                sortedBlocks[i].renderer.enabled = false;

            CalculateBounds();
        }
    }

    private int numBlocks;
}
