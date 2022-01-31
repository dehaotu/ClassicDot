using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using classicDot;
public class Controller : MonoBehaviour
{
    public InputField widthInput;
    public InputField heightInput;
    
    public void UpdateGameboard()
    {
        int width = 0;
        if (widthInput.text.Length > 0) width = int.Parse(widthInput.text);
        int height = 0;
        if (heightInput.text.Length > 0) height = int.Parse(heightInput.text);
        if (width <= 8 && height <= 8)
        Gameboard.Instance.UpdateBoard(width, height);
    }
}
