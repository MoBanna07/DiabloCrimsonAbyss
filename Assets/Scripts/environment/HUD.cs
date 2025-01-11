using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshProUGUI

public class HUD : MonoBehaviour
{
    public TextMeshProUGUI textField; // Drag and drop a TextMeshProUGUI component here
    public Button targetButton; // Drag and drop the Button here to modify its child image
    //public Image targetImage; // Drag and drop a normal Image component here in case of using images not buttons 

    private Image buttonImage; // The child Image component of the button
    private Color originalImageColor; // Store the original color of the button image

    //void Start()
    //{
    //    if (textField == null)
    //    {
    //        Debug.LogError("TextField (TextMeshProUGUI) is not assigned in the Inspector!");
    //    }

    //    if (targetButton == null)
    //    {
    //        Debug.LogError("Target Button is not assigned in the Inspector!");
    //    }
    //    else
    //    {
    //        // Get the Image component of the button's child (usually its background)
    //        buttonImage = targetButton.GetComponentInChildren<Image>();
    //        if (buttonImage != null)
    //        {
    //            // Save the original image color for resetting later
    //            originalImageColor = buttonImage.color;
    //        }
    //        else
    //        {
    //            Debug.LogError("No Image component found in the Button's children!");
    //        }
    //    }
    //}

    //public void SetPotions(string numberOfPotions)
    //{
    //    if (textField != null)
    //    {
    //        textField.text = numberOfPotions;
    //        Debug.Log("Potions: " + numberOfPotions);
    //    }
    //    else
    //    {
    //        Debug.LogError("TextField is not assigned!");
    //    }
    //}

    //public void SetRune(string numberOfRunes)
    //{
    //    if (textField != null)
    //    {
    //        textField.text = numberOfRunes;
    //    }
    //    else
    //    {
    //        Debug.LogError("TextField is not assigned!");
    //    }
    //}

    //public void DarkenImage()
    //{
    //    if (buttonImage != null)
    //    {
    //        // Make the button image darker by reducing RGB values
    //        Color darkenedColor = originalImageColor * 0.7f; // 70% brightness
    //        buttonImage.color = darkenedColor;

    //        Debug.Log("Button image darkened to simulate a pressed state.");
    //    }
    //}

    //public void ResetImageColor()
    //{
    //    if (buttonImage != null)
    //    {
    //        // Reset the button image color to its original color
    //        buttonImage.color = originalImageColor;

    //        Debug.Log("Button image color reset to the original state.");
    //    }
    //}
    //use those if the HUD was images not buttons
    //public void DarkenImage()
    //{
    //    if (targetImage != null)
    //    {
    //        // Make the image darker by reducing RGB values
    //        Color darkenedColor = originalImageColor * 0.7f; // 70% brightness
    //        targetImage.color = darkenedColor;

    //        Debug.Log("Image darkened to simulate a pressed state.");
    //    }
    //    else
    //    {
    //        Debug.LogError("Target Image is not assigned!");
    //    }
    //}

    //public void ResetImageColor()
    //{
    //    if (targetImage != null)
    //    {
    //        // Reset the image color to its original color
    //        targetImage.color = originalImageColor;

    //        Debug.Log("Image color reset to the original state.");
    //    }
    //    else
    //    {
    //        Debug.LogError("Target Image is not assigned!");
    //    }
    //}
}
