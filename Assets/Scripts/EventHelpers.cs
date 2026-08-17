using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using UnityEngine.UI;

public class EventHelpers : MonoBehaviour
{
    private CinemachineImpulseSource impulseSource;

    private void Start()
    {
        // Auto-fetch from the Player Singleton instance right away
        if (Player.Instance != null)
        {
            impulseSource = Player.Instance.GetComponentInChildren<CinemachineImpulseSource>();
        }
    }

    /// <summary>
    /// Generates a default camera shake based on the impulse source attached to the Player.
    /// </summary>
    public void ShakeScreen(float duration = 0.5f)
    {
        if (impulseSource != null)
        {
            impulseSource.ImpulseDefinition.ImpulseDuration = duration;

            if (impulseSource.DefaultVelocity == Vector3.zero)
            {
                impulseSource.GenerateImpulseWithVelocity(new Vector3(1f, 1f, 0f));
            }
            else
            {
                impulseSource.GenerateImpulse();
            }
        }
        else
        {
            Debug.LogWarning("EventHelpers: No CinemachineImpulseSource found in Player children for ShakeScreen.");
        }
    }

    /// <summary>
    /// Generates a camera shake with a specific force.
    /// </summary>
    public void ShakeScreenWithForce(float force, float duration = 0.5f)
    {
        if (impulseSource != null)
        {
            impulseSource.ImpulseDefinition.ImpulseDuration = duration;
            impulseSource.GenerateImpulseWithForce(force);
        }
    }

    /// <summary>
    /// Logs a custom message to the console. Useful for debugging UnityEvents.
    /// </summary>
    public void LogMessage(string message)
    {
        Debug.Log($"[EventHelpers] {message}");
    }

    /// <summary>
    /// Spawns a prefab at the position of this GameObject.
    /// </summary>
    public void SpawnPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            Instantiate(prefab, transform.position, transform.rotation);
        }
    }

    /// <summary>
    /// Flashes the screen white briefly.
    /// </summary>
    public void FlashScreenWhite()
    {
        StartCoroutine(FlashRoutine(Color.white, 0.5f));
    }

    /// <summary>
    /// Flashes the screen red briefly.
    /// </summary>
    public void FlashScreenRed()
    {
        StartCoroutine(FlashRoutine(Color.red, 0.5f));
    }

    /// <summary>
    /// Adds a GameObject to the Player's CinemachineTargetGroup so the camera focuses on it.
    /// This 1-argument version shows up in the Unity Inspector. It uses a default zoom out of 10.
    /// </summary>
    public void FocusCameraOn(GameObject target)
    {
        FocusCameraOnWithZoom(target, 10f);
    }

    /// <summary>
    /// Advanced version for scripts that want to specify exactly how much to zoom out.
    /// </summary>
    public void FocusCameraOnWithZoom(GameObject target, float zoomOut)
    {
        if (target == null || Player.Instance == null) return;

        CinemachineTargetGroup targetGroup = Player.Instance.GetComponentInChildren<CinemachineTargetGroup>();
        if (targetGroup != null)
        {
            // Clear the player and any other targets so the camera centers perfectly on the new target
            targetGroup.Targets.Clear();

            // Using zoomOut as the radius forces the camera to frame it wider (zoom out)
            targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = target.transform, Weight = 1f, Radius = zoomOut });
        }
        else
        {
            Debug.LogWarning("EventHelpers: No CinemachineTargetGroup found in Player children for FocusCameraOn.");
        }
    }

    /// <summary>
    /// Removes all extra targets from the Player's CinemachineTargetGroup, leaving only the Player.
    /// </summary>
    public void ClearCameraFocus()
    {
        if (Player.Instance == null) return;

        CinemachineTargetGroup targetGroup = Player.Instance.GetComponentInChildren<CinemachineTargetGroup>();
        if (targetGroup != null)
        {
            targetGroup.Targets.Clear();
            targetGroup.Targets.Add(new CinemachineTargetGroup.Target { Object = Player.Instance.transform, Weight = 1f, Radius = 1f });
        }
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {
        GameObject canvasObj = new GameObject("ScreenFlashCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        Image image = canvasObj.AddComponent<Image>();
        image.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
            yield return null;
        }

        Destroy(canvasObj);
    }
}
