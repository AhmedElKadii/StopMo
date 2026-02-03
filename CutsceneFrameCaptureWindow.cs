using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CutsceneFrameCaptureWindow : EditorWindow
{
    private CutsceneManager targetManager;
    private Camera sceneCamera;
    private List<GameObject> charactersToCapture = new List<GameObject>();
    private string frameName = "";
    private string animationSavePath = "Assets/Animations/CutscenePoses/";
    
    private int currentFrameIndex = 0;
    private bool isPlaying = false;
    private double lastFrameTime = 0;
    private float playbackFPS = 24f;
    private Vector2 frameStripScroll;
    
    [MenuItem("Tools/Cutscene Frame Capture")]
    public static void ShowWindow()
    {
        GetWindow<CutsceneFrameCaptureWindow>("Frame Capture");
    }
    
    private void OnEnable()
    {
        if (targetManager == null)
        {
            targetManager = FindFirstObjectByType<CutsceneManager>();
        }
        
        if (sceneCamera == null)
        {
            sceneCamera = Camera.main;
        }
        
        EditorApplication.update += OnEditorUpdate;
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        isPlaying = false;
    }
    
    private void OnEditorUpdate()
    {
        if (isPlaying && targetManager != null && targetManager.frames.Count > 0)
        {
            double currentTime = EditorApplication.timeSinceStartup;
            
            if (currentTime - lastFrameTime >= (1.0 / playbackFPS))
            {
                lastFrameTime = currentTime;
                currentFrameIndex++;
                
                if (currentFrameIndex >= targetManager.frames.Count)
                {
                    currentFrameIndex = 0; // Loop
                }
                
                PreviewFrame(currentFrameIndex);
                Repaint();
            }
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label(
            new GUIContent("Cutscene Frame Capture", "Capture and manage frames."),
            EditorStyles.boldLabel
        );
        
        EditorGUILayout.Space();
        
        targetManager = (CutsceneManager)EditorGUILayout.ObjectField(
            new GUIContent("Cutscene Manager", "Target manager that stores frames."),
            targetManager, 
            typeof(CutsceneManager), 
            true
        );
        
        sceneCamera = (Camera)EditorGUILayout.ObjectField(
            new GUIContent("Camera to Capture", "Camera to record the frame view."),
            sceneCamera, 
            typeof(Camera), 
            true
        );
        
        EditorGUILayout.Space();
        
        animationSavePath = EditorGUILayout.TextField(
            new GUIContent("Animation Save Path", "Folder for pose clips."),
            animationSavePath
        );
        
        EditorGUILayout.Space();

        frameName = EditorGUILayout.TextField(
            new GUIContent("Frame Name", "Optional label."),
            frameName
        );
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            new GUIContent("Characters in this Frame:", "Characters to capture."),
            EditorStyles.boldLabel
        );
        
        int indexToRemove = -1;
        for (int i = 0; i < charactersToCapture.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            charactersToCapture[i] = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("", "Character to capture."),
                charactersToCapture[i], 
                typeof(GameObject), 
                true
            );
            if (GUILayout.Button(
                new GUIContent("Remove", "Remove slot."),
                GUILayout.Width(60)
            ))
            {
                indexToRemove = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        if (indexToRemove >= 0)
        {
            charactersToCapture.RemoveAt(indexToRemove);
        }
        
        if (GUILayout.Button(new GUIContent("Add Character Slot", "Add slot.")))
        {
            charactersToCapture.Add(null);
        }
        
        EditorGUILayout.Space(10);
        
        if (targetManager != null)
        {
            EditorGUILayout.LabelField(
                new GUIContent("Quick Add:", "Quick add."),
                EditorStyles.boldLabel
            );
            EditorGUILayout.BeginHorizontal();
            
            if (targetManager.francisco != null && GUILayout.Button(
                new GUIContent("Francisco", "Add Francisco.")
            ))
                AddCharacterIfNotPresent(targetManager.francisco);
            if (targetManager.barnardo != null && GUILayout.Button(
                new GUIContent("Barnardo", "Add Barnardo.")
            ))
                AddCharacterIfNotPresent(targetManager.barnardo);
            if (targetManager.marcellus != null && GUILayout.Button(
                new GUIContent("Marcellus", "Add Marcellus.")
            ))
                AddCharacterIfNotPresent(targetManager.marcellus);
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            
            if (targetManager.horatio != null && GUILayout.Button(
                new GUIContent("Horatio", "Add Horatio.")
            ))
                AddCharacterIfNotPresent(targetManager.horatio);
            if (targetManager.hamlet != null && GUILayout.Button(
                new GUIContent("Hamlet", "Add Hamlet.")
            ))
                AddCharacterIfNotPresent(targetManager.hamlet);
            if (targetManager.ghost != null && GUILayout.Button(
                new GUIContent("Ghost", "Add Ghost.")
            ))
                AddCharacterIfNotPresent(targetManager.ghost);
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(20);

        // PLAYBACK CONTROLS
        if (targetManager != null && targetManager.frames.Count > 0)
        {
            EditorGUILayout.LabelField(
                new GUIContent("Playback Controls", "Preview frames."),
                EditorStyles.boldLabel
            );
            
            // Playback buttons in one row - grey and full width
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = Color.grey;
            
            // Jump to start
            if (GUILayout.Button(
                new GUIContent("|◀", "First frame."),
                GUILayout.Height(30)
            ))
            {
                JumpToStart();
            }
            
            // Previous frame
            if (GUILayout.Button(
                new GUIContent("◀", "Previous frame."),
                GUILayout.Height(30)
            ))
            {
                PreviousFrame();
            }
            
            // Play/Pause
			string playButtonText = isPlaying ? "█" : "▶";
            if (GUILayout.Button(
                new GUIContent(playButtonText, "Play/pause."),
                GUILayout.Height(30)
            ))
            {
                TogglePlayback();
            }
            
            // Next frame
            if (GUILayout.Button(
                new GUIContent("▶", "Next frame."),
                GUILayout.Height(30)
            ))
            {
                NextFrame();
            }
            
            // Jump to end
            if (GUILayout.Button(
                new GUIContent("▶|", "Last frame."),
                GUILayout.Height(30)
            ))
            {
                JumpToEnd();
            }
            
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            // Frame counter - display 1-based indexing
            EditorGUILayout.LabelField(
                new GUIContent(
                    $"Frame {currentFrameIndex + 1} / {targetManager.frames.Count}",
                    "Current frame."
                ),
                EditorStyles.centeredGreyMiniLabel);
            
            // Scrubber slider (no label) - display 1-based indexing
            int newFrameDisplayIndex = EditorGUILayout.IntSlider(
                new GUIContent("", "Scrub frames."),
                currentFrameIndex + 1,
                1,
                targetManager.frames.Count
            );
            if (newFrameDisplayIndex != currentFrameIndex + 1)
            {
                currentFrameIndex = newFrameDisplayIndex - 1;
                PreviewFrame(currentFrameIndex);
            }
            
            // Playback FPS
            playbackFPS = EditorGUILayout.Slider(
                new GUIContent("Playback FPS", "Preview speed."),
                playbackFPS,
                1f,
                60f
            );
            
            EditorGUILayout.Space(10);
        }
        
        // FRAME STRIP
        if (targetManager != null && targetManager.frames.Count > 0)
        {
            EditorGUILayout.LabelField(
                new GUIContent("Frames", "Frame tools."),
                EditorStyles.boldLabel
            );
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                new GUIContent("Duplicate", "Duplicate frame."),
                GUILayout.Height(26)
            ))
            {
                DuplicateCurrentFrame();
            }

            GUI.enabled = currentFrameIndex > 0;
            if (GUILayout.Button(
                new GUIContent("Move ◀", "Move earlier."),
                GUILayout.Height(26),
                GUILayout.Width(70)
            ))
            {
                MoveFrame(-1);
            }

            GUI.enabled = currentFrameIndex < targetManager.frames.Count - 1;
            if (GUILayout.Button(
                new GUIContent("Move ▶", "Move later."),
                GUILayout.Height(26),
                GUILayout.Width(70)
            ))
            {
                MoveFrame(1);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            frameStripScroll = EditorGUILayout.BeginScrollView(
                frameStripScroll,
                GUILayout.Height(36)
            );
            EditorGUILayout.BeginHorizontal();

            GUIStyle dotStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = 26,
                fixedHeight = 22,
                alignment = TextAnchor.MiddleCenter
            };

            for (int i = 0; i < targetManager.frames.Count; i++)
            {
                GUI.backgroundColor = (i == currentFrameIndex)
                    ? new Color(1f, 0.9f, 0.4f)
                    : Color.grey;

                if (GUILayout.Button(
                    new GUIContent(
                        i == currentFrameIndex ? "●" : "○",
                        GetFrameTooltip(i)
                    ),
                    dotStyle
                ))
                {
                    currentFrameIndex = i;
                    PreviewFrame(currentFrameIndex);
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }
        
        // CAPTURE BUTTONS
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button(
            new GUIContent("CAPTURE FRAME", "Capture new frame."),
            GUILayout.Height(40)
        ))
        {
            CaptureFrame(-1);
        }
        
        // Replace current frame button (only show if there are frames)
        if (targetManager != null && targetManager.frames.Count > 0)
        {
            GUI.backgroundColor = new Color(0.6f, 0.4f, 0.8f); // Purple
            if (GUILayout.Button(
                new GUIContent("↻", "Replace frame."),
                GUILayout.Height(40),
                GUILayout.Width(50)
            ))
            {
                if (EditorUtility.DisplayDialog("Replace Frame", 
                    $"Replace frame {currentFrameIndex}?", "Replace", "Cancel"))
                {
                    ReplaceCurrentFrame();
                }
            }
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        if (targetManager != null && targetManager.frames.Count > 0)
        {
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button(
                new GUIContent("Delete Current Frame", "Delete frame."),
                GUILayout.Height(24)
            ))
            {
                if (EditorUtility.DisplayDialog("Delete Frame", 
                    $"Delete frame {currentFrameIndex}?", "Delete", "Cancel"))
                {
                    DeleteFrame(currentFrameIndex);
                }
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button(
                new GUIContent("Clear All Frames", "Delete all."),
                GUILayout.Height(24)
            ))
            {
                if (EditorUtility.DisplayDialog("Clear Frames", 
                    "Are you sure you want to clear all frames?", "Yes", "Cancel"))
                {
                    ClearAllFrames();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.LabelField($"Total Frames: {targetManager.frames.Count}");
        }
    }
    
    private void TogglePlayback()
    {
        isPlaying = !isPlaying;
        if (isPlaying)
        {
            lastFrameTime = EditorApplication.timeSinceStartup;
        }
    }
    
    private void JumpToStart()
    {
        currentFrameIndex = 0;
        PreviewFrame(currentFrameIndex);
        isPlaying = false;
    }
    
    private void JumpToEnd()
    {
        if (targetManager == null || targetManager.frames.Count == 0) return;
        currentFrameIndex = targetManager.frames.Count - 1;
        PreviewFrame(currentFrameIndex);
        isPlaying = false;
    }
    
    private void PreviousFrame()
    {
        if (targetManager == null || targetManager.frames.Count == 0) return;
        
        isPlaying = false;
        currentFrameIndex--;
        if (currentFrameIndex < 0) currentFrameIndex = 0;
        
        PreviewFrame(currentFrameIndex);
    }
    
    private void NextFrame()
    {
        if (targetManager == null || targetManager.frames.Count == 0) return;
        
        isPlaying = false;
        currentFrameIndex++;
        if (currentFrameIndex >= targetManager.frames.Count)
            currentFrameIndex = targetManager.frames.Count - 1;
        
        PreviewFrame(currentFrameIndex);
    }
    
    private void PreviewFrame(int index)
    {
        if (targetManager == null || index < 0 || index >= targetManager.frames.Count)
            return;
        
        targetManager.ApplyFrame(index);
        PopulateCharactersFromFrame(index);
        SceneView.RepaintAll();
    }
    
    private void ReplaceCurrentFrame()
    {
        if (targetManager == null || currentFrameIndex < 0 || currentFrameIndex >= targetManager.frames.Count)
            return;
        
        if (sceneCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Camera to capture!", "OK");
            return;
        }
        
        int undoGroup = BeginUndoGroup("Replace Frame");

        // Delete the old frame's GameObject and pose clips
        CutsceneFrame oldFrame = targetManager.frames[currentFrameIndex];
        if (oldFrame.cameraPosition != null)
        {
            GameObject oldFrameContainer = oldFrame.cameraPosition.parent != null 
                ? oldFrame.cameraPosition.parent.gameObject 
                : oldFrame.cameraPosition.gameObject;
            
            Undo.DestroyObjectImmediate(oldFrameContainer);
        }
        DestroyFramePoseClips(oldFrame);
        
        // Create new frame data
        string frameContainerName = string.IsNullOrEmpty(frameName) 
            ? $"Frame{currentFrameIndex}_Positions" 
            : $"Frame{currentFrameIndex}_{frameName}";
            
        GameObject frameContainer = new GameObject(frameContainerName);
        Undo.RegisterCreatedObjectUndo(frameContainer, "Replace Frame");
        
        GameObject camPosMarker = new GameObject("CameraPosition");
        camPosMarker.transform.SetParent(frameContainer.transform);
        camPosMarker.transform.SetPositionAndRotation(
            sceneCamera.transform.position,
            sceneCamera.transform.rotation
        );
        
        CutsceneFrame newFrame = new CutsceneFrame();
        newFrame.frameName = frameName;
        newFrame.cameraPosition = camPosMarker.transform;
        newFrame.characters = new List<CharacterState>();
        
        foreach (GameObject character in charactersToCapture)
        {
            if (character != null)
            {
                GameObject charPosMarker = new GameObject($"{character.name}_Position");
                charPosMarker.transform.SetParent(frameContainer.transform);
                charPosMarker.transform.SetPositionAndRotation(
                    character.transform.position,
                    character.transform.rotation
                );
                
                string poseClipName = $"{frameContainerName}_{character.name}_Pose";
                AnimationClip poseClip = CaptureCharacterPose(character, poseClipName);
                
                CharacterState charState = new CharacterState
                {
                    characterName = character.name,
                    position = charPosMarker.transform,
                    poseClip = poseClip,
                    isActive = character.activeSelf
                };
                
                newFrame.characters.Add(charState);
            }
        }
        
        // Replace in list
        Undo.RegisterCompleteObjectUndo(targetManager, "Replace Frame");
        targetManager.frames[currentFrameIndex] = newFrame;
        EditorUtility.SetDirty(targetManager);
        
        Debug.Log($"Frame {currentFrameIndex} replaced! Characters: {newFrame.characters.Count}");
        
        charactersToCapture.Clear();
        
        // Preview the replaced frame
        PreviewFrame(currentFrameIndex);

        EndUndoGroup(undoGroup);
    }

    private void DuplicateCurrentFrame()
    {
        if (targetManager == null || currentFrameIndex < 0 || currentFrameIndex >= targetManager.frames.Count)
            return;

        int undoGroup = BeginUndoGroup("Duplicate Frame");

        CutsceneFrame sourceFrame = targetManager.frames[currentFrameIndex];

        string frameContainerName = string.IsNullOrEmpty(sourceFrame.frameName)
            ? $"Frame{currentFrameIndex + 1}_Positions"
            : $"Frame{currentFrameIndex + 1}_{sourceFrame.frameName}";

        GameObject frameContainer = new GameObject(frameContainerName);
        Undo.RegisterCreatedObjectUndo(frameContainer, "Duplicate Frame");

        GameObject camPosMarker = new GameObject("CameraPosition");
        camPosMarker.transform.SetParent(frameContainer.transform);
        if (sourceFrame.cameraPosition != null)
        {
            camPosMarker.transform.SetPositionAndRotation(
                sourceFrame.cameraPosition.position,
                sourceFrame.cameraPosition.rotation
            );
        }

        CutsceneFrame newFrame = new CutsceneFrame();
        newFrame.frameName = sourceFrame.frameName;
        newFrame.cameraPosition = camPosMarker.transform;
        newFrame.characters = new List<CharacterState>();
        newFrame.duration = sourceFrame.duration;

        foreach (CharacterState charState in sourceFrame.characters)
        {
            GameObject charPosMarker = new GameObject($"{charState.characterName}_Position");
            charPosMarker.transform.SetParent(frameContainer.transform);
            if (charState.position != null)
            {
                charPosMarker.transform.SetPositionAndRotation(
                    charState.position.position,
                    charState.position.rotation
                );
            }

            AnimationClip poseClip = null;
            if (charState.poseClip != null)
            {
                poseClip = DuplicatePoseClip(
                    charState.poseClip,
                    frameContainerName,
                    charState.characterName
                );
            }

            CharacterState newCharState = new CharacterState
            {
                characterName = charState.characterName,
                position = charPosMarker.transform,
                poseClip = poseClip,
                isActive = charState.isActive
            };

            newFrame.characters.Add(newCharState);
        }

        Undo.RegisterCompleteObjectUndo(targetManager, "Duplicate Frame");
        int insertIndex = Mathf.Clamp(currentFrameIndex + 1, 0, targetManager.frames.Count);
        targetManager.frames.Insert(insertIndex, newFrame);
        currentFrameIndex = insertIndex;
        EditorUtility.SetDirty(targetManager);

        RefreshFrameContainerNames();
        PreviewFrame(currentFrameIndex);

        EndUndoGroup(undoGroup);
    }

    private void MoveFrame(int direction)
    {
        if (targetManager == null || targetManager.frames.Count == 0)
            return;

        int newIndex = currentFrameIndex + direction;
        if (newIndex < 0 || newIndex >= targetManager.frames.Count)
            return;

        int undoGroup = BeginUndoGroup("Move Frame");
        Undo.RegisterCompleteObjectUndo(targetManager, "Move Frame");
        CutsceneFrame temp = targetManager.frames[currentFrameIndex];
        targetManager.frames[currentFrameIndex] = targetManager.frames[newIndex];
        targetManager.frames[newIndex] = temp;

        currentFrameIndex = newIndex;
        EditorUtility.SetDirty(targetManager);

        RefreshFrameContainerNames();
        PreviewFrame(currentFrameIndex);

        EndUndoGroup(undoGroup);
    }
    
    private void DeleteFrame(int index)
    {
        if (targetManager == null || index < 0 || index >= targetManager.frames.Count)
            return;

        int undoGroup = BeginUndoGroup("Delete Frame");

        CutsceneFrame frameToDelete = targetManager.frames[index];
        
        if (frameToDelete.cameraPosition != null)
        {
            GameObject camParent = frameToDelete.cameraPosition.parent != null 
                ? frameToDelete.cameraPosition.parent.gameObject 
                : frameToDelete.cameraPosition.gameObject;
            
            Undo.DestroyObjectImmediate(camParent);
        }

        DestroyFramePoseClips(frameToDelete);
        
        Undo.RegisterCompleteObjectUndo(targetManager, "Delete Frame");
        targetManager.frames.RemoveAt(index);
        EditorUtility.SetDirty(targetManager);
        
        if (targetManager.frames.Count > 0)
        {
            if (currentFrameIndex >= targetManager.frames.Count)
            {
                currentFrameIndex = targetManager.frames.Count - 1;
            }
            
            RefreshFrameContainerNames();
            PreviewFrame(currentFrameIndex);
        }
        else
        {
            currentFrameIndex = 0;
        }

        EndUndoGroup(undoGroup);
    }
    
    private void ClearAllFrames()
    {
        if (targetManager == null) return;

        int undoGroup = BeginUndoGroup("Clear Frames");

        foreach (CutsceneFrame frame in targetManager.frames)
        {
            if (frame.cameraPosition != null)
            {
                GameObject camParent = frame.cameraPosition.parent != null 
                    ? frame.cameraPosition.parent.gameObject 
                    : frame.cameraPosition.gameObject;
                
                Undo.DestroyObjectImmediate(camParent);
            }

            DestroyFramePoseClips(frame);
        }
        
        Undo.RegisterCompleteObjectUndo(targetManager, "Clear Frames");
        targetManager.frames.Clear();
        currentFrameIndex = 0;
        EditorUtility.SetDirty(targetManager);

        EndUndoGroup(undoGroup);
    }
    
    private void AddCharacterIfNotPresent(GameObject character)
    {
        if (!charactersToCapture.Contains(character))
        {
            charactersToCapture.Add(character);
        }
    }
    
    private AnimationClip CaptureCharacterPose(GameObject character, string clipName)
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = false;
        
        Transform[] allTransforms = character.GetComponentsInChildren<Transform>();
        
        foreach (Transform bone in allTransforms)
        {
            string path = AnimationUtility.CalculateTransformPath(bone, character.transform);
            
            AnimationCurve posXCurve = AnimationCurve.Constant(0, 0, bone.localPosition.x);
            AnimationCurve posYCurve = AnimationCurve.Constant(0, 0, bone.localPosition.y);
            AnimationCurve posZCurve = AnimationCurve.Constant(0, 0, bone.localPosition.z);
            
            clip.SetCurve(path, typeof(Transform), "localPosition.x", posXCurve);
            clip.SetCurve(path, typeof(Transform), "localPosition.y", posYCurve);
            clip.SetCurve(path, typeof(Transform), "localPosition.z", posZCurve);
            
            AnimationCurve rotXCurve = AnimationCurve.Constant(0, 0, bone.localRotation.x);
            AnimationCurve rotYCurve = AnimationCurve.Constant(0, 0, bone.localRotation.y);
            AnimationCurve rotZCurve = AnimationCurve.Constant(0, 0, bone.localRotation.z);
            AnimationCurve rotWCurve = AnimationCurve.Constant(0, 0, bone.localRotation.w);
            
            clip.SetCurve(path, typeof(Transform), "localRotation.x", rotXCurve);
            clip.SetCurve(path, typeof(Transform), "localRotation.y", rotYCurve);
            clip.SetCurve(path, typeof(Transform), "localRotation.z", rotZCurve);
            clip.SetCurve(path, typeof(Transform), "localRotation.w", rotWCurve);
            
            AnimationCurve scaleXCurve = AnimationCurve.Constant(0, 0, bone.localScale.x);
            AnimationCurve scaleYCurve = AnimationCurve.Constant(0, 0, bone.localScale.y);
            AnimationCurve scaleZCurve = AnimationCurve.Constant(0, 0, bone.localScale.z);
            
            clip.SetCurve(path, typeof(Transform), "localScale.x", scaleXCurve);
            clip.SetCurve(path, typeof(Transform), "localScale.y", scaleYCurve);
            clip.SetCurve(path, typeof(Transform), "localScale.z", scaleZCurve);
        }
        
        EnsureAnimationSavePath();
        
        string assetPath = animationSavePath + clipName + ".anim";
        AssetDatabase.CreateAsset(clip, assetPath);
        Undo.RegisterCreatedObjectUndo(clip, "Create Pose Clip");
        AssetDatabase.SaveAssets();
        
        return clip;
    }
    
    private void CaptureFrame(int insertIndex = -1)
    {
        if (targetManager == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a CutsceneManager!", "OK");
            return;
        }
        
        if (sceneCamera == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Camera to capture!", "OK");
            return;
        }
        
        // Use the insert index directly (0-based)
        int frameNumber = insertIndex >= 0 ? insertIndex : targetManager.frames.Count;
        
        // GameObject naming uses 0-based indexing
        string frameContainerName = string.IsNullOrEmpty(frameName) 
            ? $"Frame{frameNumber}_Positions" 
            : $"Frame{frameNumber}_{frameName}";
            
        GameObject frameContainer = new GameObject(frameContainerName);
        
        GameObject camPosMarker = new GameObject("CameraPosition");
        camPosMarker.transform.SetParent(frameContainer.transform);
        camPosMarker.transform.SetPositionAndRotation(
            sceneCamera.transform.position,
            sceneCamera.transform.rotation
        );
        
        CutsceneFrame newFrame = new CutsceneFrame();
        newFrame.frameName = frameName;
        newFrame.cameraPosition = camPosMarker.transform;
        newFrame.characters = new List<CharacterState>();
        
        foreach (GameObject character in charactersToCapture)
        {
            if (character != null)
            {
                GameObject charPosMarker = new GameObject($"{character.name}_Position");
                charPosMarker.transform.SetParent(frameContainer.transform);
                charPosMarker.transform.SetPositionAndRotation(
                    character.transform.position,
                    character.transform.rotation
                );
                
                string poseClipName = $"{frameContainerName}_{character.name}_Pose";
                AnimationClip poseClip = CaptureCharacterPose(character, poseClipName);
                
                CharacterState charState = new CharacterState
                {
                    characterName = character.name,
                    position = charPosMarker.transform,
                    poseClip = poseClip,
                    isActive = character.activeSelf
                };
                
                newFrame.characters.Add(charState);
            }
        }
        
        Undo.RecordObject(targetManager, "Capture Frame");
        
        // Insert at the specified index
        if (insertIndex >= 0 && insertIndex <= targetManager.frames.Count)
        {
            targetManager.frames.Insert(insertIndex, newFrame);
            currentFrameIndex = insertIndex;
        }
        else
        {
            targetManager.frames.Add(newFrame);
            currentFrameIndex = targetManager.frames.Count - 1;
        }
        
        EditorUtility.SetDirty(targetManager);

        RefreshFrameContainerNames();
        
        Debug.Log($"Frame '{frameContainerName}' captured with {newFrame.characters.Count} characters! Total frames: {targetManager.frames.Count}");
        
        charactersToCapture.Clear();
    }

    private void PopulateCharactersFromFrame(int index)
    {
        if (targetManager == null || index < 0 || index >= targetManager.frames.Count)
            return;

        CutsceneFrame frame = targetManager.frames[index];
        charactersToCapture.Clear();

        foreach (CharacterState charState in frame.characters)
        {
            if (charState == null || string.IsNullOrEmpty(charState.characterName))
                continue;

            GameObject character = FindCharacterByName(charState.characterName);
            if (character != null && !charactersToCapture.Contains(character))
            {
                charactersToCapture.Add(character);
            }
        }
    }

    private GameObject FindCharacterByName(string characterName)
    {
        if (targetManager == null) return null;

        if (targetManager.francisco != null && targetManager.francisco.name == characterName)
            return targetManager.francisco;
        if (targetManager.barnardo != null && targetManager.barnardo.name == characterName)
            return targetManager.barnardo;
        if (targetManager.marcellus != null && targetManager.marcellus.name == characterName)
            return targetManager.marcellus;
        if (targetManager.horatio != null && targetManager.horatio.name == characterName)
            return targetManager.horatio;
        if (targetManager.hamlet != null && targetManager.hamlet.name == characterName)
            return targetManager.hamlet;
        if (targetManager.ghost != null && targetManager.ghost.name == characterName)
            return targetManager.ghost;

        return GameObject.Find(characterName);
    }

    private string GetFrameTooltip(int index)
    {
        if (targetManager == null || index < 0 || index >= targetManager.frames.Count)
            return "";

        CutsceneFrame frame = targetManager.frames[index];
        string label = string.IsNullOrEmpty(frame.frameName)
            ? $"Frame {index + 1}"
            : $"Frame {index + 1}: {frame.frameName}";

        return $"{label}\nClick to preview this frame.";
    }

    private void RefreshFrameContainerNames()
    {
        if (targetManager == null) return;

        for (int i = 0; i < targetManager.frames.Count; i++)
        {
            CutsceneFrame frame = targetManager.frames[i];
            if (frame.cameraPosition == null) continue;

            Transform container = frame.cameraPosition.parent != null
                ? frame.cameraPosition.parent
                : frame.cameraPosition;

            string baseName = string.IsNullOrEmpty(frame.frameName)
                ? $"Frame{i}_Positions"
                : $"Frame{i}_{frame.frameName}";

            container.name = baseName;
        }
    }

    private AnimationClip DuplicatePoseClip(
        AnimationClip sourceClip,
        string frameContainerName,
        string characterName
    )
    {
        EnsureAnimationSavePath();

        AnimationClip newClip = Object.Instantiate(sourceClip);
        newClip.name = $"{frameContainerName}_{characterName}_Pose";

        string assetPath = animationSavePath + newClip.name + ".anim";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(newClip, assetPath);
        Undo.RegisterCreatedObjectUndo(newClip, "Duplicate Pose Clip");
        AssetDatabase.SaveAssets();

        return newClip;
    }

    private void DestroyFramePoseClips(CutsceneFrame frame)
    {
        if (frame == null || frame.characters == null) return;

        foreach (CharacterState characterState in frame.characters)
        {
            if (characterState != null && characterState.poseClip != null)
            {
                Undo.DestroyObjectImmediate(characterState.poseClip);
            }
        }
    }

    private int BeginUndoGroup(string name)
    {
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(name);
        return group;
    }

    private void EndUndoGroup(int group)
    {
        Undo.CollapseUndoOperations(group);
    }

    private void EnsureAnimationSavePath()
    {
        string trimmedPath = animationSavePath.TrimEnd('/');
        if (AssetDatabase.IsValidFolder(trimmedPath))
            return;

        string[] folders = trimmedPath.Split('/');
        string currentPath = folders[0];

        for (int i = 1; i < folders.Length; i++)
        {
            string newPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(newPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = newPath;
        }
    }
}
