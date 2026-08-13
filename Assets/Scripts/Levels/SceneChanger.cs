using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneChanger : MonoBehaviour
{
  [SerializeField] private Object sceneToLoad;
  [SerializeField] private GameObject player;

  private void OnTriggerEnter2D(Collider2D other)
  {
    // Check if the object entering the trigger is the player
    if (other.gameObject == player)
    {
      SceneManager.LoadScene(sceneToLoad.name);
    }
  }
}
