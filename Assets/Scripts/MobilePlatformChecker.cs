using UnityEngine;

public class MobilePlatformChecker : MonoBehaviour
{
  public bool showOnlyOnMobile = true;

  void Start()
  {
    bool isMobile = Application.isMobilePlatform;

    if (showOnlyOnMobile)
    {
      gameObject.SetActive(isMobile);
    }
    else
    {
      gameObject.SetActive(!isMobile);
    }
  }
}
