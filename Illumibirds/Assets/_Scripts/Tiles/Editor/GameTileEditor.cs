using UnityEditor;
using UnityEngine;
using Tiles;

namespace Tiles.Editor
{
    [CustomEditor(typeof(GameTile))]
    public class GameTileEditor : UnityEditor.Editor
    {
        private SerializedProperty tileTypeProp;
        private SerializedProperty isWalkableProp;
        private SerializedProperty spriteProp;
        private SerializedProperty colorProp;
        private SerializedProperty colliderTypeProp;
        private SerializedProperty flagsProp;
        private SerializedProperty transformProp;

        private static readonly Color FloorColor = new Color(0.3f, 0.8f, 0.3f);
        private static readonly Color WallColor = new Color(0.8f, 0.3f, 0.3f);
        private static readonly Color DoorColor = new Color(0.3f, 0.5f, 0.9f);
        private static readonly Color SpawnColor = new Color(0.9f, 0.8f, 0.2f);
        private static readonly Color HalfWallColor = new Color(0.7f, 0.5f, 0.3f);

        private void OnEnable()
        {
            tileTypeProp = serializedObject.FindProperty("tileType");
            isWalkableProp = serializedObject.FindProperty("isWalkable");
            spriteProp = serializedObject.FindProperty("m_Sprite");
            colorProp = serializedObject.FindProperty("m_Color");
            colliderTypeProp = serializedObject.FindProperty("m_ColliderType");
            flagsProp = serializedObject.FindProperty("m_Flags");
            transformProp = serializedObject.FindProperty("m_TileMatrixType");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var tile = (GameTile)target;

            // Draw type indicator header
            DrawTypeHeader(tile.tileType);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Game Tile Settings", EditorStyles.boldLabel);

            // Tile Type
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(tileTypeProp);
            if (EditorGUI.EndChangeCheck())
            {
                // Auto-set walkable based on type
                var newType = (GameTile.TileType)tileTypeProp.enumValueIndex;
                isWalkableProp.boolValue = GetDefaultWalkable(newType);
            }

            // Walkable toggle
            EditorGUILayout.PropertyField(isWalkableProp);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Base Tile Settings", EditorStyles.boldLabel);

            // Sprite
            EditorGUILayout.PropertyField(spriteProp);

            // Color
            EditorGUILayout.PropertyField(colorProp);

            // Collider Type
            EditorGUILayout.PropertyField(colliderTypeProp);

            // Flags
            EditorGUILayout.PropertyField(flagsProp);

            // Transform - show as user-friendly enum + custom matrix if needed
            if (transformProp != null)
            {
                EditorGUILayout.PropertyField(transformProp, new GUIContent("Transform"));
            }

            // Show transform matrix in a readable way
            EditorGUILayout.Space(5);
            DrawTransformMatrix(tile);

            // Sprite preview
            EditorGUILayout.Space(10);
            DrawSpritePreview(tile);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSpritePreview(GameTile tile)
        {
            if (tile.sprite == null)
                return;

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            var previewSize = 128f;
            var rect = GUILayoutUtility.GetRect(previewSize, previewSize, GUILayout.ExpandWidth(false));

            var sprite = tile.sprite;
            var texture = sprite.texture;
            var texCoords = new Rect(
                sprite.textureRect.x / texture.width,
                sprite.textureRect.y / texture.height,
                sprite.textureRect.width / texture.width,
                sprite.textureRect.height / texture.height
            );

            // Draw checkerboard background for transparency
            EditorGUI.DrawTextureTransparent(rect, texture);

            // Draw the sprite with correct UVs and tint
            GUI.DrawTextureWithTexCoords(rect, texture, texCoords);
        }

        private void DrawTransformMatrix(GameTile tile)
        {
            var matrix = tile.transform;

            // Extract position, rotation, scale from matrix
            var position = new Vector3(matrix.m03, matrix.m13, matrix.m23);
            var scale = matrix.lossyScale;
            var rotation = matrix.rotation.eulerAngles;

            EditorGUILayout.LabelField("Transform", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            var newPosition = EditorGUILayout.Vector3Field("Position", position);
            var newRotation = EditorGUILayout.Vector3Field("Rotation", rotation);
            var newScale = EditorGUILayout.Vector3Field("Scale", scale);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Change Tile Transform");
                var newMatrix = Matrix4x4.TRS(newPosition, Quaternion.Euler(newRotation), newScale);
                tile.transform = newMatrix;
                EditorUtility.SetDirty(tile);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawTypeHeader(GameTile.TileType type)
        {
            var color = GetTypeColor(type);
            var icon = GetTypeIcon(type);

            var rect = GUILayoutUtility.GetRect(0, 30, GUILayout.ExpandWidth(true));

            // Background
            EditorGUI.DrawRect(rect, color);

            // Label
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            EditorGUI.LabelField(rect, $"{icon} {type}", style);
        }

        private Color GetTypeColor(GameTile.TileType type)
        {
            return type switch
            {
                GameTile.TileType.Floor => FloorColor,
                GameTile.TileType.Wall => WallColor,
                GameTile.TileType.Door => DoorColor,
                GameTile.TileType.Spawn => SpawnColor,
                GameTile.TileType.HalfWall => HalfWallColor,
                _ => Color.gray
            };
        }

        private string GetTypeIcon(GameTile.TileType type)
        {
            return type switch
            {
                GameTile.TileType.Floor => "▢",
                GameTile.TileType.Wall => "▣",
                GameTile.TileType.Door => "◫",
                GameTile.TileType.Spawn => "★",
                GameTile.TileType.HalfWall => "▤",
                _ => "?"
            };
        }

        private bool GetDefaultWalkable(GameTile.TileType type)
        {
            return type switch
            {
                GameTile.TileType.Floor => true,
                GameTile.TileType.Wall => false,
                GameTile.TileType.Door => true,
                GameTile.TileType.Spawn => true,
                GameTile.TileType.HalfWall => false,
                _ => false
            };
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var tile = (GameTile)target;

            if (tile.sprite == null)
                return base.RenderStaticPreview(assetPath, subAssets, width, height);

            var spriteEditor = UnityEditor.Editor.CreateEditor(tile.sprite);
            var preview = spriteEditor.RenderStaticPreview(assetPath, subAssets, width, height);
            DestroyImmediate(spriteEditor);

            return preview;
        }

        public override bool HasPreviewGUI()
        {
            var tile = (GameTile)target;
            return tile.sprite != null;
        }
    }
}
