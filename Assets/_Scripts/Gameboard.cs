using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace classicDot
{
    public class Gameboard : MonoBehaviour
    {
        #region Main gameboard Variables
        // Main gameboard
        public GameObject gameboard;
        public GameObject dot;
        public float interval; //Each dot is spaced evenly from each other (both vertical and horizontal)
        private Transform center;
        public GameObject cursorDot;
        public GameObject linePrefab;
        [Tooltip("Height where the new dot will drop")]
        public Transform dropHeight;
        [HideInInspector] public GameObject currLine;
        private DrawLine drawLine;
        [HideInInspector] public bool lineFollowCursor;
        private float prevInterval;

        [Tooltip("Number of rows on the gameboard")] public int numRows = 3;
        [Tooltip("Number of columns on the gameboard")] public int numCols = 3;

        [Header("Color Settings")]
        [Tooltip("Color choices for dots")] public List<Color> colors = new List<Color>();

        private SpriteRenderer gameboardRenderer;
        private Vector3 topLeft, topRight, bottomLeft, bottomRight;
        //Height and width of the gameboard
        private float height, width;
        //Storing all centers on the grid
        private List<Vector3> allCoords = new List<Vector3>();
        private List<GameObject> spawnedDots = new List<GameObject>();
        private List<int> connectedDots = new List<int>();

        private bool isLookingForNextDot;
        #endregion

        #region Singleton
        private static Gameboard _instance;
        public static Gameboard Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        #endregion

        private void Start()
        {
            center = gameboard.transform.Find("Center");
            center.position = Vector3.zero;
            center.parent = gameboard.transform;
            gameboardRenderer = gameboard.GetComponent<SpriteRenderer>();
            InitiateGameboard();
            prevInterval = interval;
        }

        private void Update()
        {
            ///////////For testing in editor 
            if (interval != prevInterval)
            {
                ResetBoard();
                PopulateDots(numRows, numCols);
                prevInterval = interval;
            }
            ///////////////////////
            if (lineFollowCursor) LineFollowCursor();

            if (Input.GetMouseButtonUp(0))
            {
                ClearMatch();
                RemoveLine();
            }
        }

        /// <summary>
        /// Initialize Gameboard
        /// </summary>
        private void InitiateGameboard()
        {
            if (gameboardRenderer == null)
            {
                Debug.LogError("Gameboard Sprite can not be empty");
                return;
            }

            // Main Gameboard
            topRight = gameboardRenderer.bounds.max;
            topLeft = new Vector3(gameboardRenderer.bounds.min.x, gameboardRenderer.bounds.max.y, 0);
            bottomRight = new Vector3(gameboardRenderer.bounds.max.x, gameboardRenderer.bounds.min.y, 0);
            bottomLeft = gameboardRenderer.bounds.min;
            height = gameboardRenderer.bounds.size.y;
            width = gameboardRenderer.bounds.size.x;

            // Populate Main Gameboard
            PopulateDots(numRows, numCols);
        }

        ///<summary>
        ///Populate a numRows x numCols grid of dots, and calibrate the grid to the center of the gameboard
        ///</summary>
        private void PopulateDots(int numRows, int numCols)
        {
            float x, y;
            Vector3 dotsCenter = new Vector3(bottomLeft.x + (interval * (numCols - 1) / 2),
                bottomLeft.y + (interval * (numRows - 1) / 2), 0);

            for (int r = 0; r < numRows; r++)
            {
                y = bottomLeft.y + interval * r;
                for (int c = 0; c < numCols; c++)
                {
                    x = bottomLeft.x + interval * c;
                    allCoords.Add(new Vector3(x, y, 0) - dotsCenter);
                }
            }
            /*PrintCoords(allCoords);*/
            int counter = 0;
            foreach (Vector3 coord in allCoords)
            {
                // Set up dot properties
                int colorType = Random.Range(0, colors.Count);
                spawnedDots.Add(InstantiateDot(coord, colorType, counter));
                counter++;
            }
        }

        private GameObject InstantiateDot(Vector3 coord, int colorType, int index)
        {
            GameObject spawnedDot = Instantiate(dot, coord, Quaternion.identity);
            spawnedDot.GetComponent<SpriteRenderer>().color = colors[colorType];
            spawnedDot.GetComponent<Dot>().index = index;
            spawnedDot.GetComponent<Dot>().colorType = colorType;
            spawnedDot.transform.parent = center;
            return spawnedDot;
        }



        /// <summary>
        /// Print all spawned dots on the board
        /// </summary>
        /// <param name="coords"></param>
        private void PrintCoords(List<Vector3> coords)
        {
            Debug.Log("printing coordinates");
            string prtStr = "";
            foreach (Vector3 coord in coords)
            {
                prtStr += coord.ToString();
            }
            Debug.Log("Number of Coordinates: " + coords.Count);
            Debug.Log(prtStr);
        }

        public void LineFollowCursor()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorDot.transform.position = new Vector3(pos.x, pos.y, 0);
        }

        public void DrawNewLine(Dot dot)
        {
            currLine = Instantiate(linePrefab, dot.transform.position, Quaternion.identity);
            drawLine = currLine.GetComponent<DrawLine>();
            ConnectDot(dot, 0);
            drawLine.SetColor(colors[dot.colorType]); // Set line color according to starting dot color
        }
        public void ExtendLine(Dot dot)
        {
            ConnectDot(dot, 1);
        }

        /// <summary>
        /// Check if the new dot can be added to the current match, if so, add the node to the line
        /// Also append the cursor position to the line
        /// </summary>
        /// <param name="dot"></param>
        /// <param name="state">
        ///     State 0 = start
        ///     State 1 = mid
        /// </param>
        private void ConnectDot(Dot dot, int state = 1)
        {
            if (!CheckMatch(dot))
            {
                return;
            }
            else
            {
                // Simple scaling animation to help player notice that they have selected a dot
                StartCoroutine(LerpSize(dot.transform, dot.transform.localScale * 1.25f, .25f)); 

                // If current matches with the previous dot
                switch (state)
                {
                    case 0:
                        drawLine.AddNode(dot.transform);
                        connectedDots.Add(dot.index);
                        lineFollowCursor = true;
                        drawLine.AddNode(cursorDot.transform);
                        break;

                    case 1:
                        lineFollowCursor = true;
                        drawLine.RemoveLastNode();
                        drawLine.AddNode(dot.transform);
                        connectedDots.Add(dot.index);
                        drawLine.AddNode(cursorDot.transform);
                        break;
                }
            }
        }

        /// <summary>
        /// Check if the new dot is a match to the previous dot
        /// </summary>
        /// <param name="dot"> The new dot that's pending to be added to the connected dots</param>
        private bool CheckMatch(Dot dot)
        {
            if (connectedDots.Count == 0) return true;
            else
            {
                Dot prevDot = spawnedDots[connectedDots[connectedDots.Count - 1]].GetComponent<Dot>();
                int prevDotInd = prevDot.index;
                // Avoid duplicate nodes
                if (connectedDots.Count > 2 && dot.index == connectedDots[connectedDots.Count - 2])
                {
                    return false;
                }
                // Avoid nodes not in adjacency (including diagnal)
                // Check left, right, top and bottom index
                
                if ((prevDotInd == dot.index - 1 && (prevDotInd / numCols == dot.index / numCols)) ||
                    (prevDotInd == dot.index + 1 && (prevDotInd / numCols == dot.index / numCols)) || 
                    prevDotInd == dot.index + numCols || 
                    prevDotInd == dot.index - numCols)
                {
                    // Avoid different color nodes
                    return dot.colorType == prevDot.colorType;
                }
                return false;
            }
        }

        /// <summary>
        /// Clear all the dots that are on the line, and populate the new one if the top of the column has empty slot
        /// </summary>
        private void ClearMatch()
        {
            // Prohibits the deletion of dots if the number of matches is only 1
            if (connectedDots.Count < 2) return;

            // Square Mechanic
            // If there are duplicate code, we know it can only that there's a square in the match
            if (connectedDots.Count > 2 && connectedDots.Distinct().ToList().Count != connectedDots.Count)
            {
                int matchColor = spawnedDots[connectedDots[0]].GetComponent<Dot>().colorType;
                for (int y = 0; y < numRows; y++)
                {
                    for (int x = 0; x < numCols; x++)
                    {
                        int ind = y * numCols + x;
                        if (spawnedDots[ind].GetComponent<Dot>().colorType == matchColor)
                        {
                            connectedDots.Add(ind);
                        }
                    }
                }
                connectedDots = connectedDots.Distinct().ToList();
            }

            connectedDots.Sort();
            connectedDots.Reverse();

            // Move dots and realize drop animation 
            foreach (int dotInd in connectedDots)
            {
                int currDotInd = dotInd;
                int dotAboveInd = dotInd + numCols;
                // Destroy and refill dots
                while (dotAboveInd < numRows * numCols)
                {
                    Destroy(spawnedDots[currDotInd]);

                    // Replace the removed the dot with dots above
                    Dot dotAbove = spawnedDots[dotAboveInd].GetComponent<Dot>();
                    int color_above = dotAbove.colorType;
                    spawnedDots[currDotInd] = InstantiateDot(allCoords[dotAboveInd], color_above, currDotInd);
                    StartCoroutine(LerpPosition(spawnedDots[currDotInd].transform, allCoords[currDotInd], 0.5f));
                    currDotInd = dotAboveInd;
                    dotAboveInd += numCols;
                }
                // Refill the top empty place
                Destroy(spawnedDots[currDotInd]);
                spawnedDots[currDotInd] = InstantiateDot(new Vector3(allCoords[currDotInd].x, dropHeight.position.y, 0), Random.Range(0, colors.Count), currDotInd);
                StartCoroutine(LerpPosition(spawnedDots[currDotInd].transform, allCoords[currDotInd], 0.5f));
            }

        }

        /// <summary>
        /// Remove the line
        /// </summary>
        public void RemoveLine()
        {
            Destroy(currLine);
            currLine = null;
            drawLine = null;
            connectedDots.Clear();
            lineFollowCursor = false;
        }

        /// <summary>
        /// Lerping dropping animation for dots
        /// </summary>
        IEnumerator LerpPosition(Transform targetTransform, Vector3 targetPosition, float duration)
        {
            float time = 0;
            Vector3 startPosition = targetTransform.position;

            while (time < duration)
            {
                if(targetTransform) targetTransform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (targetTransform) targetTransform.position = targetPosition;
        }

        /// <summary>
        /// Lerping scaling animation for dots
        /// </summary>
        IEnumerator LerpSize(Transform targetTransform, Vector3 targetSize, float duration)
        {
            float time = 0;
            Vector3 startSize = targetTransform.localScale;

            // Scaling up
            while (time < duration)
            {
                if (targetTransform) targetTransform.localScale = Vector3.Lerp(startSize, targetSize, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (targetTransform) targetTransform.localScale = targetSize;
            // Scaling down
            time = 0;
            while (time < duration)
            {
                if (targetTransform) targetTransform.localScale = Vector3.Lerp(targetSize, startSize, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (targetTransform) targetTransform.localScale = startSize;
        }

        public void SmoothLine(bool isSmoothing)
        {
            drawLine.lineSmoothing = isSmoothing;
        }

        public void ResetBoard()
        {
            RemoveLine();
            // Destroy all dots
            foreach (GameObject spawnedDot in spawnedDots)
            {
                Destroy(spawnedDot);
            }
            spawnedDots.Clear();
            allCoords.Clear();
            connectedDots.Clear();
        }

        public void UpdateBoard(int w, int h)
        {
            ResetBoard();
            numCols = w;
            numRows = h;
            InitiateGameboard();
        }
    }

}
