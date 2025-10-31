using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PerlinTerrainGenerator : EditorWindow
{
    [System.Serializable]
    public class NoiseLayer
    {
        public float scale = 50f;
        public float amplitude = 10f;
        public float offsetX = 0f;
        public float offsetY = 0f;
    }


    Terrain terrain;
    List<NoiseLayer> layers = new List<NoiseLayer>();
    bool livePreview = true;

    [MenuItem("Tools/Layered Perlin Terrain")]
    public static void ShowWindow()
    {
        GetWindow<PerlinTerrainGenerator>("Layered Perlin Terrain");
    }

    void OnGUI()
    {
        GUILayout.Label("Perlin Terrain Generator", EditorStyles.boldLabel);

        terrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", terrain, typeof(Terrain), true);
        GUILayout.Space(10);

        livePreview = EditorGUILayout.Toggle("Live Preview", livePreview);
        GUILayout.Space(10);

        GUILayout.Label("Noise Layers", EditorStyles.boldLabel);
        for (int i = 0; i < layers.Count; i++)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Layer " + (i + 1), EditorStyles.miniBoldLabel);

            layers[i].scale = EditorGUILayout.Slider("Scale", layers[i].scale, 1f, 500f);
            layers[i].amplitude = EditorGUILayout.Slider("Amplitude", layers[i].amplitude, 0f, 100f);
            layers[i].offsetX = EditorGUILayout.Slider("Offset X", layers[i].offsetX, 0f, 9999f);
            layers[i].offsetY = EditorGUILayout.Slider("Offset Y", layers[i].offsetY, 0f, 9999f);

            if (GUILayout.Button("Remove Layer"))
            {
                layers.RemoveAt(i);
            }

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        if (GUILayout.Button("Add Layer"))
        {
            layers.Add(new NoiseLayer());
        }
        GUILayout.Space(15);

        if (GUILayout.Button("Generate Terrain"))
        {
            if (terrain != null)
            {
                ApplyPerlinNoise();
            }
            else
            {
                Debug.LogWarning("No terrain selected!");
            }
        }

        if (livePreview && terrain != null && Event.current.type == EventType.Repaint)
        {
            ApplyPerlinNoise();
        }
    }

    void ApplyPerlinNoise()
    {
        if (terrain == null || layers.Count == 0)
        {
            return;
        }

        TerrainData data = terrain.terrainData;
        int width = data.heightmapResolution;
        int heightmapWidth = data.heightmapResolution;
        int heightmapHeight = data.heightmapResolution;

        float[,] heights = new float[heightmapWidth, heightmapHeight];

        for (int y = 0; y < heightmapHeight; y++)
        {
            for (int x = 0; x < heightmapWidth; x++)
            {
                float value = 0f;
                foreach (var layer in layers)
                {
                    float xCoord = (float)x / width * layer.scale + layer.offsetX;
                    float yCoord = (float)y / heightmapHeight * layer.scale + layer.offsetY;
                    value += Mathf.PerlinNoise(xCoord, yCoord) * layer.amplitude;
                }
                heights[y, x] = Mathf.Clamp01(value / data.size.y);
            }
        }

        data.SetHeights(0, 0, heights);
    }
}