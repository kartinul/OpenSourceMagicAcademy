using UnityEngine;

public class PCPlatformChecker : MonoBehaviour
{
  void Start()
  {
    if (Application.isMobilePlatform)
    {
      gameObject.SetActive(false);
    }
  }
}
