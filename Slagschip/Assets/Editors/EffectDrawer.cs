using FX;
using FX.Enumeration;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Effect))]
public class EffectDrawer : PropertyDrawer
{
    private const string FoldoutKey = "EffectDrawer_Foldout_";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        int id = property.serializedObject.targetObject.GetInstanceID();
        string foldoutPrefKey = FoldoutKey + id + "_" + property.propertyPath;

        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);

        bool isExpanded = SessionState.GetBool(foldoutPrefKey, false);
        isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, label, true);
        SessionState.SetBool(foldoutPrefKey, isExpanded);

        if (!isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;

        float y = foldoutRect.y + lineHeight;

        SerializedProperty effectType = property.FindPropertyRelative("effectType");
        SerializedProperty timing = property.FindPropertyRelative("timing");
        SerializedProperty delay = property.FindPropertyRelative("delay");
        SerializedProperty particleSystem = property.FindPropertyRelative("particleSystem");
        SerializedProperty audioClip = property.FindPropertyRelative("audioClip");
        SerializedProperty audioSource = property.FindPropertyRelative("audioSource");
        SerializedProperty onEffectStart = property.FindPropertyRelative("onEffectStart");
        SerializedProperty onEffectEnd = property.FindPropertyRelative("onEffectEnd");

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), timing);
        y += lineHeight;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), effectType);
        y += lineHeight;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), delay);
        y += lineHeight;

        switch ((EEffectType)effectType.enumValueIndex)
        {
            case EEffectType.Particle:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), particleSystem);
                y += lineHeight;
                break;
            case EEffectType.Audio:
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), audioClip);
                y += lineHeight;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), audioSource);
                y += lineHeight;
                break;
        }

        float evtStartHeight = EditorGUI.GetPropertyHeight(onEffectStart, true);
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, evtStartHeight), onEffectStart, true);
        y += evtStartHeight + 2;

        float evtEndHeight = EditorGUI.GetPropertyHeight(onEffectEnd, true);
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, evtEndHeight), onEffectEnd, true);
        y += evtEndHeight;

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int id = property.serializedObject.targetObject.GetInstanceID();
        string foldoutPrefKey = FoldoutKey + id + "_" + property.propertyPath;
        bool isExpanded = SessionState.GetBool(foldoutPrefKey, false);

        float height = EditorGUIUtility.singleLineHeight + 2;
        if (!isExpanded) return height;

        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        SerializedProperty effectType = property.FindPropertyRelative("effectType");
        SerializedProperty onEffectStart = property.FindPropertyRelative("onEffectStart");
        SerializedProperty onEffectEnd = property.FindPropertyRelative("onEffectEnd");

        height += lineHeight * 3; // timing, effectType, delay

        switch ((EEffectType)effectType.enumValueIndex)
        {
            case EEffectType.Particle:
            case EEffectType.Audio:
                height += lineHeight;
                if ((EEffectType)effectType.enumValueIndex == EEffectType.Audio)
                    height += lineHeight; // audioSource
                break;
        }

        height += EditorGUI.GetPropertyHeight(onEffectStart, true) + 2;
        height += EditorGUI.GetPropertyHeight(onEffectEnd, true);

        return height;
    }
}