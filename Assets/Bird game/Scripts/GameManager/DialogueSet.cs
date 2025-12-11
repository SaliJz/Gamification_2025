using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de diálogos con variantes múltiples para la Gaviota de Franklin.
/// Usa ScriptableObject para fácil configuración.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueSet", menuName = "Gameplay/Dialogue Set")]
public class DialogueSet : ScriptableObject
{
    [Header("Dialogue Configuration")]
    [Tooltip("Nombre del conjunto de diálogos")]
    public string dialogueSetName;

    [Header("Dialogue Variants")]
    [Tooltip("Diferentes textos que aparecen al hacer clic múltiples veces")]
    [TextArea(3, 10)]
    public List<string> dialogueVariants = new List<string>();

    private int currentIndex = 0;

    /// <summary>
    /// Obtiene el siguiente diálogo en la secuencia
    /// </summary>
    public string GetNextDialogue()
    {
        if (dialogueVariants == null || dialogueVariants.Count == 0)
        {
            return "...";
        }

        string dialogue = dialogueVariants[currentIndex];
        currentIndex = (currentIndex + 1) % dialogueVariants.Count;

        return dialogue;
    }

    /// <summary>
    /// Reinicia el índice al principio
    /// </summary>
    public void Reset()
    {
        currentIndex = 0;
    }

    /// <summary>
    /// Obtiene un diálogo aleatorio
    /// </summary>
    public string GetRandomDialogue()
    {
        if (dialogueVariants == null || dialogueVariants.Count == 0)
        {
            return "...";
        }

        return dialogueVariants[Random.Range(0, dialogueVariants.Count)];
    }
}