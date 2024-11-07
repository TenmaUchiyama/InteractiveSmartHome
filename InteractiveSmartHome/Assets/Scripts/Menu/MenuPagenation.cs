using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPagenation : MonoBehaviour
{
    [SerializeField] private GameObject actionPage; 
    [SerializeField] private GameObject routinePage; 



    private void Start() {
        OpenRoutinePage();
    }



    public void OpenActionPage()
    {
        actionPage.SetActive(true);
        routinePage.SetActive(false);
    }

    public void OpenRoutinePage()
    {
        actionPage.SetActive(false);
        routinePage.SetActive(true);
    }
}
