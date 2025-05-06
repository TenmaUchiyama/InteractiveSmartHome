
using SpatialLLM.Device;
using UnityEngine;
using UnityEngine.UI;



namespace SpatialLLM.Minimap{


public class MiniMapIcon : MonoBehaviour
{



   public bool isWithVoiceMode = false;
   
   [SerializeField] private Button  button; 
   
   [SerializeField] private MinimapEditorSO minimapEditorSO; 
   [SerializeField] private SADevice device; 

   private Image image; 
   private bool isSelected; 

   private string SELECTED_IMG_COLOR = "#8A8A8A";



   public bool IsSelected() 
   {
      return isSelected; 
   }

   public SADevice GetDevice() 
   {
      return device; 
   }
   


   private void Start() {
      image = GetComponent<Image>();
       button.onClick.AddListener(()=>{
         if(!isWithVoiceMode){
            MinimapManager.Instance.SpawnEditor(minimapEditorSO, device); Debug.Log($"Button Clicked {this.gameObject.name}"); 
            return;
         }

         
         isSelected = !isSelected;

         if(ColorUtility.TryParseHtmlString(SELECTED_IMG_COLOR, out Color color))
         {
            image.color = isSelected ? color : Color.white;
         }
   }); 
   
   }
}
}