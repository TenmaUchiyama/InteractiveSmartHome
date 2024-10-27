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


    private Animator animator;

   

 



    protected virtual void OnDisable()
        {
           
                InteractableView.WhenStateChanged -= HandleStateChanged;
           
        }

    private void Start() {
        animator = GetComponent<Animator>();
        InteractableView = _interactableView as IInteractableView;
         InteractableView.WhenStateChanged += HandleStateChanged;

    }


        private void HandleStateChanged(InteractableStateChangeArgs args)
    {


        
         switch(args.NewState)
         {
           case InteractableState.Normal:
                 
                 animator.SetTrigger(NORMAL);
            break;
            case InteractableState.Hover:
            Debug.Log("<color=green>Hover</color>");
                animator.SetTrigger(HIGHLIGHTED);
            break;
            case InteractableState.Select:
                Debug.Log("<color=green>Selected</color>");
                   onClickEvent.Invoke();
                animator.SetTrigger(PRESSED);
            break;
            


         }
    }

   



    
}
