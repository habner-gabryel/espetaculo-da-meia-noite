using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    [Header("Configurações de Detecção")]
    [SerializeField] private float interactionRange = 3.5f; // Alcance da interação
    [SerializeField] private LayerMask interactableLayer = ~0; // Camadas que serão detectadas

    [Header("Feedback Visual")]
    [SerializeField] private GameObject interactionPromptUI; // O GameObject do UI Text/Panel

    // Referência ao objeto que está sendo mirado
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;

    private void Update()
    {
        // 1. Dispara o Raycast do centro da tela
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hitInfo, interactionRange, interactableLayer);

        // 2. Verifica se o objeto atingido é interagível
        if (hitSomething) {
            IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                // Um objeto interagível foi encontrado!
                if (currentInteractable != interactable) {
                    // Se mudou de objeto, atualiza as referências e o prompt
                    currentInteractable = interactable;
                    currentInteractableObject = hitInfo.collider.gameObject;
                    ShowInteractionPrompt(interactable.GetInteractionPrompt());
                }
                return; // Sai do método, pois encontrou algo
            }
        }

        if (currentInteractable != null) {
            currentInteractable = null;
            currentInteractableObject = null;
            HideInteractionPrompt();
        }
    }

    // Método chamado pela ação da tecla F (configurado no passo 4)
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Garante que a ação só é executada quando a tecla é pressionada (e não segurada)
        if (!context.performed) return;

        if (currentInteractable != null) {
            currentInteractable.Interact();
            Debug.Log($"Interagiu com: {currentInteractableObject.name}");
        } else {
            Debug.Log("Nada para interagir.");
        }
    }

    // Métodos para controlar o prompt visual
    private void ShowInteractionPrompt(string promptText)
    {
        if (interactionPromptUI != null) {
            TMPro.TextMeshProUGUI textComponent = interactionPromptUI.GetComponent<TMPro.TextMeshProUGUI>();
            if (textComponent != null) {
                textComponent.text = promptText;
            }
            interactionPromptUI.SetActive(true);
        }
    }

    private void HideInteractionPrompt()
    {
        if (interactionPromptUI != null) {
            interactionPromptUI.SetActive(false);
        }
    }

    // Desenha o gizmo para visualizar o alcance no Editor
    private void OnDrawGizmosSelected()
    {
        if (Camera.main != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * interactionRange);
        }
    }

	private void OnEnable()
	{
		// Assina o evento da ação 'Interact' ao método OnInteract
		// Assumindo que você tenha uma referência ao PlayerControls
		// A maneira mais fácil: fazer seu PlayerController expor o inputActions
		// Ou, se preferir, crie uma variável pública e atribua no Inspector.
	}

	private void OnDisable()
	{
		// Descansa a assinatura
	}
}