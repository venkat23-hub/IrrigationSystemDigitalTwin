using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AlignPlantsGrid : EditorWindow
{
    private static Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    [MenuItem("Tools/Align 256 Strawberry Plants (16x16 Grid)")]
    static void AlignInGrid()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length != 256)
        {
            Debug.LogWarning("Please select exactly 256 plant objects.");
            return;
        }

        // Save original positions
        originalPositions.Clear();
        foreach (GameObject obj in selected)
        {
            originalPositions[obj] = obj.transform.position;
            Undo.RecordObject(obj.transform, "Align Plants in Grid");
        }

        int columns = 16;
        int rows = 16;

        float terrainWidth = 68f;
        float terrainLength = 68f;
        float spacingX = terrainWidth / (columns - 1);  // 4.27 m
        float spacingZ = terrainLength / (rows - 1);    // 4.27 m

        // Bottom-left corner of the terrain as start
        Vector3 startPos = new Vector3(0f, 0f, 0f);

        // Optional: center the field
        Terrain terrain = Terrain.activeTerrain;
        if (terrain)
        {
            startPos = terrain.transform.position;
        }

        // Sort selection for consistent order
        System.Array.Sort(selected, (a, b) => a.name.CompareTo(b.name));

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int index = r * columns + c;
                if (index >= selected.Length) break;

                Vector3 newPos = startPos + new Vector3(c * spacingX, 0f, r * spacingZ);

                // Adjust to terrain height
                if (terrain)
                {
                    float height = terrain.SampleHeight(newPos);
                    newPos.y = height;
                }

                selected[index].transform.position = newPos;
            }
        }

        Debug.Log($"✅ Aligned 256 strawberry plants in a 16×16 grid across {terrainWidth}×{terrainLength} m terrain!");
    }

    [MenuItem("Tools/Restore Original Plant Positions")]
    static void RestoreOriginalPositions()
    {
        if (originalPositions.Count == 0)
        {
            Debug.LogWarning("No saved positions found! Align first before restoring.");
            return;
        }

        foreach (var kvp in originalPositions)
        {
            if (kvp.Key != null)
            {
                Undo.RecordObject(kvp.Key.transform, "Restore Plant Positions");
                kvp.Key.transform.position = kvp.Value;
            }
        }

        Debug.Log($"♻️ Restored {originalPositions.Count} plants to their original positions!");
        originalPositions.Clear();
    }
}
