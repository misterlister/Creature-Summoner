#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static GameConstants;

[CustomEditor(typeof(TerrainLayout))]
public class TerrainLayoutEditor : Editor
{
    private const float CELL_SIZE = 80f;
    private const float CELL_SPACING = 4f;
    private const float LABEL_WIDTH = 110f;
    private const float LABEL_HEIGHT = 20f;

    private bool mirrorRandomization = false;
    private bool allowChasm = false;

    public override void OnInspectorGUI()
    {
        TerrainLayout layout = (TerrainLayout)target;
        serializedObject.Update();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Terrain Grid Layout", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Grid displayed left-to-right as it appears in battle.\n" +
            "Select terrain types from the dropdown.",
            MessageType.Info);

        EditorGUILayout.Space(10);

        DrawGrid(layout);

        EditorGUILayout.Space(12);

        DrawRandomizationOptions();

        EditorGUILayout.Space(10);

        DrawUtilityButtons(layout);

        serializedObject.ApplyModifiedProperties();
    }

    // ================= GRID =================

    private void DrawGrid(TerrainLayout layout)
    {
        DrawBattlefieldIndicator();
        EditorGUILayout.Space(8);

        DrawColumnHeaders();

        for (int row = 0; row < BATTLE_ROWS; row++)
        {
            EditorGUILayout.BeginHorizontal();

            DrawRowLabel(row);

            for (int col = 0; col < BATTLE_COLS; col++)
            {
                DrawTerrainCell(layout, row, col);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(CELL_SPACING);
        }
    }

    private void DrawRowLabel(int row)
    {
        string label = row switch
        {
            0 => "Top",
            1 => "Mid",
            2 => "Bot",
            _ => row.ToString()
        };

        var style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight,
            fontStyle = FontStyle.Bold
        };

        GUILayout.BeginVertical(GUILayout.Width(LABEL_WIDTH), GUILayout.Height(CELL_SIZE));
        GUILayout.FlexibleSpace();
        GUILayout.Label($"{label} (Row {row})", style);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
    }

    private void DrawColumnHeaders()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(LABEL_WIDTH);

        for (int col = 0; col < BATTLE_COLS; col++)
        {
            DrawColumnHeader($"Col {col}", GetColumnTint(col));
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawColumnHeader(string label, Color tint)
    {
        Rect rect = GUILayoutUtility.GetRect(CELL_SIZE, LABEL_HEIGHT * 2);
        EditorGUI.DrawRect(rect, tint * 0.3f);

        var style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 10,
            normal = { textColor = Color.white }
        };

        EditorGUI.LabelField(rect, label, style);
        GUILayout.Space(CELL_SPACING);
    }

    // ================= CELLS =================

    private void DrawTerrainCell(TerrainLayout layout, int row, int col)
    {
        Color tintColor = GetColumnTint(col);
        Rect cellRect = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE);

        EditorGUI.DrawRect(cellRect, tintColor * 0.2f);
        Handles.DrawSolidRectangleWithOutline(cellRect, Color.clear, tintColor * 0.6f);

        TerrainTypeEnum current = layout.GetTerrainType(row, col);

        Rect popupRect = new Rect(
            cellRect.x + 4,
            cellRect.y + 22,
            cellRect.width - 8,
            EditorGUIUtility.singleLineHeight
        );

        EditorGUI.BeginChangeCheck();
        TerrainTypeEnum next = (TerrainTypeEnum)EditorGUI.EnumPopup(popupRect, current);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(layout, "Change Terrain");
            layout.SetTerrain(row, col, next);
            EditorUtility.SetDirty(layout);
        }

        DrawCellLabels(cellRect, row, col, current);
        GUILayout.Space(CELL_SPACING);
    }

    private void DrawCellLabels(Rect cellRect, int row, int col, TerrainTypeEnum terrain)
    {
        var posStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 8,
            normal = { textColor = new Color(1f, 1f, 1f, 0.6f) }
        };

        EditorGUI.LabelField(
            new Rect(cellRect.x + 2, cellRect.y + 2, 50, 14),
            $"({row},{col})",
            posStyle);

        var nameStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.LowerCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 9,
            normal = { textColor = Color.white }
        };

        EditorGUI.LabelField(
            new Rect(cellRect.x + 4, cellRect.yMax - 18, cellRect.width - 8, 16),
            terrain.ToString(),
            nameStyle);
    }

    // ================= UI SECTIONS =================

    private void DrawRandomizationOptions()
    {
        EditorGUILayout.LabelField("Randomization Options", EditorStyles.boldLabel);

        mirrorRandomization = EditorGUILayout.ToggleLeft(
            "Mirror (symmetrical battlefield)",
            mirrorRandomization);

        allowChasm = EditorGUILayout.ToggleLeft(
            "Allow Chasm terrain",
            allowChasm);
    }

    private void DrawUtilityButtons(TerrainLayout layout)
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog(
                "Clear Terrain",
                "Remove all terrain from this layout?",
                "Yes",
                "Cancel"))
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
    }

    // ================= HELPERS =================

    private Color GetColumnTint(int col)
    {
        if (col == 2 || col == 3) return new Color(0.8f, 0.3f, 0.3f); // Frontline
        if (col == 1 || col == 4) return new Color(0.8f, 0.6f, 0.3f); // Midline
        return new Color(0.3f, 0.6f, 0.8f);                           // Backline
    }

    private void DrawBattlefieldIndicator()
    {
        float totalWidth = EditorGUIUtility.currentViewWidth - 40;
        float colWidth = CELL_SIZE; // width of one terrain cell
        int colsPerSide = 3;        // player 3 columns, enemy 3 columns

        float gridStartX = LABEL_WIDTH; // after row labels

        // PLAYER SIDE
        Rect playerRect = new Rect(
            gridStartX,
            GUILayoutUtility.GetLastRect().y, // use current Y position
            colWidth * colsPerSide,
            20);
        EditorGUI.DrawRect(playerRect, new Color(0.2f, 0.4f, 0.8f, 0.2f));

        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };
        GUI.Label(playerRect, "PLAYER SIDE", style);

        // ENEMY SIDE
        Rect enemyRect = new Rect(
            gridStartX + colWidth * colsPerSide,
            playerRect.y,
            colWidth * colsPerSide,
            20);
        EditorGUI.DrawRect(enemyRect, new Color(0.8f, 0.3f, 0.3f, 0.2f));
        GUI.Label(enemyRect, "ENEMY SIDE", style);
    }


    // ================= RANDOMIZATION =================

    private void RandomizeTerrain(TerrainLayout layout)
    {
        var validTerrains = new List<TerrainTypeEnum>();

        foreach (TerrainTypeEnum t in System.Enum.GetValues(typeof(TerrainTypeEnum)))
        {
            if (!allowChasm && t == TerrainTypeEnum.Chasm)
                continue;

            validTerrains.Add(t);
        }

        int rows = BATTLE_ROWS;
        int cols = BATTLE_COLS;
        int maxCol = mirrorRandomization ? (cols + 1) / 2 : cols;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                TerrainTypeEnum value =
                    Random.value > 0.5f
                        ? validTerrains[Random.Range(0, validTerrains.Count)]
                        : TerrainTypeEnum.Regular;

                layout.SetTerrain(row, col, value);

                if (mirrorRandomization)
                {
                    int mirrorRow = rows - 1 - row;
                    int mirrorCol = cols - 1 - col;
                    layout.SetTerrain(mirrorRow, mirrorCol, value);
                }
            }
        }
    }
}
#endif
