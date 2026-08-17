using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public static class EventHelpers
{
    private static System.Collections.Generic.Dictionary<string, object> originalZones = new System.Collections.Generic.Dictionary<string, object>();

    /// <summary>
    /// Generates a default camera shake based on the impulse source attached to the Player.
    /// </summary>
    public static void ShakeScreen(float duration = 0.5f)
    {
        if (Player.Instance == null) return;
        var impulseSource = Player.Instance.GetComponentInChildren<CinemachineImpulseSource>();

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
    public static void ShakeScreenWithForce(float force, float duration = 0.5f)
    {
        if (Player.Instance == null) return;
        var impulseSource = Player.Instance.GetComponentInChildren<CinemachineImpulseSource>();

        if (impulseSource != null)
        {
            impulseSource.ImpulseDefinition.ImpulseDuration = duration;
            impulseSource.GenerateImpulseWithForce(force);
        }
    }

    /// <summary>
    /// Logs a custom message to the console. Useful for debugging UnityEvents.
    /// </summary>
    public static void LogMessage(string message)
    {
        Debug.Log($"[EventHelpers] {message}");
    }

    /// <summary>
    /// Spawns a prefab at a specific position.
    /// </summary>
    public static void SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab != null)
        {
            GameObject.Instantiate(prefab, position, rotation);
        }
    }

    /// <summary>
    /// Flashes the screen white briefly.
    /// </summary>
    public static void FlashScreenWhite()
    {
        if (Player.Instance != null)
        {
            Player.Instance.StartCoroutine(FlashRoutine(Color.white, 0.5f));
        }
    }

    /// <summary>
    /// Flashes the screen red briefly.
    /// </summary>
    public static void FlashScreenRed()
    {
        if (Player.Instance != null)
        {
            Player.Instance.StartCoroutine(FlashRoutine(Color.red, 0.5f));
        }
    }

    /// <summary>
    /// Adds a GameObject to the Player's CinemachineTargetGroup so the camera focuses on it.
    /// </summary>
    public static void FocusCameraOn(GameObject target)
    {
        FocusCameraOnWithZoom(target, 10f);
    }

    /// <summary>
    /// Advanced version for scripts that want to specify exactly how much to zoom out.
    /// </summary>
    public static void FocusCameraOnWithZoom(GameObject target, float zoomOut)
    {
        if (target == null)
        {
            Debug.LogError("EventHelpers: FocusCameraOn was called, but the Target GameObject is missing/null!");
            return;
        }

        if (Player.Instance == null)
        {
            Debug.LogError("EventHelpers: Player.Instance is null, cannot focus camera!");
            return;
        }

        var vcam = Player.Instance.GetComponentInChildren<CinemachineCamera>();
        if (vcam != null)
        {
            // Directly hijack the camera's tracking target! This is 100% reliable.
            vcam.Target.TrackingTarget = target.transform;
        }
        else
        {
            Debug.LogWarning("EventHelpers: No CinemachineCamera found in Player children!");
        }

        var composer = Player.Instance.GetComponentInChildren<CinemachinePositionComposer>();
        if (composer != null)
        {
            // Directly change the camera distance to zoom out
            composer.CameraDistance = zoomOut;

            // Aggressively center the camera by zeroing out any dead/soft zones.
            // Using reflection makes this immune to Cinemachine 2 vs 3 API differences.
            if (originalZones.Count == 0)
            {
                foreach (var prop in composer.GetType().GetProperties())
                {
                    if (prop.Name.Contains("Zone") && prop.CanWrite && (prop.PropertyType == typeof(Vector2) || prop.PropertyType == typeof(float)))
                    {
                        originalZones[prop.Name] = prop.GetValue(composer);
                        if (prop.PropertyType == typeof(Vector2)) prop.SetValue(composer, Vector2.zero);
                        if (prop.PropertyType == typeof(float)) prop.SetValue(composer, 0f);
                    }
                }
                foreach (var field in composer.GetType().GetFields())
                {
                    if (field.Name.Contains("Zone") && !field.IsInitOnly && (field.FieldType == typeof(Vector2) || field.FieldType == typeof(float)))
                    {
                        originalZones[field.Name] = field.GetValue(composer);
                        if (field.FieldType == typeof(Vector2)) field.SetValue(composer, Vector2.zero);
                        if (field.FieldType == typeof(float)) field.SetValue(composer, 0f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Removes all extra targets from the Player's CinemachineTargetGroup, leaving only the Player.
    /// </summary>
    public static void ClearCameraFocus()
    {
        if (Player.Instance == null) return;

        var vcam = Player.Instance.GetComponentInChildren<CinemachineCamera>();
        if (vcam != null)
        {
            // Revert back to the Player
            vcam.Target.TrackingTarget = Player.Instance.transform;
        }

        var composer = Player.Instance.GetComponentInChildren<CinemachinePositionComposer>();
        if (composer != null)
        {
            // Revert to a default distance (or whatever the standard gameplay distance is)
            composer.CameraDistance = 10f; // Default gameplay distance

            if (originalZones.Count > 0)
            {
                foreach (var kvp in originalZones)
                {
                    var prop = composer.GetType().GetProperty(kvp.Key);
                    if (prop != null && prop.CanWrite) prop.SetValue(composer, kvp.Value);

                    var field = composer.GetType().GetField(kvp.Key);
                    if (field != null && !field.IsInitOnly) field.SetValue(composer, kvp.Value);
                }
                originalZones.Clear();
            }
        }
    }

    private static IEnumerator FlashRoutine(Color color, float duration)
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

        GameObject.Destroy(canvasObj);
    }
}
