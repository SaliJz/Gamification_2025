using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sistema de diálogos para mostrar información educativa sobre la Gaviota de Franklin.
/// Incluye variantes modulables que cambian al hacer clic repetidamente.
/// </summary>
public class DialogueBox : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject franklinBirdSprite;

    [Header("Animation Settings")]
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private bool autoAdvance = false;
    [SerializeField] private float autoAdvanceDelay = 3f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip textSound;
    [SerializeField] private AudioClip completeSound;
    private AudioSource audioSource;

    private bool isTyping = false;
    private bool isWaitingForInput = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;

    public bool IsWaitingForInput => isWaitingForInput;
    public bool IsTyping => isTyping;

    #region Unity Lifecycle
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        HideDialogue();
    }
    #endregion

    #region Show/Hide Dialogue
    public void ShowDialogue(string text)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (franklinBirdSprite != null)
        {
            franklinBirdSprite.SetActive(true);
        }

        currentFullText = text;
        isWaitingForInput = true;

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(text));
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (franklinBirdSprite != null)
        {
            franklinBirdSprite.SetActive(false);
        }

        isWaitingForInput = false;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
    }
    #endregion

    #region Text Typing
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;

            if (textSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(textSound, 0.1f);
            }

            yield return new WaitForSecondsRealtime(textSpeed);
        }

        isTyping = false;

        if (completeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completeSound);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }

        if (autoAdvance)
        {
            yield return new WaitForSecondsRealtime(autoAdvanceDelay);
            OnContinueClicked();
        }
    }

    public void SkipTyping()
    {
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullText;
            isTyping = false;

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
            }
        }
    }
    #endregion

    #region Button Callbacks
    private void OnContinueClicked()
    {
        if (isTyping)
        {
            SkipTyping();
        }
        else
        {
            isWaitingForInput = false;
            HideDialogue();
        }
    }

    public void OnBirdClicked()
    {
        if (isTyping)
        {
            SkipTyping();
        }
    }
    #endregion
}