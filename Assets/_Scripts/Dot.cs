using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace classicDot
{
    public class Dot : MonoBehaviour
    {
        public int index;
        public int colorType;

        private void OnMouseDown()
        {
            if (Gameboard.Instance.currLine == null)
            {
                Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
                transform.position = pos;
                Gameboard.Instance.DrawNewLine(this);
            } 
        }
        
        private void OnMouseEnter()
        {
            if (Gameboard.Instance.currLine != null) Gameboard.Instance.SmoothLine(true);
            if (Gameboard.Instance.currLine != null)
            {
                Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);
                transform.position = pos;
                Gameboard.Instance.ExtendLine(this);
            }
        }

        private void OnMouseExit()
        {
            // Stop smoothing the line,
            // since constanly rounding up the cursor location will result in jittery line swing
            if (Gameboard.Instance.currLine != null) Gameboard.Instance.SmoothLine(false);
        }
    }
}

