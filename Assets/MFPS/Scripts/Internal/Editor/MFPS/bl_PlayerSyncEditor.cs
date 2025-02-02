﻿using UnityEngine;
using UnityEditor;
using MFPSEditor;
using Photon.Pun;

[CustomEditor(typeof(bl_PlayerSync))]
public class bl_PlayerSyncEditor : Editor
{

    bl_PlayerSync m_Target;
    private SerializedProperty m_SynchronizePositionProperty;
    private SerializedProperty m_SynchronizeRotationProperty;
    private SerializedProperty m_SynchronizeScaleProperty;

    private bool m_InterpolateHelpOpen;
    private bool m_ExtrapolateHelpOpen;
    private bool m_InterpolateRotationHelpOpen;
    private bool m_InterpolateScaleHelpOpen;

    private const int EDITOR_LINE_HEIGHT = 20;

    private const string INTERPOLATE_TOOLTIP =
        "Choose between synchronizing the value directly (by disabling interpolation) or smoothly move it towards the newest update.";

    private const string INTERPOLATE_HELP =
        "You can use interpolation to smoothly move your GameObject towards a new position that is received via the network. "
        + "This helps to reduce the stuttering movement that results because the network updates only arrive 10 times per second.\n"
        + "As a side effect, the GameObject is always lagging behind the actual position a little bit. This can be addressed with extrapolation.";

    private const string EXTRAPOLATE_TOOLTIP = "Extrapolation is used to predict where the GameObject actually is";

    private const string EXTRAPOLATE_HELP =
        "Whenever you deal with network values, all values you receive will be a little bit out of date since that data needs "
        + "to reach you first. You can use extrapolation to try to predict where the player actually is, based on the movement data you have received.\n"
        +
        "This has to be tweaked carefully for each specific game in order to insure the optimal prediction. Sometimes it is very easy to extrapolate states, because "
        +
        "the GameObject behaves very predictable (for example for vehicles). Other times it can be very hard because the user input is translated directly to the game "
        + "and you cannot really predict what the user is going to do (for example in fighting games)";

    private const string INTERPOLATE_HELP_URL = "http://doc.exitgames.com/en/pun/current/tutorials/rpg-movement";
    private const string EXTRAPOLATE_HELP_URL = "http://doc.exitgames.com/en/pun/current/tutorials/rpg-movement";

    public void OnEnable()
    {
        SetupSerializedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_Target = (bl_PlayerSync)this.target;
        bool isProjectPrefab = EditorUtility.IsPersistent(m_Target.gameObject);
        bool allowSceneObjects = !EditorUtility.IsPersistent(m_Target);

        GUI.enabled = false;
        GUILayout.BeginVertical("box");
        m_Target.FPState = (PlayerFPState)EditorGUILayout.EnumPopup("FPState", m_Target.FPState, EditorStyles.toolbarDropDown);
        GUILayout.EndVertical();
        GUI.enabled = true;
        if (m_Target.NetworkGuns == null)
        {
            m_Target.NetworkGuns = new System.Collections.Generic.List<bl_NetworkGun>();
        }

        if (m_Target.NetworkGuns.Count == 0)
        {
            m_Target.NetworkGuns.Add(null);
        }

        DrawIsPlayingWarning();
        GUI.enabled = !Application.isPlaying;

        DrawSynchronizePositionHeader();
        DrawSynchronizePositionData();

        GUI.enabled = !Application.isPlaying;
        DrawSynchronizeRotationHeader();
        DrawSynchronizeRotationData();

        GUI.enabled = !Application.isPlaying;
        DrawSynchronizeScaleHeader();
        DrawSynchronizeScaleData();

        serializedObject.ApplyModifiedProperties();

        GUI.enabled = true;
        DrawNetworkGunsList();
        EditorGUILayout.BeginVertical("box");
        m_Target.HeatTarget = EditorGUILayout.ObjectField("Heat Target", m_Target.HeatTarget, typeof(Transform), isProjectPrefab) as Transform;
        m_Target.GManager = (bl_GunManager)EditorGUILayout.ObjectField("Gun Manager", m_Target.GManager, typeof(bl_GunManager), allowSceneObjects) as bl_GunManager;
        m_Target.m_PlayerAnimation = EditorGUILayout.ObjectField("Player Animation", m_Target.m_PlayerAnimation, typeof(bl_PlayerAnimations), allowSceneObjects) as bl_PlayerAnimations;
        if(GUILayout.Button("Current FPWeapon", EditorStyles.toolbarButton))
        {
            bl_GunManager gm = m_Target.transform.GetComponentInChildren<bl_GunManager>(true);
            if (Application.isPlaying)
            {
                Selection.activeObject = gm.CurrentGun;
                EditorGUIUtility.PingObject(gm.CurrentGun);
            }
            else
            {
                Selection.activeObject = gm;
                EditorGUIUtility.PingObject(gm);
            }
        }
        EditorGUILayout.EndVertical();
    }

