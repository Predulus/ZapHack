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
    class BSPTree
    {
        bool hasBadNodes = false;
        BSPNode rootNode;
        List<BSPNode> leaves;
        List<BSPNode> twigs;

        private int largestNodeArea = 0;
        private List<List<Vector2Int>> corridors;

        public List<List<Vector2Int>> Corridors { get => corridors; set => corridors = value; }

        static int maxLeaves = 16;
        static int minLeaves = 9;
        public bool HasBadNodes { get => hasBadNodes; }
        public BSPTree(BSPNode _rootNode)
        {
            leaves = new List<BSPNode>();
            // Set the root node for this tree
            this.rootNode = _rootNode;
            rootNode.RecursiveSplit(rootNode);
            TraverseCheckForBadNodes(rootNode);
            if (!hasBadNodes)
            {

                // Construct a List of leaves
                GetLeaves(_rootNode);

                FindLargestLeaf();

                // Get rid of excess leaves
                Prune();

                // Construct a List of twigs (a twig is a parent node of a leaf)
                GetTwigs();

                // Create a room within each leaf
                foreach (BSPNode leaf in leaves)
                {
                    leaf.GenerateRoom();
                    // Draw the room if debugging setting is turned on
                    if (ZapDebug.DisplayRooms)
                        leaf.DrawRoomRect();
                }

                // Create corridors between pairs of leaves
                corridors = new List<List<Vector2Int>>();
                foreach(BSPNode twig in twigs)
                {
                    if (twig.HasTwoLeaves())
                    {
                        List<Vector2Int> corridor;
                        Room[] rooms = new Room[2];
                        rooms[0] = twig.Children[0].Room;
                        rooms[1] = twig.Children[1].Room;
                        if (rooms[0] != null && rooms[1] != null)
                        {
                            corridor = MakeSimpleCorridor(rooms);
                            corridors.Add(corridor);
                        }
                    }
                }

                // If debugging, draw leaf dimensions in each leaf
                if (ZapDebug.On)
                {
                    foreach (BSPNode node in leaves)
                    {
                        if (ZapDebug.DisplayDebugInfo)
                            node.DrawDebugInfo(node.NodeRect, BSPNode.textureRect, (Color32)Color.white);
                    }
                }
                // If debugging, draw the number of leaves
                if (ZapDebug.On && ZapDebug.DisplayLeafCount)
                {
                    DrawTextToTexture2D textDraw = GameObject.Find("DrawText").GetComponent<DrawTextToTexture2D>();
                    string drawString = "Leaves." + leaves.Count.ToString();
                    Color32[] stringPixels = textDraw.GetPixelsForString(drawString);
                    RectInt stringRect = new RectInt(0, 0, drawString.Length * BSPNode.charWidth, BSPNode.charHeight);
                    Texture2D stringTexture = new Texture2D(stringRect.width, stringRect.height);
                    int stringX = 5;
                    int stringY = 5;
                    rootNode.Blit(stringPixels, stringRect, BSPNode.colorData, BSPNode.colorDataRect, stringX, stringY);
                }
            }
        }
        public void GetLeaves(BSPNode _node)
        {
            if (_node.IsLeaf())
            {
                leaves.Add(_node);
            }
            else
            {
                // recurse deeper
                if (_node.Children[0] != null) GetLeaves(_node.Children[0]);
                if (_node.Children[1] != null) GetLeaves(_node.Children[1]);
            }
        }

        public bool TwigsContainNode(BSPNode _node)
        {
            foreach(BSPNode twig in twigs)
            {
                if (twig.ID == _node.ID)
                    return true;
            }
            return false;
        }
        public void GetTwigs()
        {
            twigs = new List<BSPNode>();
            foreach (BSPNode leaf in leaves)
            {
                BSPNode twig = leaf.ParentNode;
                if (twig == null) 
                    Debug.Log("GetTwigs: parent node of leaf was null!");
//                else if (twig.HasTwoLeaves() && !TwigsContainNode(twig))
                else if (!TwigsContainNode(twig))
                            twigs.Add(twig);
            }
        }
        public void TraverseCheckForBadNodes(BSPNode _node)
        {
            CheckIfNodeIsBad(_node);
            if (hasBadNodes) 
                return; // if we have a bad node, no further traversal needed, we can reject this tree
            if (_node.Children[0] != null) TraverseCheckForBadNodes(_node.Children[0]);
            if (_node.Children[1] != null) TraverseCheckForBadNodes(_node.Children[1]);
        }

        public void CheckIfNodeIsBad(BSPNode _node)
        {
            if (_node.IsLeaf() && NodeTooLong(_node.NodeRect.width, _node.NodeRect.height))
                hasBadNodes = true;
        }

        public bool NodeTooLong(int side1, int side2)
        {
            float longer, shorter;
            // Find the longer and shorter sides
            if (side1 > side2)
            {
                longer = side1; shorter = side2;
            }
            else
            {
                longer = side2; shorter = side1;
            }
            Debug.Log("Testing for bad node: ratio is " + (longer / shorter).ToString());
            if ((longer / shorter) > 2f)
                return true;
            else
                return false;
        }

        public void FindLargestLeaf()
        {
            foreach (BSPNode leaf in leaves)
            {
                if (leaf.NodeArea() > largestNodeArea)
                    largestNodeArea = leaf.NodeArea();
            }

            // Set largestNode flag to true in the largest node, false in others
            foreach (BSPNode leaf in leaves)
            {
                if (leaf.NodeArea() == largestNodeArea)
                    leaf.IsLargest = true;
                else
                    leaf.IsLargest = false;
            }
        }

        public void Prune()
        {
            int targetNumLeaves = Random.Range(minLeaves, maxLeaves + 1);
            // Don't trim, if the number of leaves we have is less than or equal to the target number
            if (leaves.Count <= targetNumLeaves)
                return;
            while (leaves.Count > targetNumLeaves)
            {
                TrimLeaf();
            }
            TrimLeaf();
            TrimLeaf();
        }

        public void TrimLeaf()
        {
            // Pick a leaf to trim, at random
            int indexOfLeafToTrim = Random.Range(0, leaves.Count);
            // Don't trim the largest one, if we got the index of the largest one, try again
            if (!leaves[indexOfLeafToTrim].IsLargest)
            {
                if (leaves[indexOfLeafToTrim].ParentNode.Children[0] == leaves[indexOfLeafToTrim])
                    leaves[indexOfLeafToTrim].ParentNode.Children[0] = null;
                if (leaves[indexOfLeafToTrim].ParentNode.Children[1] == leaves[indexOfLeafToTrim])
                    leaves[indexOfLeafToTrim].ParentNode.Children[1] = null;
                leaves[indexOfLeafToTrim] = null;
                leaves.RemoveAt(indexOfLeafToTrim);
            }
            else
                TrimLeaf();
        }

        public List<Vector2Int> MakeSimpleCorridor(Room[] rooms)
        {
            int currentX, currentY, deltaX, deltaY, deltaYSign, deltaXSign;
            int absDeltaX, absDeltaY;
            int moveLength;

            int startX, startY, endX, endY;

            List<Vector2Int> corridorPoints = new List<Vector2Int>();

            if (rooms[0].ContainingNode.SplitDirection == Direction.HORIZONTAL)
            {
                if (rooms[0].Center.x < rooms[1].Center.x)
                {
                    startX = rooms[0].RoomRect.x + rooms[0].RoomRect.width;
                    endX = rooms[1].RoomRect.x;
                    startY = Random.Range(rooms[0].RoomRect.y, rooms[0].RoomRect.y + rooms[0].RoomRect.height);
                    endY = Random.Range(rooms[1].RoomRect.y, rooms[1].RoomRect.y + rooms[1].RoomRect.height);
                }
                else
                {
                    startX = rooms[1].RoomRect.x + rooms[1].RoomRect.width;
                    endX = rooms[0].RoomRect.x;
                    startY = Random.Range(rooms[1].RoomRect.y, rooms[1].RoomRect.y + rooms[1].RoomRect.height);
                    endY = Random.Range(rooms[0].RoomRect.y, rooms[0].RoomRect.y + rooms[0].RoomRect.height);
                }
                currentX = startX;
                currentY = startY;
                AddCorridorPoint(corridorPoints, currentX, currentY);

                currentX++;
                AddCorridorPoint(corridorPoints, currentX, currentY);

                deltaY = endY - startY;
                absDeltaY = Math.Abs(deltaY);
                deltaYSign = deltaY / (absDeltaY);

                deltaX = endX - startX;
                absDeltaX = Math.Abs(deltaX);
                deltaXSign = deltaX / (absDeltaX);

                while (!(currentX < endX && (currentY * deltaYSign) <= (endY * deltaYSign)))
                {
                    // Random horizontal or diagonal
                    if (Random.Range(0, 2) == 0)
                    { // Move horizontally
                        moveLength = Random.Range(1, endX - currentX);
                        for (int i = 0; i < moveLength; i++)
                        {
                            currentX++;
                            AddCorridorPoint(corridorPoints, currentX, currentY);
                        }
                    }
                    else
                    { // Move diagonally
                        moveLength = Random.Range(1, endX - currentX);
                        for (int i = 0; i < moveLength; i++)
                        {
                            currentX++;
                            currentY += deltaY;
                            AddCorridorPoint(corridorPoints, currentX, currentY);
                        }
                    }
                }
            }
            else  // rooms[0].ContainingNode.SplitDirection == Direction.VERTICAL
            {
                if (rooms[0].Center.y < rooms[1].Center.y)  
                {
                    startY = rooms[0].RoomRect.y + rooms[0].RoomRect.height;
                    endY = rooms[1].RoomRect.y;
                    startX = Random.Range(rooms[0].RoomRect.x, rooms[0].RoomRect.x + rooms[0].RoomRect.width);
                    endX = Random.Range(rooms[1].RoomRect.x, rooms[1].RoomRect.x + rooms[1].RoomRect.width);
                }
                else
                {
                    startY = rooms[1].RoomRect.y + rooms[1].RoomRect.height;
                    endY = rooms[0].RoomRect.y;
                    startX = Random.Range(rooms[1].RoomRect.x, rooms[1].RoomRect.x + rooms[1].RoomRect.width);
                    endX = Random.Range(rooms[0].RoomRect.x, rooms[0].RoomRect.x + rooms[0].RoomRect.width);
                }
                currentX = startX;
                currentY = startY;
                AddCorridorPoint(corridorPoints, currentX, currentY);

                currentY++;
                AddCorridorPoint(corridorPoints, currentX, currentY);

                deltaY = endY - startY;
                absDeltaY = Math.Abs(deltaY);
                if (deltaY == 0)
                {
                    deltaYSign = 0;
                }
                else
                {
                    deltaYSign = deltaY / (absDeltaY);
                }


                deltaX = endX - startX;
                absDeltaX = Math.Abs(deltaX);
                if (deltaX == 0)
                {
                    deltaXSign = 0;
                }
                else
                {
                    deltaXSign = deltaX / (absDeltaX);
                }
//                while (currentY < endY && (currentX * deltaXSign) <= (endX * deltaXSign))
                    while ((currentY < endY) && ((currentX * deltaXSign) <= (endX * deltaXSign)))
                    {
                        // Random vertical or diagonal
                        if (Random.Range(0, 2) == 0)
                    { // Move vertically
                        moveLength = Random.Range(1, endY - currentY);
                        for (int i = 0; i < moveLength; i++)
                        {
                            currentY++;
                            AddCorridorPoint(corridorPoints, currentX, currentY);
                        }
                    }
                    else
                    { // Move diagonally
                        moveLength = Random.Range(1, endY - currentY);
                        for (int i = 0; i < moveLength; i++)
                        {
                            currentX += deltaX;
                            currentY++;
                            AddCorridorPoint(corridorPoints, currentX, currentY);
                        }
                    }
                }

            }
            return corridorPoints;
        }

        public void AddCorridorPoint(List<Vector2Int> _points, int _x, int _y)
        {
            _points.Add(new Vector2Int(_x, _y));
        }
    }
}
