using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.BSP;

namespace Assets.Scripts
{
    class Room
    {
        public static float minWidthFraction = 0.3f;
        public static float maxWidthFraction = 0.9f;
        public static float minHeightFraction = 0.3f;
        public static float maxHeightFraction = 0.9f;
        private RectInt roomRect;
        private Vector2Int center;
        public RectInt RoomRect { get => roomRect; set => roomRect = value; }
        public Color color = Color.white;
        public Vector2Int Center { get => center; set => center = value; }
        private BSPNode containingNode;
        public BSPNode ContainingNode { get => containingNode; set => containingNode = value; }
        // Constructor
        public Room(RectInt _roomRect, BSPNode _containingNode)
        {
            this.roomRect = _roomRect;
            this.containingNode = _containingNode;
            center = new Vector2Int(roomRect.x + roomRect.width / 2, roomRect.y + roomRect.height / 2);
        }
        
    }
}
