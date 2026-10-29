using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Pressione F para Abrir";
    [SerializeField] private float rotationAngle = 90f;
    [SerializeField] private float rotationSpeed = 2f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, rotationAngle, 0);
    }

    public string GetInteractionPrompt()
    {
        return isOpen ? "Pressione F para Fechar" : prompt;
    }

    public void Interact()
    {
        isOpen = !isOpen;
        Debug.Log($"Porta {(isOpen ? "aberta" : "fechada")}!");
        transform.rotation = isOpen ? openRotation : closedRotation;
    }

}