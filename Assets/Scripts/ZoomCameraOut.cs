using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZoomCameraOut : MonoBehaviour
{

  [SerializeField] private float cameraDistance = 13f;
  [SerializeField] private float zoomSpeed = 2f;

  private CinemachinePositionComposer positionComposer;
  private Coroutine zoomCoroutine;
  private float initialCameraDistance;

  void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.gameObject.GetComponent<Player>() != null)
    {
      if (positionComposer == null)
      {
        positionComposer = collision.GetComponentInChildren<CinemachinePositionComposer>();
        if (positionComposer != null)
        {
          initialCameraDistance = positionComposer.CameraDistance;
        }
      }

      if (positionComposer != null)
      {
        if (zoomCoroutine != null)
        {
          StopCoroutine(zoomCoroutine);
        }
        zoomCoroutine = StartCoroutine(SmoothZoom(cameraDistance));
      }
    }
  }

  void OnTriggerExit2D(Collider2D collision)
  {
    if (collision.gameObject.GetComponent<Player>() != null)
    {
      if (positionComposer != null)
      {
        if (zoomCoroutine != null)
        {
          StopCoroutine(zoomCoroutine);
        }
        if (gameObject.activeInHierarchy)
        {
          zoomCoroutine = StartCoroutine(SmoothZoom(initialCameraDistance));
        }
      }
    }
  }

  private IEnumerator SmoothZoom(float targetDistance)
  {
    while (Mathf.Abs(positionComposer.CameraDistance - targetDistance) > 0.01f)
    {
      positionComposer.CameraDistance = Mathf.Lerp(positionComposer.CameraDistance, targetDistance, Time.deltaTime * zoomSpeed);
      yield return null;
    }

    positionComposer.CameraDistance = targetDistance;
  }
}