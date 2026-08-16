using System.Collections;
using UnityEngine;
using TMPro;

public class Combatant : MonoBehaviour
{
    [Header("Stats")]
    public string combatantName = "Combatant";
    public int maxHP = 100;

    [HideInInspector]
    public int currentHP;
    public bool IsDead => currentHP <= 0;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float flashDuration = 0.05f;
    [SerializeField] private float knockbackDistance = 0.35f;
    [SerializeField] private float knockbackDuration = 0.12f;

    private Material materialInstance;
    private static readonly int FlashAmountProperty = Shader.PropertyToID("_FlashAmount");
    private Coroutine hitFeedbackCoroutine;

    public BattleManager battleManager;

    void Awake()
    {
        currentHP = maxHP;

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            materialInstance = spriteRenderer.material;
        }
    }

    public void TakeDamage(int amount, Transform attackerTransform = null)
    {
        amount = Mathf.Max(0, amount);

        currentHP -= amount;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"{combatantName} took {amount} damage!");

        PlayHitFeedback(attackerTransform);
    }

    public void Heal(int amount)
    {
        amount = Mathf.Max(0, amount);

        currentHP += amount;
        currentHP = Mathf.Min(maxHP, currentHP);

        Debug.Log($"{combatantName} healed {amount} HP!");
    }

    public void PlayHitFeedback(Transform attackerTransform = null)
    {
        if (hitFeedbackCoroutine != null)
            StopCoroutine(hitFeedbackCoroutine);

        hitFeedbackCoroutine = StartCoroutine(HitSequenceRoutine(attackerTransform));
    }

    private IEnumerator HitSequenceRoutine(Transform attackerTransform)
    {
        Vector3 startWorldPos = transform.position;
        Vector3 worldPushDir;

        if (attackerTransform != null)
        {

            Vector2 diff = transform.position - attackerTransform.position;

            if (diff.sqrMagnitude > 0.0001f)
            {
                worldPushDir = diff.normalized;
            }
            else
            {
                worldPushDir = (transform.position.x >= attackerTransform.position.x) ? Vector3.right : Vector3.left;
            }
        }
        else
        {
            worldPushDir = -transform.right;
        }

        Vector3 targetWorldPos = startWorldPos + (worldPushDir * knockbackDistance);

        Vector3 startLocalPos = transform.localPosition;
        Vector3 targetLocalPos = transform.parent != null 
            ? transform.parent.InverseTransformPoint(targetWorldPos) 
            : targetWorldPos;

        Coroutine flashCoroutine = StartCoroutine(FlashRoutine());

        float elapsed = 0f;
        float halfDuration = knockbackDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, elapsed / halfDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(targetLocalPos, startLocalPos, elapsed / halfDuration);
            yield return null;
        }

        transform.localPosition = startLocalPos;
        yield return flashCoroutine;
    }

    private IEnumerator FlashRoutine()
    {
        if (materialInstance == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            materialInstance.SetFloat(FlashAmountProperty, 1f);
            yield return new WaitForSeconds(flashDuration);

            materialInstance.SetFloat(FlashAmountProperty, 0f);
            yield return new WaitForSeconds(flashDuration);
        }

        materialInstance.SetFloat(FlashAmountProperty, 0f);
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }

    [SerializeField] private Spell spellOnVictory;

    public void RegisterVictoryReward()
    {
        battleManager.OnVictory.RemoveListener(SpellOnVictory);
        battleManager.OnVictory.AddListener(SpellOnVictory);
    }

    public void SpellOnVictory()
    {
        if (spellOnVictory == null)
            return;

        SpellBook spellBook = FindFirstObjectByType<SpellBook>();

        if (spellBook != null)
            spellBook.UnlockSpell(spellOnVictory);
    }
}