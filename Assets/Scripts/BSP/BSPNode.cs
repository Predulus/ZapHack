using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using DrawTextToTexture2D = Assets.Scripts.Utils.DrawTextToTexture2D;

namespace Assets.Scripts.BSP
{
    enum Direction
    {
        NONE = 0,
        HORIZONTAL = 1,
        VERTICAL = 2
    }

    enum Position
    {
        NONE = 0,
        LEFT = 1,
        RIGHT = 2,
        TOP = 3,
        BOTTOM
    }
    class BSPNode
    {   //
        // Static vars
        //
        private Direction splitDirection;
        private Position nodePosition;
        public Direction SplitDirection { get => splitDirection; set => splitDirection = value; }
        public Position NodePosition { get => nodePosition; set => nodePosition = value; }
        public static int textureSize = 400;
        public static int numCellsHoriz = (int)(1.5 * textureSize);
        public static int numCellsVert = (int)(1.0 * textureSize);
        private static int currentID;
        private static int nodeBorderSize = textureSize/40; //40
        // Minimum desired room width and height
        private static int minRoomWidth = (int) (1.5 * 80 * textureSize/400);
        private static int minRoomHeight = 80 * textureSize / 400;
        public static Color32[] colorData;
        public static RectInt colorDataRect;
        public static RectInt textureRect = new RectInt(0, 0, numCellsHoriz, numCellsVert);
        public static Color32 tempColor32;
        public static int charWidth = DrawTextToTexture2D.charWidth;
        //            public static int charHeight = 17;
        public static int charHeight = DrawTextToTexture2D.charHeight;
        //            public static int charPixelCount = 38;
        public static int numCharsInSourceTexture = DrawTextToTexture2D.numCharsInSourceTexture;
        //        public static int pixelsPerChar = 187;
        public static int pixelsPerChar = DrawTextToTexture2D.pixelsPerChar;

        //
        // Private vars
        //
        private int _ID;
        private BSPNode parentNode;
        private BSPNode[] children = new BSPNode[2];
        private RectInt nodeRect;
        private int minSplitWidth, minSplitHeight;
        // nodeBorder: Need some space on each edge of a node so a) rooms arent bumping up against each other and b) space for corridors
        private bool isLargest = false;
        private Room room;

        //
        // Public vars and getters/setters
        //
        public BSPNode ParentNode { get => parentNode; set => parentNode = value; }
        public int ID { get => _ID; set => _ID = value; }
        public Room Room { get => room; set => room = value; }
        public bool IsLargest { get => isLargest; set => isLargest = value; }
        public BSPNode[] Children { get => children; set => children = value; }
        public RectInt NodeRect { get => nodeRect; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_nodeRect">A rectangle for this node</param>
        /// <param name="_parentNode">A reference to the parent node</param>
        public BSPNode(RectInt _nodeRect, BSPNode _parentNode = null)
        {
            if (_parentNode == null)
            {
                int colorDataLength = (int)(1.5 * textureSize * textureSize);
                colorData = new Color32[colorDataLength];
                colorDataRect = new RectInt(0, 0, (int)(1.5 * textureSize), textureSize);
                //Color32 orange = new Color32(200, 120, 0, 255);
                Color32 initialFillColor = new Color32(0, 0, 0, 255);
                for (int i = 0; i < colorDataLength; i++)
                {
                    colorData[i] = initialFillColor;
                }
            }
            this.parentNode = _parentNode;
            this._ID = currentID;
            currentID++;
            this.nodeRect = _nodeRect;
            // Check that children are null, as they should be
            if (children[0] != null)
                Debug.Log("Node " + this.ID.ToString() + " had non-null child 0 at construction!");
            if (children[1] != null)
                Debug.Log("Node " + this.ID.ToString() + " had non-null child 1 at construction!");

            // calculate the minimum width and height for splits of this node, based on minimum room dimensions and borders
            this.minSplitWidth = minRoomWidth + (nodeBorderSize * 2);
            this.minSplitHeight = minRoomHeight + (nodeBorderSize * 2);
            if (ZapDebug.DisplayNodes)
                DrawRect(nodeRect, textureRect, tempColor32);
        }
        /// <summary>
        /// Split() Creates left and right children for this node, splitting the nodeRect randomly either horiz or vert,
        /// a random ratio.  A check is made to ensure the smaller child is not too small/short
        /// </summary>
        /// <returns>Returns a boolean value depending on whether the node was able to be split</returns>
        public bool Split()
        {
            // Randomly split either horizontally or vertically
            if (Random.Range(0, 2) == 0 && CanBeSplitHorizontally())
            { // Split horizontally
                // Calculate the RectInts for the new child cells
                int _leftWidth = Random.Range(minSplitWidth, nodeRect.width - minSplitWidth);
                RectInt _newRectLeft = this.nodeRect;
                _newRectLeft.width = _leftWidth;
                int _rightWidth = nodeRect.width - _leftWidth;
                RectInt _newRectRight = new RectInt(_newRectLeft.x + _newRectLeft.width, nodeRect.y, _rightWidth, nodeRect.height);

                // Create the new child nodes
                tempColor32 = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)255);
                BSPNode newLeftNode = new BSPNode(_newRectLeft, this);
                newLeftNode.nodePosition = Position.LEFT;
                BSPNode newRightNode = new BSPNode(_newRectRight, this);
                newRightNode.nodePosition = Position.RIGHT;
                // Assign new children references in this node
                children[0] = newLeftNode;
                children[1] = newRightNode;
                Debug.Log("------------------------------------------------------------------------------\nPerformed horizontal split - new child nodes:");
                Debug.Log(newLeftNode.nodeRect.ToString());
                Debug.Log(newRightNode.nodeRect.ToString());
                Debug.Log("------------------------------------------------------------------------------");
                splitDirection = Direction.HORIZONTAL;
                return true; // node was able to be split
            }
            else if (CanBeSplitVertically())
            { // Split vertically
                // Calculate the RectInts for the new child cells
                int _topHeight = Random.Range(minSplitHeight, nodeRect.height - minSplitHeight);
                RectInt _newRectTop = this.nodeRect;
                _newRectTop.height = _topHeight;

                int _btmHeight = nodeRect.height - _topHeight;
                RectInt _newRectBtm = new RectInt(nodeRect.x,
                                                  nodeRect.y + _newRectTop.height,
                                                  nodeRect.width,
                                                  nodeRect.height - _topHeight);
                // Create the new child nodes
                tempColor32 = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)255);
                BSPNode newTopNode = new BSPNode(_newRectTop, this);
                newTopNode.nodePosition = Position.TOP;
                BSPNode newBtmNode = new BSPNode(_newRectBtm, this);
                newBtmNode.nodePosition = Position.BOTTOM;

