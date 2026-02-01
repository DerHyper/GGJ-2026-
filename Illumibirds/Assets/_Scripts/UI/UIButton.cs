using System;
using System.Collections;
using UnityEngine;


public class UIButton : MonoBehaviour
{
    [SerializeField] AudioClip hoverSound, clickSound;
    [SerializeField] float scaleBoost = 0.2f;
    [SerializeField] float scaleSpeed = 8f;

    bool isActive;
    Vector3 originalScale;
    Coroutine scaleCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetActive(bool active, bool instant = false)
    {
        isActive = active;
        Vector3 targetScale = active ? originalScale + Vector3.one * scaleBoost : originalScale;
        Debug.Log($"[UIButton] {name} SetActive({active}) - originalScale: {originalScale}, targetScale: {targetScale}, currentScale: {transform.localScale}");

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        if (instant)
        {
            transform.localScale = targetScale;
        }
        else
        {
            scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
        }
    }

    IEnumerator ScaleCoroutine(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    public void OnPointerEnter()
    {
        Debug.Log($"[UIButton] {name} OnPointerEnter - MainMenu.Instance is {(MainMenu.Instance != null ? "valid" : "NULL")}");
        MainMenu.Instance?.SetActiveItem(this);
    }

    public void OnPointerExit()
    {
        Debug.Log($"[UIButton] {name} OnPointerExit");
        MainMenu.Instance?.ClearActiveItem(this);
    }

    public void PlayHoverSound()
    {
        if(TryGetComponent<AudioSource>(out AudioSource source))
        {
            if(hoverSound)
            source.PlayOneShot(hoverSound);
        }
    }


    public void PlayClickSound()
    {
        if(TryGetComponent<AudioSource>(out AudioSource source))
        {
            if(clickSound)
            source.PlayOneShot(clickSound);
        }
    }
}
