using UnityEngine;
using UnityEditor;

public class CreateIrrigationGrid : EditorWindow
{
    private Terrain terrain;
    private int gridSize = 16;               // For 16x16 plants
    private float irrigationWidth = 0.03f;   // Width between rows (normalized)
    private float depth = 0.005f;            // How much to lower terrain
    private float opacity = 0.5f;            // Similar to brush opacity (0–1)

    [MenuItem("Tools/Create Irrigation Grid")]
    public static void ShowWindow()
    {
        GetWindow<CreateIrrigationGrid>("Irrigation Grid Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("💧 Irrigation Grid Generator", EditorStyles.boldLabel);
        GUILayout.Space(5);

        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        gridSize = EditorGUILayout.IntSlider("Grid Size", gridSize, 4, 64);
        irrigationWidth = EditorGUILayout.Slider("Irrigation Width", irrigationWidth, 0.01f, 0.2f);
        depth = EditorGUILayout.Slider("Dig Depth", depth, 0.001f, 0.02f);
        opacity = EditorGUILayout.Slider("Opacity", opacity, 0.1f, 1.0f);

        GUILayout.Space(10);

        if (GUILayout.Button("🪓 Create Irrigation Grid"))
        {
            if (terrain == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a Terrain!", "OK");
                return;
            }
            CreateGridIrrigation();
        }
    }

    private void CreateGridIrrigation()
    {
        TerrainData terrainData = terrain.terrainData;
        int heightmapRes = terrainData.heightmapResolution;

        float[,] heights = terrainData.GetHeights(0, 0, heightmapRes, heightmapRes);

        int step = heightmapRes / gridSize;
        float lowerAmount = depth * opacity;

        for (int x = 0; x < heightmapRes; x++)
        {
            for (int z = 0; z < heightmapRes; z++)
            {
                bool betweenRow = (x % step < step * irrigationWidth);
                bool betweenCol = (z % step < step * irrigationWidth);

                if (betweenRow || betweenCol)
                {
                    heights[z, x] -= lowerAmount;
                    if (heights[z, x] < 0f) heights[z, x] = 0f;
                }
            }
        }

        Undo.RegisterCompleteObjectUndo(terrainData, "Create Irrigation Grid");
        terrainData.SetHeights(0, 0, heights);

        Debug.Log($"✅ Irrigation grid created with {gridSize}x{gridSize} pattern!");
    }
}
