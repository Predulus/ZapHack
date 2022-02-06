using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Scripts.BSP;
using Random = UnityEngine.Random;


namespace Assets.Scripts
{
    public class Level : MonoBehaviour
    {
        int pngCount = 0;
        private static int numCellsHoriz = BSPNode.numCellsHoriz;
        private static int numCellsVert = BSPNode.numCellsVert;
        Cell[,] levelGrid = new Cell[numCellsHoriz, numCellsVert];
        Texture2D texture;
        Color32[] colorData = new Color32[numCellsHoriz * numCellsVert];
        public Material screenMaterial;
        BSPTree tree;
//        public RenderTexture screenRenderTexture;

        // Use this for initialization
        void Start()
        {
            Random.InitState(123456789);
            BSPNode rootNode;
            Color32 tempColor32 = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)255);
            // If we generate a random draw-color that is black or near-black (which won't be able to be seen on a black bg,
            // just set the draw color to white
            if (tempColor32.r + tempColor32.g + tempColor32.b < 20)
                tempColor32 = (Color32)Color.white;
                BSPNode.tempColor32 = tempColor32;
            bool _hasBadNodes = true;
            while (_hasBadNodes)
            {
                rootNode = new BSPNode(new RectInt(0, 0, numCellsHoriz, numCellsVert));
                texture = new Texture2D(numCellsHoriz, numCellsVert, TextureFormat.RGBA32, false);
                tree = new BSPTree(rootNode);
                if (!tree.HasBadNodes)
                    _hasBadNodes = false;
            }

            colorData = BSPNode.colorData;
            texture.SetPixels32(colorData);
            texture.Apply();
            screenMaterial.mainTexture = (Texture)texture;
        }

        // Update is called once per frame
        void Update()
        {
            // Regen tree on pressing space
            if (Input.GetKeyDown("space"))
            {
                texture = new Texture2D(numCellsHoriz, numCellsVert, TextureFormat.RGBA32, false);
                BSPNode rootNode = new BSPNode(new RectInt(0, 0, numCellsHoriz, numCellsVert));
                Color32 tempColor32 = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)255);
                // If we generate a random draw-color that is black or near-black (which won't be able to be seen on a black bg,
                // just set the draw color to white
                if (tempColor32.r + tempColor32.g + tempColor32.b < 20)
                    tempColor32 = (Color32)Color.white;
                BSPNode.tempColor32 = tempColor32;
                bool _hasBadNodes = true;
                while (_hasBadNodes)
                {
                    rootNode = new BSPNode(new RectInt(0, 0, numCellsHoriz, numCellsVert));
                    texture = new Texture2D(numCellsHoriz, numCellsVert, TextureFormat.RGBA32, false);
                    tree = new BSPTree(rootNode);
                    if (!tree.HasBadNodes)
                        _hasBadNodes = false;
                }

                colorData = BSPNode.colorData;
                texture.SetPixels32(colorData);
                texture.Apply();
                screenMaterial.mainTexture = (Texture)texture;
                DrawCorridors();

            }
            // Save texture as png file on pressing s
            if (Input.GetKeyDown("s"))
            {
                SaveTextureAsPNG(texture, "D:\\temp\\grid" + pngCount.ToString() + ".png");
                pngCount++;
            }

        }
        public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
        {
            byte[] _bytes = _texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(_fullPath, _bytes);
            Debug.Log(_bytes.Length / 1024 + "Kb was saved as: " + _fullPath);
        }

        public void DrawCorridorPoints(List<Vector2Int> _points)
        {
            foreach (Vector2Int point in _points)
            {
                texture.SetPixel(point.x, point.y, Color.white);
            }
            texture.Apply();
        }
        public void DrawCorridors()
        {
            foreach (List<Vector2Int> corridor in tree.Corridors)
            {
                DrawCorridorPoints(corridor);
            }
        }
    }

}