using UnityEngine;
using Sirenix.OdinInspector; 

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterDataSO : ScriptableObject
{
    [BoxGroup("General Info")]
    public string characterName = "Test Player";

    [BoxGroup("Stats")]
    [ProgressBar(0, 200, ColorGetter = "GetHealthColor")]
    public int maxHealth = 100;

    [BoxGroup("Movement")]
    [Range(1f, 10f)]
    public float moveSpeed = 5.0f;

    [BoxGroup("Movement")]
    [Range(1f, 15f)]
    public float sprintSpeed = 8.0f;

    [BoxGroup("Movement")]
    [Range(1f, 20f)] 
    public float jumpForce = 7.0f;

    [BoxGroup("Controls")]
    public float rotationSensitivity = 0.5f;

    [BoxGroup("Physics")]
    public float groundCheckRadius = 0.3f;

    [BoxGroup("Physics")]
    public LayerMask groundLayer;

    private Color GetHealthColor(int value)
    {
        return Color.Lerp(Color.red, Color.green, value / 200f);
    }
}