using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/Player")]
public class PlayerSO : ScriptableObject
{
  public AnimationClip idleFront;
  public AnimationClip idleBack;
  public AnimationClip idleLeft;
  public AnimationClip idleRight;
  public AnimationClip walkFront;
  public AnimationClip walkBack;
  public AnimationClip walkLeft;
  public AnimationClip walkRight;
}