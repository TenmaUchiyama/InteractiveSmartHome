using System.Collections;
using System.Collections.Generic;
using NodeTypes;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MRNodeUI : MonoBehaviour
{
    
    [SerializeField] NodeType nodeType;
    [SerializeField] ThemeType themeType; 
    [SerializeField] private NodeThemeMapSO nodeThemeMapSO;

     const string NORMAL= "Normal";
    const string HIGHLIGHTED = "Highlighted";
    const string PRESSED = "Pressed";



    [Tooltip("The IInteractableView (Interactable) component to wrap.")]
    [SerializeField, Interface(typeof(IInteractableView))]
    private UnityEngine.Object _interactableView;
    private IInteractableView InteractableView;
    [SerializeField] private RayInteractable rayInteractable;

    [SerializeField] Image backgroundImage;
    [SerializeField] Button button;

    public UnityEvent onClickEvent;
    private UITheme uiTheme;




    private void Awake() {
        InteractableView = _interactableView as IInteractableView;
        uiTheme = nodeThemeMapSO.GetUITheme(themeType);
    }


    void OnValidate()
    {
       
        // backgroundImage.color = nodeThemeMapSO.GetUITheme(themeType).backplateColor;
       
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.normalColor = nodeThemeMapSO.GetUITheme(themeType).backplateColor;
        colorBlock.highlightedColor = nodeThemeMapSO.GetUITheme(themeType).sectionPlateColor;
        colorBlock.pressedColor = nodeThemeMapSO.GetUITheme(themeType).sectionPlateColor;
        colorBlock.selectedColor = nodeThemeMapSO.GetUITheme(themeType).sectionPlateColor;
        colorBlock.disabledColor = nodeThemeMapSO.GetUITheme(themeType).backplateColor;
        colorBlock.colorMultiplier = 1;
        colorBlock.fadeDuration = 0.1f;
        this.button.colors = colorBlock;
    }
    


    

    public NodeType GetNodeType()
    {
        return nodeType;
    }



    private void Start() {
        InteractableView = _interactableView as IInteractableView;
    }
    

      protected virtual void OnEnable()
        {
              
                InteractableView.WhenStateChanged += HandleStateChanged;
            }



    protected virtual void OnDisable()
        {
           
                InteractableView.WhenStateChanged -= HandleStateChanged;
           
        }
      private void HandleStateChanged(InteractableStateChangeArgs args)
    {

         switch(args.NewState)
         {
           case InteractableState.Normal:
               
                backgroundImage.color = uiTheme.backplateColor;
            break;
            case InteractableState.Hover:
              
                backgroundImage.color = uiTheme.sectionPlateColor;
            break;
            case InteractableState.Select:
                   onClickEvent.Invoke();
                backgroundImage.color = uiTheme.sectionPlateColor;
            break;
        

         }
    }

}
