using UnityEngine;

public class RedutorEstresseInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Pressione F para Pegar";
    [SerializeField] private string itemName = "Chave";

    public string GetInteractionPrompt()
    {
        return prompt;
    }

    public void Interact()
    {
        Debug.Log($"Pegou o item: {itemName}!");
        gameObject.SetActive(false);
    }
}