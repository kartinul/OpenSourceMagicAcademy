using UnityEngine;
using UnityEngine.Tilemaps;

public class SetSpriteLayer : MonoBehaviour
{
  public int sortingOrder = 5;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    foreach (SpriteRenderer s in gameObject.GetComponentsInChildren<SpriteRenderer>())
    {
      s.sortingOrder = sortingOrder;
    }
    foreach (TilemapRenderer s in gameObject.GetComponentsInChildren<TilemapRenderer>())
    {
      s.sortingOrder = sortingOrder;
    }
  }

}