    int GetGunsCount()
    {
        int count = 0;

        for (int i = 0; i < m_Target.NetworkGuns.Count; ++i)
        {
            if (m_Target.NetworkGuns[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    void DrawNetworkGunsList()
    {
        GUILayout.Space(5);
        SerializedProperty listProperty = serializedObject.FindProperty(Dependency.GunListPropiertie);

        if (listProperty == null)
        {
            return;
        }

        float containerElementHeight = 22;
        float containerHeight = listProperty.arraySize * containerElementHeight;

        bool isOpen = PhotonGUI.ContainerHeaderFoldout("Network Guns (" + GetGunsCount() + ")", serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue);
        serializedObject.FindProperty("ObservedComponentsFoldoutOpen").boolValue = isOpen;

        if (isOpen == false)
        {
            containerHeight = 0;
        }

        Rect containerRect = PhotonGUI.ContainerBody(containerHeight);
        if (isOpen == true)
        {
            for (int i = 0; i < listProperty.arraySize; ++i)
            {
                Rect elementRect = new Rect(containerRect.xMin, containerRect.yMin + containerElementHeight * i, containerRect.width, containerElementHeight);
                {
                    Rect texturePosition = new Rect(elementRect.xMin + 6, elementRect.yMin + elementRect.height / 2f - 1, 9, 5);
                    //MFPSEditorUtils.DrawTexture(texturePosition, MFPSEditorUtils.texGrabHandle);

                    Rect propertyPosition = new Rect(elementRect.xMin + 20, elementRect.yMin + 3, elementRect.width - 45, 16);
                    EditorGUI.PropertyField(propertyPosition, listProperty.GetArrayElementAtIndex(i), new GUIContent());

                    Rect removeButtonRect = new Rect(elementRect.xMax - PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        elementRect.yMin + 2,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedWidth,
                                                        PhotonGUI.DefaultRemoveButtonStyle.fixedHeight);

                    GUI.enabled = listProperty.arraySize > 1;
                    if (GUI.Button(removeButtonRect, new GUIContent(MFPSEditorUtils.texRemoveButton), PhotonGUI.DefaultRemoveButtonStyle))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                    GUI.enabled = true;

                    if (i < listProperty.arraySize - 1)
                    {
                        texturePosition = new Rect(elementRect.xMin + 2, elementRect.yMax, elementRect.width - 4, 1);
                        PhotonGUI.DrawSplitter(texturePosition);
                    }
                }
            }
        }

        if (PhotonGUI.AddButton())
        {
            listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));
        }

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawIsPlayingWarning()
    {
        if (Application.isPlaying == false)
        {
            return;
        }

        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Label("Editing is disabled in play mode so the two objects don't go out of sync");
        }
        GUILayout.EndVertical();
    }

    private void SetupSerializedProperties()
    {
        this.m_SynchronizePositionProperty = serializedObject.FindProperty(Dependency.SynchronizePositionProperty);
        this.m_SynchronizeRotationProperty = serializedObject.FindProperty(Dependency.SynchronizeRotationProperty);
        this.m_SynchronizeScaleProperty = serializedObject.FindProperty(Dependency.SynchronizeScaleProperty);
    }

    private void DrawSynchronizePositionHeader()
    {
        DrawHeader("Synchronize Position", this.m_SynchronizePositionProperty);
    }

    private void DrawSynchronizePositionData()
    {
        if (this.m_SynchronizePositionProperty == null || this.m_SynchronizePositionProperty.boolValue == false)
        {
            return;
        }

        SerializedProperty interpolatePositionProperty = serializedObject.FindProperty("m_PositionModel.InterpolateOption");
        PhotonTransformViewPositionModel.InterpolateOptions interpolateOption = (PhotonTransformViewPositionModel.InterpolateOptions)interpolatePositionProperty.enumValueIndex;

        SerializedProperty extrapolatePositionProperty = serializedObject.FindProperty("m_PositionModel.ExtrapolateOption");
        PhotonTransformViewPositionModel.ExtrapolateOptions extrapolateOption = (PhotonTransformViewPositionModel.ExtrapolateOptions)extrapolatePositionProperty.enumValueIndex;

        float containerHeight = 155;

        switch (interpolateOption)
        {
            case PhotonTransformViewPositionModel.InterpolateOptions.FixedSpeed:
            case PhotonTransformViewPositionModel.InterpolateOptions.Lerp:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.Disabled)
        {
            containerHeight += EDITOR_LINE_HEIGHT;
        }

        switch (extrapolateOption)
        {
            case PhotonTransformViewPositionModel.ExtrapolateOptions.FixedSpeed:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (this.m_InterpolateHelpOpen == true)
        {
            containerHeight += GetInterpolateHelpBoxHeight();
        }

        if (this.m_ExtrapolateHelpOpen == true)
        {
            containerHeight += GetExtrapolateHelpBoxHeight();
        }

        // removed Gizmo Options. -3 lines, -1 splitter
        containerHeight -= EDITOR_LINE_HEIGHT * 2;

        Rect rect = PhotonGUI.ContainerBody(containerHeight);

        Rect propertyRect = new Rect(rect.xMin + 5, rect.yMin + 2, rect.width - 10, EditorGUIUtility.singleLineHeight);

        DrawTeleport(ref propertyRect);
        DrawSplitter(ref propertyRect);

        DrawSynchronizePositionDataInterpolation(ref propertyRect, interpolatePositionProperty, interpolateOption);
        DrawSplitter(ref propertyRect);

        DrawSynchronizePositionDataExtrapolation(ref propertyRect, extrapolatePositionProperty, extrapolateOption);
        DrawSplitter(ref propertyRect);

        DrawSynchronizePositionDataGizmos(ref propertyRect);
    }

    private float GetInterpolateHelpBoxHeight()
    {
        return PhotonGUI.RichLabel.CalcHeight(new GUIContent(INTERPOLATE_HELP), Screen.width - 54) + 35;
    }

    private float GetExtrapolateHelpBoxHeight()
    {
        return PhotonGUI.RichLabel.CalcHeight(new GUIContent(EXTRAPOLATE_HELP), Screen.width - 54) + 35;
    }

    private void DrawSplitter(ref Rect propertyRect)
    {
        Rect splitterRect = new Rect(propertyRect.xMin - 3, propertyRect.yMin, propertyRect.width + 6, 1);
        PhotonGUI.DrawSplitter(splitterRect);

        propertyRect.y += 5;
    }

    private void DrawSynchronizePositionDataGizmos(ref Rect propertyRect)
    {
        GUI.enabled = true;

        /* EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.DrawErrorGizmo"),
             new GUIContent("Draw synchronized position error"));
         propertyRect.y += EDITOR_LINE_HEIGHT;*/
    }

    private void DrawHelpBox(ref Rect propertyRect, bool isOpen, float height, string helpText, string url)
    {
        if (isOpen == true)
        {
            Rect helpRect = new Rect(propertyRect.xMin, propertyRect.yMin, propertyRect.width, height - 5);
            GUI.BeginGroup(helpRect, GUI.skin.box);
            GUI.Label(new Rect(5, 5, propertyRect.width - 10, height - 30), helpText, PhotonGUI.RichLabel);
            if (GUI.Button(new Rect(5, height - 30, propertyRect.width - 10, 20), "Read more in our documentation"))
            {
                Application.OpenURL(url);
            }
            GUI.EndGroup();

            propertyRect.y += height;
        }
    }

    private void DrawPropertyWithHelpIcon(ref Rect propertyRect, ref bool isHelpOpen, SerializedProperty property, string tooltip)
    {
        Rect propertyFieldRect = new Rect(propertyRect.xMin, propertyRect.yMin, propertyRect.width - 20, propertyRect.height);
        string propertyName = ObjectNames.NicifyVariableName(property.name);
        EditorGUI.PropertyField(propertyFieldRect, property, new GUIContent(propertyName, tooltip));

        Rect helpIconRect = new Rect(propertyFieldRect.xMax + 5, propertyFieldRect.yMin, 20, propertyFieldRect.height);
        isHelpOpen = GUI.Toggle(helpIconRect, isHelpOpen, PhotonGUI.HelpIcon, GUIStyle.none);

        propertyRect.y += EDITOR_LINE_HEIGHT;
    }

    private void DrawSynchronizePositionDataExtrapolation(ref Rect propertyRect, SerializedProperty extrapolatePositionProperty, PhotonTransformViewPositionModel.ExtrapolateOptions extrapolateOption)
    {
        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_ExtrapolateHelpOpen, extrapolatePositionProperty, EXTRAPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_ExtrapolateHelpOpen, GetExtrapolateHelpBoxHeight(), EXTRAPOLATE_HELP, EXTRAPOLATE_HELP_URL);

        if (extrapolateOption != PhotonTransformViewPositionModel.ExtrapolateOptions.Disabled)
        {
            EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.ExtrapolateIncludingRoundTripTime"));
            propertyRect.y += EDITOR_LINE_HEIGHT;
        }

        switch (extrapolateOption)
        {
            case PhotonTransformViewPositionModel.ExtrapolateOptions.FixedSpeed:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.ExtrapolateSpeed"));
                propertyRect.y += EDITOR_LINE_HEIGHT;
                break;
        }
    }

    private void DrawTeleport(ref Rect propertyRect)
    {
        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.TeleportEnabled"),
            new GUIContent("Enable teleport for great distances"));
        propertyRect.y += EDITOR_LINE_HEIGHT;

        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.TeleportIfDistanceGreaterThan"),
            new GUIContent("Teleport if distance greater than"));
        propertyRect.y += EDITOR_LINE_HEIGHT;
    }

