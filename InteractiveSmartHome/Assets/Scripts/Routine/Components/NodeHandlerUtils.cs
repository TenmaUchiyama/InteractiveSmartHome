using System;
using System.Collections;
using System.Collections.Generic;
using MRFlow.Component;
using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodeHandlerUtils : MonoBehaviour
{

    const string NORMAL= "Normal";
    const string HIGHLIGHTED = "Highlighted";
    const string PRESSED = "Pressed";



    [Tooltip("The IInteractableView (Interactable) component to wrap.")]
    [SerializeField, Interface(typeof(IInteractableView))]
    private UnityEngine.Object _interactableView;
    private IInteractableView InteractableView;

    [SerializeField] MRNode _mrNode; 
    [SerializeField] Image backgroundImage;


    public UnityEvent onClickEvent;

    private UITheme thisTheme;



   

 



    void OnDisable()
        {
           
        InteractableView.WhenStateChanged -= HandleStateChanged;
           
        }

    void Start()
    {
        thisTheme = _mrNode.GetUITheme();

    }

    protected virtual void OnEnable()
    {
     
        InteractableView.WhenStateChanged += HandleStateChanged;
    }
    private void Awake() {
        InteractableView = _interactableView as IInteractableView;

    }


        private void HandleStateChanged(InteractableStateChangeArgs args)
    {


        
         switch(args.NewState)
         {
        //    case InteractableState.Normal:
        //         Color color = backgroundImage.color;
        //         color.a = 0.5f; // デフォルトの透明度を0.5に設定
        //         backgroundImage.color = color;
        //         break;

            // case InteractableState.Hover:
            //     Debug.Log("<color=red>Hover</color>");
            //     Color colorHover = this.thisTheme.sectionPlateColor; // sectionPlateColorから直接取得
            //     colorHover.a = 1f; // Hoverの透明度を1に設定
            //     backgroundImage.color = colorHover;
            //     break;

            // case InteractableState.Select:
            //     Color colorSelect = this.thisTheme.sectionPlateColor; // sectionPlateColorから直接取得
            //     colorSelect.a = 1f; // Selectの透明度を1に設定
            //     backgroundImage.color = colorSelect;
            //     break;
            


         }
    }

   
}
