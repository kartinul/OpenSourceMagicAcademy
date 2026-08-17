using UnityEngine;

public class HideWhenPlayerCannotMove : MonoBehaviour
{
  private Player player;
  private Canvas canvas;

  void Start()
  {
    canvas = GetComponent<Canvas>();
  }

  void Update()
  {
    if (player == null)
    {
      GameObject p = GameObject.FindWithTag("Player");
      if (p != null) player = p.GetComponent<Player>();
    }

    if (player != null && canvas != null)
    {
      canvas.enabled = player.canMove;
    }
  }
}
