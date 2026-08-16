using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneChanger : MonoBehaviour
{
  [SerializeField] private Object sceneToLoad;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.GetComponent<Player>() != null)
    {
      SceneManager.LoadScene(sceneToLoad.name);
    }
  }
}
