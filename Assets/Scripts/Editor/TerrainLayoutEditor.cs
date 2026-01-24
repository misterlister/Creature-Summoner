#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static GameConstants;

[CustomEditor(typeof(TerrainLayout))]
public class TerrainLayoutEditor : Editor
{
    private const float CELL_SIZE = 80f;
    private const float CELL_SPACING = 4f;
    private const float LABEL_WIDTH = 60f;
    private const float LABEL_HEIGHT = 20f;

    public override void OnInspectorGUI()
    {
        TerrainLayout layout = (TerrainLayout)target;
        serializedObject.Update();

        EditorGUILayout.Space(10);

        // === BIOME SECTION ===
        EditorGUILayout.LabelField("Biome Configuration", EditorStyles.boldLabel);

        var biomeProp = serializedObject.FindProperty("LayoutBiome");
        EditorGUILayout.PropertyField(biomeProp);

        if (layout.LayoutBiome != null)
        {
            EditorGUILayout.HelpBox(
                $"Biome: {layout.LayoutBiome.BiomeName}\n" +
                $"Only terrain types valid for this biome will be used.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No biome assigned. Set a biome to filter available terrain types.",
                MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        // === GRID SECTION ===
        EditorGUILayout.LabelField("Terrain Grid Layout", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Grid displayed left-to-right as it appears in battle.\n" +
            "Front Line (left) faces the enemy center line.\n" +
            "Select terrain types from the dropdown.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        DrawGrid(layout);

        EditorGUILayout.Space(10);

        // === UTILITY BUTTONS ===
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog("Clear Terrain",
                "Remove all terrain from this layout?", "Yes", "Cancel"))
            {
                Undo.RecordObject(layout, "Clear Terrain Layout");
                layout.Clear();
                EditorUtility.SetDirty(layout);
            }
        }

        if (GUILayout.Button("Randomize"))
        {
            Undo.RecordObject(layout, "Randomize Terrain Layout");
            RandomizeTerrain(layout);
            EditorUtility.SetDirty(layout);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGrid(TerrainLayout layout)
    {
        var frontLineProp = serializedObject.FindProperty("frontLine");
        var middleLineProp = serializedObject.FindProperty("middleLine");
        var backLineProp = serializedObject.FindProperty("backLine");

        // Draw battlefield orientation indicator
        DrawBattlefieldIndicator();

        EditorGUILayout.Space(5);

        // Column headers with visual separator
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(LABEL_WIDTH);

        DrawColumnHeader("FRONT LINE\n(Col 0)", new Color(0.8f, 0.3f, 0.3f));
        DrawColumnHeader("MIDDLE LINE\n(Col 1)", new Color(0.8f, 0.6f, 0.3f));
        DrawColumnHeader("BACK LINE\n(Col 2)", new Color(0.3f, 0.6f, 0.8f));

        EditorGUILayout.EndHorizontal();

        // Draw each row horizontally (left to right)
        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            DrawGridRow(row, frontLineProp, middleLineProp, backLineProp, layout);
        }
    }

    private void DrawBattlefieldIndicator()
    {
        var rect = GUILayoutUtility.GetRect(0, 30);
        rect.width = EditorGUIUtility.currentViewWidth - 40;
        rect.x = 20;

        // Draw arrow showing battle flow
        var arrowStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };

        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * 0.4f, rect.height),
            new Color(0.2f, 0.4f, 0.8f, 0.2f));
        GUI.Label(new Rect(rect.x, rect.y, rect.width * 0.4f, rect.height),
            "PLAYER SIDE", arrowStyle);

