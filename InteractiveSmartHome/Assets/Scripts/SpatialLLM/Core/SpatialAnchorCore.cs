using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.XR.BuildingBlocks;
using UnityEngine;

public class SpatialAnchorCore : SpatialAnchorCoreBuildingBlock
{





    public async Task SaveSAAnchor(OVRSpatialAnchor anchor)
    {
        await base.WaitForInit(anchor);
        await base.SaveAsync(anchor);
    }
  
}