    private void DrawSynchronizePositionDataInterpolation(ref Rect propertyRect, SerializedProperty interpolatePositionProperty,
        PhotonTransformViewPositionModel.InterpolateOptions interpolateOption)
    {
        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_InterpolateHelpOpen, interpolatePositionProperty, INTERPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_InterpolateHelpOpen, GetInterpolateHelpBoxHeight(), INTERPOLATE_HELP, INTERPOLATE_HELP_URL);

        switch (interpolateOption)
        {
            case PhotonTransformViewPositionModel.InterpolateOptions.FixedSpeed:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.InterpolateMoveTowardsSpeed"),
                    new GUIContent("MoveTowards Speed"));
                propertyRect.y += EDITOR_LINE_HEIGHT;
                break;

            case PhotonTransformViewPositionModel.InterpolateOptions.Lerp:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.InterpolateLerpSpeed"), new GUIContent("Lerp Speed"));
                propertyRect.y += EDITOR_LINE_HEIGHT;
                break;

        }
    }

    private void DrawSynchronizeRotationHeader()
    {
        DrawHeader("Synchronize Rotation", this.m_SynchronizeRotationProperty);
    }

    private void DrawSynchronizeRotationData()
    {
        if (this.m_SynchronizeRotationProperty == null || this.m_SynchronizeRotationProperty.boolValue == false)
        {
            return;
        }

        SerializedProperty interpolateRotationProperty = serializedObject.FindProperty("m_RotationModel.InterpolateOption");
        PhotonTransformViewRotationModel.InterpolateOptions interpolateOption =
            (PhotonTransformViewRotationModel.InterpolateOptions)interpolateRotationProperty.enumValueIndex;

        float containerHeight = 20;

        switch (interpolateOption)
        {
            case PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards:
            case PhotonTransformViewRotationModel.InterpolateOptions.Lerp:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (this.m_InterpolateRotationHelpOpen == true)
        {
            containerHeight += GetInterpolateHelpBoxHeight();
        }

        Rect rect = PhotonGUI.ContainerBody(containerHeight);
        Rect propertyRect = new Rect(rect.xMin + 5, rect.yMin + 2, rect.width - 10, EditorGUIUtility.singleLineHeight);

        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_InterpolateRotationHelpOpen, interpolateRotationProperty, INTERPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_InterpolateRotationHelpOpen, GetInterpolateHelpBoxHeight(), INTERPOLATE_HELP, INTERPOLATE_HELP_URL);

        switch (interpolateOption)
        {
            case PhotonTransformViewRotationModel.InterpolateOptions.RotateTowards:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_RotationModel.InterpolateRotateTowardsSpeed"),
                    new GUIContent("RotateTowards Speed"));
                break;
            case PhotonTransformViewRotationModel.InterpolateOptions.Lerp:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_RotationModel.InterpolateLerpSpeed"), new GUIContent("Lerp Speed"));
                break;
        }
    }

    private void DrawSynchronizeScaleHeader()
    {
        DrawHeader("Synchronize Scale", this.m_SynchronizeScaleProperty);
    }

    private void DrawSynchronizeScaleData()
    {
        if (this.m_SynchronizeScaleProperty == null || this.m_SynchronizeScaleProperty.boolValue == false)
        {
            return;
        }

        SerializedProperty interpolateScaleProperty = serializedObject.FindProperty("m_ScaleModel.InterpolateOption");
        PhotonTransformViewScaleModel.InterpolateOptions interpolateOption = (PhotonTransformViewScaleModel.InterpolateOptions)interpolateScaleProperty.enumValueIndex;

        float containerHeight = EDITOR_LINE_HEIGHT;

        switch (interpolateOption)
        {
            case PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards:
            case PhotonTransformViewScaleModel.InterpolateOptions.Lerp:
                containerHeight += EDITOR_LINE_HEIGHT;
                break;
        }

        if (this.m_InterpolateScaleHelpOpen == true)
        {
            containerHeight += GetInterpolateHelpBoxHeight();
        }

        Rect rect = PhotonGUI.ContainerBody(containerHeight);
        Rect propertyRect = new Rect(rect.xMin + 5, rect.yMin + 2, rect.width - 10, EditorGUIUtility.singleLineHeight);

        DrawPropertyWithHelpIcon(ref propertyRect, ref this.m_InterpolateScaleHelpOpen, interpolateScaleProperty, INTERPOLATE_TOOLTIP);
        DrawHelpBox(ref propertyRect, this.m_InterpolateScaleHelpOpen, GetInterpolateHelpBoxHeight(), INTERPOLATE_HELP, INTERPOLATE_HELP_URL);

        switch (interpolateOption)
        {
            case PhotonTransformViewScaleModel.InterpolateOptions.MoveTowards:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_ScaleModel.InterpolateMoveTowardsSpeed"),
                    new GUIContent("MoveTowards Speed"));
                break;
            case PhotonTransformViewScaleModel.InterpolateOptions.Lerp:
                EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_ScaleModel.InterpolateLerpSpeed"), new GUIContent("Lerp Speed"));
                break;
        }
    }

    private void DrawHeader(string label, SerializedProperty property)
    {
        if (property == null)
        {
            return;
        }

        bool newValue = PhotonGUI.ContainerHeaderToggle(label, property.boolValue);

        if (newValue != property.boolValue)
        {
            Undo.RecordObject(this.m_Target, "Change " + label);
            property.boolValue = newValue;
            EditorUtility.SetDirty(this.m_Target);
        }
    }
}
