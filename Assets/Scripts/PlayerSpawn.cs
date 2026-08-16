using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
  public int spawnId = 0;

  private void Awake()
  {
    GameObject player = GameObject.FindGameObjectWithTag("Player");

    if (player == null)
    {
      Debug.LogWarning("[PlayerSpawn] No Player found.");
      return;
    }

    player.transform.position = transform.position;
    player.transform.rotation = transform.rotation;
  }
}