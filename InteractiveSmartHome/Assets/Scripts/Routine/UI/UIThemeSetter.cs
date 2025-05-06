using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NodeTypes;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class UIThemeSetter : MonoBehaviour
{
  
    [SerializeField] private ThemeType selectThemeType; 
    [SerializeField] private NodeThemeMapSO nodeThemeMapSO;

    private bool hasAwoken = false;


    void OnValidate()
    {
        
            ApplyTheme();
       
    }



    public UITheme GetTheme()
    {
        return nodeThemeMapSO.GetUITheme(selectThemeType);
    }

    private void ApplyTheme() {

        UITheme theme = nodeThemeMapSO.GetUITheme(selectThemeType);
        Animator[] animators = GetComponents<Animator>().Concat(GetComponentsInChildren<Animator>()).ToArray();
            foreach (var animator in animators)
            {
                
                if (animator.CompareTag("QDSUIPrimaryButton"))
                {
                    animator.runtimeAnimatorController = theme.acPrimaryButton;
                }
                else if (animator.CompareTag("QDSUISecondaryButton"))
                {
                    animator.runtimeAnimatorController = theme.acSecondaryButton;
                }
                else if (animator.CompareTag("QDSUIBorderlessButton"))
                {
                    animator.runtimeAnimatorController = theme.acBorderlessButton;
                }
                else if (animator.CompareTag("QDSUIDestructiveButton"))
                {
                    animator.runtimeAnimatorController = theme.acDestructiveButton;
                }
                else if (animator.CompareTag("QDSUIToggleButton"))
                {
                    animator.runtimeAnimatorController = theme.acToggleButton;
                }
                else if (animator.CompareTag("QDSUIToggleBorderlessButton"))
                {
                    animator.runtimeAnimatorController = theme.acToggleBorderlessButton;
                }
                else if (animator.CompareTag("QDSUIToggleSwitch"))
                {
                    animator.runtimeAnimatorController = theme.acToggleSwitch;
                }
                else if (animator.CompareTag("QDSUIToggleCheckboxRadio"))
                {
                    animator.runtimeAnimatorController = theme.acToggleCheckboxRadio;
                }
                else if (animator.CompareTag("QDSUITextInputField"))
                {
                    animator.runtimeAnimatorController = theme.acTextInputField;
                }
                animator.Update(0);
            }

            // Apply color to all image objects under the canvas. For interactable elements, updates default normal state for editor view.
            Image[] images = GetComponents<Image>().Concat(GetComponentsInChildren<Image>()).ToArray();

            foreach (var image in images)
            {
                 if(image.CompareTag("ThemeException"))
                {
                    continue; 
                }
                if (image.CompareTag("QDSUIIcon"))
                {
                    // Apply the text color to the icons as well
                    image.color = theme.textPrimaryColor;
                }
                else if (image.CompareTag("QDSUIAccentColor"))
                {
                    // Apply the accent color
                    image.color = theme.primaryButton.normal;
                }
                else if (image.CompareTag("QDSUISharedThemeColor") || image.CompareTag("QDSUIPrimaryButton") || image.CompareTag("QDSUIDestructiveButton") || image.CompareTag("QDSUIBorderlessButton") || image.CompareTag("QDSUIToggleBorderlessButton"))
                {
                    // Same color scheme for both themes, No changes. Or apply any future theme adjustments here.
                }
                else if(image.CompareTag("QDSUISecondaryButton") || image.CompareTag("QDSUIToggleButton"))
                {
                    // Colors are applied though animation clips.
                    // Apply the backplate color of the button for plated button.
                    // image.color = theme.buttonPlateColor;
                }
                else if (image.CompareTag("QDSUISection"))
                {
                    // Apply the section plate color.
                    image.color = theme.sectionPlateColor;
                }
                else if (image.CompareTag("QDSUITooltip"))
                {
                    // Apply the backplate color of the button for plated button.
                    image.color = theme.tooltipColor;
                }
                else if (image.CompareTag("QDSUITextInputField"))
                {
                    // Colors are applied though animation clips.
                }
                else
                {
                    image.color = theme.backplateColor;
                }
            }

            SpriteRenderer[] spriteRenderers =GetComponents<SpriteRenderer>().Concat(GetComponentsInChildren<SpriteRenderer>()).ToArray();

            foreach (var spriteRenderer in spriteRenderers) 
            {
                spriteRenderer.color = theme.tooltipColor;
            }

            // Apply color and font to all TextMeshProUGUI components under the canvas
            TextMeshProUGUI[] texts = GetComponents<TextMeshProUGUI>().Concat(GetComponentsInChildren<TextMeshProUGUI>()).ToArray();
            foreach (var text in texts)
            {
                text.font = theme.textFontMedium; // Set the font

                if(text.CompareTag("ThemeException"))
                {
                    continue; 
                }

                if (text.CompareTag("QDSUISharedThemeColor") || text.CompareTag("QDSUIPrimaryButton") || text.CompareTag("QDSUIDestructiveButton"))
                {
                    // Same color scheme for both themes, No changes. Or apply any future theme adjustments here.
                }
                else if (text.CompareTag("QDSUITextSecondaryColor"))
                {
                    text.color = theme.textSecondaryColor;
                }
                else
                {
                    text.color = theme.textPrimaryColor;
                }
            }
    }



    
       
    
}
