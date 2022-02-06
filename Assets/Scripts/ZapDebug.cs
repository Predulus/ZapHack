using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class ZapDebug
    {
        static private bool on = true;
        static private bool displayNodes = true;
        static private bool displayRooms = true;
        static private bool displayDebugInfo = false;
        static private bool displayLeafCount = false;
        public static bool On { get => on; set => on = value; }
        public static bool DisplayNodes { get => displayNodes; set => displayNodes = value; }
        public static bool DisplayRooms { get => displayRooms; set => displayRooms = value; }
        public static bool DisplayDebugInfo { get => displayDebugInfo; set => displayDebugInfo = value; }
        public static bool DisplayLeafCount { get => displayLeafCount; set => displayLeafCount = value; }
    }
}
