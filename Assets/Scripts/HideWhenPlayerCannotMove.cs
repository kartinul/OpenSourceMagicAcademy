using UnityEngine;

public class HideWhenPlayerCannotMove : MonoBehaviour
{
  private Player player;
  private Canvas canvas;

  void Start()
  {
    canvas = GetComponent<Canvas>();
  }

  private float searchTimer = 0f;

  void Update()
  {
    if (player == null)
    {
      searchTimer += Time.deltaTime;
      if (searchTimer >= 1f) // check once a second
      {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.GetComponent<Player>();
        searchTimer = 0f;
      }
    }

    if (player != null && canvas != null)
    {
      canvas.enabled = player.canMove;
    }
  }
}