        EditorGUI.DrawRect(new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, rect.height),
            new Color(0.8f, 0.3f, 0.3f, 0.2f));
        GUI.Label(new Rect(rect.x + rect.width * 0.6f, rect.y, rect.width * 0.4f, rect.height),
            "ENEMY SIDE", arrowStyle);

        // Center line
        var centerX = rect.x + rect.width * 0.5f;
        Handles.color = Color.yellow;
        Handles.DrawLine(
            new Vector3(centerX, rect.y),
            new Vector3(centerX, rect.y + rect.height));

        // Arrow showing direction
        var arrowY = rect.y + rect.height + 5;
        Handles.color = Color.white;
        Handles.DrawLine(
            new Vector3(rect.x, arrowY),
            new Vector3(rect.x + rect.width, arrowY));
        DrawArrowHead(new Vector2(rect.x + rect.width, arrowY), Vector2.right);
    }

    private void DrawArrowHead(Vector2 tip, Vector2 direction)
    {
        var perpendicular = new Vector2(-direction.y, direction.x);
        var back = tip - direction * 8;
        var side1 = back + perpendicular * 4;
        var side2 = back - perpendicular * 4;

        Handles.DrawLine(tip, side1);
        Handles.DrawLine(tip, side2);
    }

    private void DrawColumnHeader(string label, Color tintColor)
    {
        var headerRect = GUILayoutUtility.GetRect(CELL_SIZE, LABEL_HEIGHT * 2);

        EditorGUI.DrawRect(headerRect, tintColor * 0.3f);

        var style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            normal = { textColor = Color.white }
        };

        EditorGUI.LabelField(headerRect, label, style);
        GUILayout.Space(CELL_SPACING);
    }

    private void DrawGridRow(int row, SerializedProperty frontProp,
                            SerializedProperty middleProp, SerializedProperty backProp,
                            TerrainLayout layout)
    {
        EditorGUILayout.BeginHorizontal();

        // Row label on left
        var rowLabel = row switch
        {
            0 => "Top",
            1 => "Mid",
            2 => "Bot",
            _ => row.ToString()
        };

        var labelStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight,
            fontStyle = FontStyle.Bold
        };
        GUILayout.Label($"{rowLabel} (Row {row})", labelStyle, GUILayout.Width(LABEL_WIDTH));

        // Draw cells left to right: Front -> Middle -> Back
        DrawTerrainCell(frontProp.FindPropertyRelative(GetRowPropertyName(row)),
            row, 0, new Color(0.8f, 0.3f, 0.3f), layout);

        DrawTerrainCell(middleProp.FindPropertyRelative(GetRowPropertyName(row)),
            row, 1, new Color(0.8f, 0.6f, 0.3f), layout);

        DrawTerrainCell(backProp.FindPropertyRelative(GetRowPropertyName(row)),
            row, 2, new Color(0.3f, 0.6f, 0.8f), layout);

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(CELL_SPACING);
    }

    private string GetRowPropertyName(int row)
    {
        return row switch
        {
            0 => "Top",
            1 => "Middle",
            2 => "Bottom",
            _ => ""
        };
    }

    private void DrawTerrainCell(SerializedProperty terrainEnumProp, int row, int col,
                                 Color tintColor, TerrainLayout layout)
    {
        var cellRect = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE);

        // Draw cell background with column color tint
        var bgColor = tintColor * 0.2f;
        EditorGUI.DrawRect(cellRect, bgColor);

        // Draw border
        var borderColor = tintColor * 0.6f;
        Handles.color = borderColor;
        Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, borderColor);

        // Enum popup for terrain selection
        var contentRect = new Rect(
            cellRect.x + 4,
            cellRect.y + 20,
            cellRect.width - 8,
            EditorGUIUtility.singleLineHeight);

        EditorGUI.BeginChangeCheck();
        var currentTerrain = (TerrainTypeEnum)terrainEnumProp.intValue;
        var newTerrain = (TerrainTypeEnum)EditorGUI.EnumPopup(contentRect, currentTerrain);

        if (EditorGUI.EndChangeCheck())
        {
            terrainEnumProp.intValue = (int)newTerrain;
        }

        // Draw position label in corner
        var posLabel = $"({row},{col})";
        var labelRect = new Rect(cellRect.x + 2, cellRect.y + 2, 30, 15);
        var labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = new Color(1, 1, 1, 0.6f) },
            fontSize = 8
        };
        EditorGUI.LabelField(labelRect, posLabel, labelStyle);

        // Show terrain name at bottom
        var nameRect = new Rect(
            cellRect.x + 4,
            cellRect.y + cellRect.height - 18,
            cellRect.width - 8,
            16);

        var nameStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.LowerCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 9,
            normal = { textColor = Color.white }
        };

        var displayName = currentTerrain.ToString();
        if (displayName.Length > 10)
            displayName = displayName.Substring(0, 10);

        EditorGUI.LabelField(nameRect, displayName, nameStyle);

        GUILayout.Space(CELL_SPACING);
    }

    private void RandomizeTerrain(TerrainLayout layout)
    {
        // Get valid terrain types based on biome
        List<TerrainTypeEnum> validTerrains = new List<TerrainTypeEnum>();

        if (layout.LayoutBiome != null)
        {
            // Get all terrain types and filter by biome
            var allTypes = System.Enum.GetValues(typeof(TerrainTypeEnum));

            foreach (TerrainTypeEnum terrainEnum in allTypes)
            {
                var terrainInstance = terrainEnum.GetTerrainInstance();
                if (terrainInstance != null && layout.LayoutBiome.IsTerrainValid(terrainInstance.GetType()))
                {
                    validTerrains.Add(terrainEnum);
                }
            }

            if (validTerrains.Count == 0)
            {
                EditorUtility.DisplayDialog("No Valid Terrains",
                    $"No terrain types are valid for biome '{layout.LayoutBiome.BiomeName}'.\n\n" +
                    "Make sure the biome has terrain variants configured.",
                    "OK");
                return;
            }
        }
        else
        {
            // No biome - use all terrain types
            var allTypes = System.Enum.GetValues(typeof(TerrainTypeEnum));
            foreach (TerrainTypeEnum terrainEnum in allTypes)
            {
                validTerrains.Add(terrainEnum);
            }
        }

        // Randomize with filtered list
        for (int col = 0; col < BATTLE_COLS; col++)
        {
            for (int row = 0; row < BATTLE_ROWS; row++)
            {
                // 50% chance to use a random terrain, otherwise keep as Regular
                if (Random.value > 0.5f && validTerrains.Count > 1)
                {
                    // Filter out Regular for variety
                    var nonRegular = validTerrains.Where(t => t != TerrainTypeEnum.Regular).ToList();
                    if (nonRegular.Count > 0)
                    {
                        var randomTerrain = nonRegular[Random.Range(0, nonRegular.Count)];
                        layout.SetTerrain(new GridPosition(row, col), randomTerrain);
                    }
                }
                else
                {
                    layout.SetTerrain(new GridPosition(row, col), TerrainTypeEnum.Regular);
                }
            }
        }
    }
}
#endif