                // Assign new children references in this node
                children[0] = newTopNode;
                children[1] = newBtmNode;
                Debug.Log("------------------------------------------------------------------------------\nPerformed vertical split - new child nodes:");
                Debug.Log(newTopNode.nodeRect.ToString());
                Debug.Log(newBtmNode.nodeRect.ToString());
                Debug.Log("------------------------------------------------------------------------------");
                splitDirection = Direction.VERTICAL;
                return true; // node was able to be split
            }
            else return false; // could not be split (too small)
        }

        bool CanBeSplitHorizontally()
        {
            //Debug.Log("CanBeSplitHoriz: nodeRect.width = " + nodeRect.width.ToString() + " 2*minSplitWidth = " + (2 * minSplitWidth).ToString());
            return (nodeRect.width >= 2 * minSplitWidth) && !TooShortToSplitHorizontally();
        }
        bool CanBeSplitVertically()
        {
            //Debug.Log("CanBeSplitVert: nodeRect.height = " + nodeRect.height.ToString() + " 2*minSplitHeight = " + (2 * minSplitHeight).ToString());
            return (nodeRect.height >= 2 * minSplitHeight) && !TooNarrowToSplitVertically();
        }
        /// <summary>
        /// RecursiveSplit - Splits the passed node, then the two generated child nodes, and so on, recursively
        /// </summary>
        /// <param name="node">The node where the recursion should begin</param>
        public void RecursiveSplit(BSPNode node)
        {
            bool _couldBeSplit = node.Split();
            if (_couldBeSplit)
            {
                RecursiveSplit(node.children[0]);
                RecursiveSplit(node.children[1]);
            }
        }

        public bool IsLeaf()
        {
            return (children[0] == null && children[1] == null);
        }

        public bool HasTwoLeaves()
        {
            return (children[0] != null && children[1] != null);
        }

        /// This function is a check on whether the current node is considered too narrow (compared to its
        /// height) to be split vertically, or too short (compared to its width) to be split horizontally
        bool TooNarrowToSplitVertically()
        {
            return (nodeRect.height / nodeRect.width > 5);
        }
        /// This function is a check on whether the current node is considered too narrow (compared to its
        /// height) to be split vertically, or too short (compared to its width) to be split horizontally
        bool TooShortToSplitHorizontally()
        {
            return (nodeRect.width / nodeRect.height > 5);
        }

        void DrawRect(RectInt _rectToDraw, RectInt _textureRect, Color32 _color)
        {
            //
            // Draw horiztonal lines
            //
            Color32[] pixelsHoriz = new Color32[_rectToDraw.width];
            pixelsHoriz = Enumerable.Repeat(_color, _rectToDraw.width).ToArray();

            // Get index into 1-D array Color32Data, for top line
            int startIndex = _textureRect.width * _rectToDraw.y + _rectToDraw.x;

            // Draw top horiztonal line
            Array.Copy(pixelsHoriz, 0, colorData, startIndex, _rectToDraw.width);

            // Get index into 1-D array Color32Data, for btm line
            startIndex = _textureRect.width * (_rectToDraw.y + _rectToDraw.height - 1) + _rectToDraw.x;

            // Draw btm horiztonal line
            Array.Copy(pixelsHoriz, 0, colorData, startIndex, _rectToDraw.width);

            //
            // Draw vertical lines
            //
            for (int row = 0; row < _rectToDraw.height; row++)
            {
                // Get index of point in left hand line
                int index = (_rectToDraw.y + row) * _textureRect.width + _rectToDraw.x;

                // Set pixel in left hand line
                colorData[index] = _color;

                // Get index of point in right hand line
                index = (_rectToDraw.y + row) * _textureRect.width + _rectToDraw.x + _rectToDraw.width - 1;

                // Set pixel in right hand line
                colorData[index] = _color; 
            }
        }

        // Draw (to our debugging texture) the rectangle for the Room that this node contains
        public void DrawRoomRect()
        {
            DrawRect(room.RoomRect, colorDataRect, room.color);
        }

        public void DrawDebugInfo(RectInt _rectToDraw, RectInt _textureRect, Color32 _color)
        {
            string dimensionsStr = _rectToDraw.width.ToString();
            Color32[] stringPixels = new Color32[dimensionsStr.Length * DrawTextToTexture2D.pixelsPerChar];
            DrawTextToTexture2D textDraw = GameObject.Find("DrawText").GetComponent<DrawTextToTexture2D>();
            stringPixels = textDraw.GetPixelsForString(dimensionsStr);
            RectInt stringRect = new RectInt(0, 0, dimensionsStr.Length * charWidth, charHeight);
            Texture2D stringTexture = new Texture2D(stringRect.width, stringRect.height);
            int stringX = _rectToDraw.x + _rectToDraw.width / 2 - stringRect.width / 2;
            int stringY = _rectToDraw.y + _rectToDraw.height / 2 - stringRect.width / 2 + charHeight + 5;
            Blit(stringPixels, stringRect, colorData, colorDataRect, stringX, stringY);

            dimensionsStr = _rectToDraw.height.ToString();
            stringPixels = new Color32[dimensionsStr.Length * DrawTextToTexture2D.pixelsPerChar];
            textDraw = GameObject.Find("DrawText").GetComponent<DrawTextToTexture2D>();
            stringPixels = textDraw.GetPixelsForString(dimensionsStr);
            stringRect = new RectInt(0, 0, dimensionsStr.Length * charWidth, charHeight);
            stringTexture = new Texture2D(stringRect.width, stringRect.height);
            stringX = _rectToDraw.x + _rectToDraw.width / 2 - stringRect.width / 2;
            stringY = _rectToDraw.y + _rectToDraw.height / 2 - stringRect.width / 2;
            Blit(stringPixels, stringRect, colorData, colorDataRect, stringX, stringY);
        }



        public Color32[] Blit(Color32[] source, RectInt sourceRect, Color32[] dest, RectInt destRect, int x, int y)
        {
            for (int sourceRow = 0; sourceRow < sourceRect.height; sourceRow++)
            {
                int sourceIndex = (sourceRect.height - sourceRow - 1) * sourceRect.width;
                int destIndex = (y + sourceRow) * destRect.width + x;
                System.Array.Copy(source, sourceIndex, dest, destIndex, sourceRect.width);
            }
            return dest;
        }

        public int NodeArea()
        {
            return nodeRect.width * nodeRect.height;
        }

        public void GenerateRoom()
        {

            // The space within the nodeRect, that the room is allowed to occupy, is smaller due to the nodeRect including a belt of nodeBorderSize around its perimeter
            RectInt availableRect = new RectInt
            {
                x = nodeRect.x + nodeBorderSize,
                y = nodeRect.y + nodeBorderSize,
                width = nodeRect.width - (2 * nodeBorderSize),
                height = nodeRect.height - (2 * nodeBorderSize)
            };

            // Randomly choose dimensions for the room, within allowed range
            float widthFraction = Random.Range(Room.minWidthFraction, Room.maxWidthFraction);
            float heightFraction = Random.Range(Room.minHeightFraction, Room.maxHeightFraction);
            int roomWidth = (int)(availableRect.width * widthFraction);
            int roomHeight = (int)(availableRect.height * heightFraction);

            int roomOriginMaxX = nodeRect.x + nodeRect.width - roomWidth;
            int roomOriginMaxY = nodeRect.y + nodeRect.height - roomHeight;

            int roomOriginX = Random.Range(availableRect.x, roomOriginMaxX + 1);
            int roomOriginY = Random.Range(availableRect.y, roomOriginMaxY + 1);

            // Create the room
            this.room = new Room(new RectInt(roomOriginX, roomOriginY, roomWidth, roomHeight), this);
        }
    }
}
