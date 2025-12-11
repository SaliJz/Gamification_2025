using UnityEngine;

/// <summary>
/// Component que maneja los clics en el sprite del ave Franklin
/// para mostrar diálogos educativos.
/// </summary>
public class FranklinBirdInteractive : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    [SerializeField] private DialogueSet mainMenuDialogues;
    [SerializeField] private DialogueSet gameOverDialogues;
    [SerializeField] private DialogueBox dialogueBox;

    [Header("Click Detection")]
    [SerializeField] private bool isMainMenu = true; // Determina si está en el menú principal o en la pantalla de game over

    private void OnMouseDown()
    {
        if (dialogueBox == null) return;

        DialogueSet currentSet = isMainMenu ? mainMenuDialogues : gameOverDialogues;

        if (currentSet != null)
        {
            string dialogue = currentSet.GetNextDialogue();
            dialogueBox.ShowDialogue(dialogue);
        }
    }

    public void SetDialogueMode(bool mainMenu)
    {
        isMainMenu = mainMenu;
    }
}