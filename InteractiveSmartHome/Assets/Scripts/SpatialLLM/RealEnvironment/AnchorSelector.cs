using System.Net.Security;
using UnityEngine;

public class AnchorSelector : MonoBehaviour
{
    public OVRSpatialAnchor currentCollidedAnchor { get; private set; }
    

    private void OnTriggerEnter(Collider other)
    {
        var anchor = other.GetComponent<OVRSpatialAnchor>();
        if (anchor != null)
        {
            currentCollidedAnchor = anchor;
            Debug.Log($"<color=cyan>Anchor Entered: {anchor.Uuid}</color>");

        
        }


        if(other.TryGetComponent<DrawOnHover>(out var component))
        {
            component.VisualizeTargetDevice(Color.green);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var anchor = other.GetComponent<OVRSpatialAnchor>();
        if (anchor != null && anchor == currentCollidedAnchor)
        {
            Debug.Log($"<color=gray>Anchor Exited: {anchor.Uuid}</color>");
            currentCollidedAnchor = null;
        }


        if(other.TryGetComponent<DrawOnHover>(out var component))
        {
            component.VisualizeTargetDevice(Color.white);
        }
    }
}
