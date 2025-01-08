using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using UnityEngine;

public class MinimapManager : Singleton<MinimapManager>
{
   [SerializeField] private MinimapEditor minimapEditor; 




    #if UITY_EDITOR
    [ContextMenu("Test")]
    public void SpawnEditor()
    {
        SpawnEditor(testSO, testDevice);
    }
    #endif

   public void SpawnEditor(MinimapEditorSO minimapEditorSO, SADevice saDevice)
   {
    minimapEditor.gameObject.SetActive(true);
    minimapEditor.SpawnEditor(minimapEditorSO, saDevice);
   }
}
