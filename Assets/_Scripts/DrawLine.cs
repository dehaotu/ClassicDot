using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace classicDot
{
    public class DrawLine : MonoBehaviour
    {
        
        public List<Transform> nodes = new List<Transform>();
        public float LineWidth = .5f;
        public bool lineSmoothing = false;
        private LineRenderer lr;
        private Color lineColor;
        private bool isColorSet = false;

        private void Start()
        {
            lr = GetComponent<LineRenderer>();
            lr.positionCount = nodes.Count;
        }

        private void Update()
        {
            UpdatePositions();
            // Things to fix the look of the line rea
            lr.startWidth = LineWidth;
            lr.endWidth = LineWidth;
            lr.widthMultiplier = LineWidth;
            if (lr != null && !isColorSet)
            {
                lr.startColor = lineColor;
                lr.endColor = lineColor;
                isColorSet = true;
            }
        }

        public void AddNode(Transform node)
        {
            nodes.Add(node);
          
        }

        public void RemoveLastNode()
        {
            nodes.RemoveAt(nodes.Count - 1);
        }

        private void UpdatePositions()
        {
            if (nodes.Count > 0)
            {
                lr.positionCount = nodes.Count;
                if (lineSmoothing)
                {

                    lr.SetPositions(nodes.ConvertAll(x => RoundVector3(x.position - new Vector3(0, 0, 2))).ToArray());
                }
                else
                {

                    lr.SetPositions(nodes.ConvertAll(x => x.position - new Vector3(0, 0, 2)).ToArray());
                }
            }
        }

        public void SetColor(Color color)
        {
            lineColor = color;
        }

        private Vector3 RoundVector3(Vector3 vec)
        {
            return new Vector3(Mathf.Round(vec.x * 100.0f) * 0.01f, Mathf.Round(vec.y * 100.0f) * 0.01f, Mathf.Round(vec.z * 100.0f) * 0.01f);
        }

    }
}
