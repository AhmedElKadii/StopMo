using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

[System.Serializable]
public class CharacterState
{
    public string characterName;
    public Transform position;
    public AnimationClip poseClip;
    public bool isActive = true;
}

[System.Serializable]
public class CutsceneFrame
{
    public string frameName;
    public Transform cameraPosition;
    public List<CharacterState> characters = new List<CharacterState>();
    public float duration = 2f;
}

public class CutsceneManager : MonoBehaviour
{
    public List<CutsceneFrame> frames = new List<CutsceneFrame>();
    public Camera mainCamera;
	public DialogueManager dm;
    
    [Header("Character References")]
    public GameObject francisco;
    public GameObject barnardo;
    public GameObject marcellus;
    public GameObject horatio;
    public GameObject hamlet;
    public GameObject ghost;
    
    private Dictionary<string, GameObject> characterObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, Animator> characterAnimators = new Dictionary<string, Animator>();
    
    private void Awake()
    {
        EnsureCharacterCache();
    }

	void Start()
	{
		Play();
	}
    
    private void RegisterCharacter(string name, GameObject character)
    {
        if (character != null)
        {
            characterObjects[name] = character;
            
            // Try to get Animator component
            Animator animator = character.GetComponent<Animator>();
            if (animator != null)
            {
                characterAnimators[name] = animator;
            }
        }
    }
    
    public void ApplyFrame(int frameIndex)
    {
        if (characterObjects.Count == 0)
        {
            EnsureCharacterCache();
        }

        if (frameIndex < 0 || frameIndex >= frames.Count)
        {
            Debug.LogWarning($"Frame index {frameIndex} out of range!");
            return;
        }
        
        CutsceneFrame frame = frames[frameIndex];
        
        // Apply camera position
        if (frame.cameraPosition != null && mainCamera != null)
        {
            mainCamera.transform.SetPositionAndRotation(
                frame.cameraPosition.position,
                frame.cameraPosition.rotation
            );
        }
        
        HashSet<string> charactersInFrame = new HashSet<string>();
        
        // Apply character positions and poses
        foreach (CharacterState charState in frame.characters)
        {
            charactersInFrame.Add(charState.characterName);
            
            if (characterObjects.TryGetValue(charState.characterName, out GameObject character))
            {
                if (character != null)
                {
                    // Apply root position
                    if (charState.position != null)
                    {
                        character.transform.SetPositionAndRotation(
                            charState.position.position,
                            charState.position.rotation
                        );
                    }
                    
                    // Apply skeletal pose
                    if (charState.poseClip != null)
                    {
                        ApplyPoseToCharacter(character, charState.poseClip);
                    }
                    
                    character.SetActive(charState.isActive);
                }
            }
        }
        
        // Hide characters NOT in this frame
        foreach (var kvp in characterObjects)
        {
            if (!charactersInFrame.Contains(kvp.Key) && kvp.Value != null)
            {
                kvp.Value.SetActive(false);
            }
        }
    }
    
    private void ApplyPoseToCharacter(GameObject character, AnimationClip clip)
    {
        // Sample the animation clip at time 0 to apply the pose
        clip.SampleAnimation(character, 0f);
    }

    private void EnsureCharacterCache()
    {
        characterObjects.Clear();
        characterAnimators.Clear();

        RegisterCharacter("Francisco", francisco);
        RegisterCharacter("Barnardo", barnardo);
        RegisterCharacter("Marcellus", marcellus);
        RegisterCharacter("Horatio", horatio);
        RegisterCharacter("Hamlet", hamlet);
        RegisterCharacter("Ghost", ghost);
    }

	public async Task Play()
	{
		ApplyFrame(0);
		await Task.Delay(3000);
		ApplyFrame(1);
		await Task.Delay(3000);
		ApplyFrame(2);
		await Task.Delay(3000);
		ApplyFrame(3);
		dm.Dialogue(0);
		await dm.parleyYaml.OnNextDialogue;
		ApplyFrame(4);
		await dm.parleyYaml.OnNextDialogue;
	}
}
