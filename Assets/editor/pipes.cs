using UnityEngine;
using UnityEditor;
using System.Linq;

public class PlaceRowPipesBetweenPlants : EditorWindow
{
    public GameObject pipePrefab;         // The pipe model prefab to use
    public Transform plantsParent;        // Parent object containing all plant objects
    public Transform pipesParent;         // Parent object to hold the placed pipes (optional)
    public int rows = 16;                 // Number of rows of plants
    public float terrainWidth = 68f;      // Field width in X
    public float terrainLength = 68f;     // Field length in Z
    public float heightOffset = 0.1f;     // Height above terrain

    [MenuItem("Tools/Place Pipes Between Rows")]
    public static void ShowWindow()
    {
        GetWindow<PlaceRowPipesBetweenPlants>("Pipe Placement Between Rows");
    }

    private void OnGUI()
    {
        GUILayout.Label("Place Pipes Between Plant Rows", EditorStyles.boldLabel);
        pipePrefab = (GameObject)EditorGUILayout.ObjectField("Pipe Prefab", pipePrefab, typeof(GameObject), false);
        plantsParent = (Transform)EditorGUILayout.ObjectField("Plants Parent", plantsParent, typeof(Transform), true);
        pipesParent = (Transform)EditorGUILayout.ObjectField("Pipes Parent (optional)", pipesParent, typeof(Transform), true);

        rows = EditorGUILayout.IntField("Number of Rows", rows);
        terrainWidth = EditorGUILayout.FloatField("Terrain Width (X)", terrainWidth);
        terrainLength = EditorGUILayout.FloatField("Terrain Length (Z)", terrainLength);
        heightOffset = EditorGUILayout.FloatField("Pipe Height Offset", heightOffset);

        if (GUILayout.Button("Place Pipes Between Rows"))
        {
            PlacePipes();
        }
    }

    private void PlacePipes()
    {
        if (pipePrefab == null)
        {
            Debug.LogError("Please assign the pipe prefab.");
            return;
        }
        if (plantsParent == null)
        {
            Debug.LogError("Please assign the Plants Parent transform.");
            return;
        }

        // If no pipes parent, create one
        if (pipesParent == null)
        {
            GameObject newParent = new GameObject("PipesBetweenRows");
            pipesParent = newParent.transform;
        }

        // Remove existing pipes under pipesParent
        for (int i = pipesParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(pipesParent.GetChild(i).gameObject);
        }

        // Calculate spacing between rows
        float rowSpacing = terrainLength / (rows - 1);
        float startZ = -terrainLength / 2f;
        float xCenter = 0f; // Place in middle X (you can adjust if needed)

        for (int i = 0; i < rows; i++)
        {
            float zPos = startZ + i * rowSpacing;
            float xPos = 0f; // middle
            Vector3 worldPos = new Vector3(xPos, 0f, zPos);

            // Sample terrain height if terrain present
            if (Terrain.activeTerrain)
            {
                Vector3 terrainPos = new Vector3(worldPos.x + Terrain.activeTerrain.transform.position.x,
                                                  0f,
                                                  worldPos.z + Terrain.activeTerrain.transform.position.z);
                float height = Terrain.activeTerrain.SampleHeight(terrainPos);
                worldPos.y = height + heightOffset;
            }
            else
            {
                worldPos.y = heightOffset;
            }

            GameObject pipe = (GameObject)PrefabUtility.InstantiatePrefab(pipePrefab) as GameObject;
            pipe.transform.position = worldPos;
            pipe.transform.SetParent(pipesParent);

            // Rotate pipe so it runs along X axis
            pipe.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Scale pipe so it covers width (terrainWidth)
            Vector3 scale = pipe.transform.localScale;
            scale.x = terrainWidth;
            pipe.transform.localScale = scale;
        }

        Debug.Log($"✅ Placed {rows} pipes between rows.");
    }
}