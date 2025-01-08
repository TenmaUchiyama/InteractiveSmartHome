
using SpatialLLM.Device;
using UnityEngine;
using UnityEngine.UI;



namespace SpatialLLM.Minimap{


public class MiniMapIcon : MonoBehaviour
{
   
   [SerializeField] private Button  button; 
   
   [SerializeField] private MinimapEditorSO minimapEditorSO; 
   [SerializeField] private SADevice device; 


   private void Start() {
      Debug.Log($"<color=yellow>MiniMapIcon Start {this.gameObject.name}</color>");
       button.onClick.AddListener(()=>{MinimapManager.Instance.SpawnEditor(minimapEditorSO, device); Debug.Log($"Button Clicked {this.gameObject.name}");});
   }
   
}
}