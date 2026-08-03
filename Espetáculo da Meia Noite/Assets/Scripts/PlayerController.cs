using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Agachamento")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;

    [Header("Camera (Opcional)")]
    [SerializeField] private Transform cameraTransform; // Arraste a camera aqui
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Física")]
    [SerializeField] private float maxFallSpeed = -20f;

    // Componentes e variaveis internas
    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;
    private bool isSprinting;
    private bool isCrouching;
    private float currentSpeed;

    // Referencias para itens
    private GameObject currentItem; // Ex: arma/lanterna equipada

    private void Awake()
    {
        // Instancia o Input Actions gerado
        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();

        // Trava e esconde o cursor para jogos 3D
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        // Habilita o input system e assina os eventos
        inputActions.Enable();

        // Subscribe dos botoes que precisam de acao imediata (performed/canceled)
        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        inputActions.Player.Crouch.performed += ctx => ToggleCrouch();
        
        inputActions.Player.Interact.performed += ctx => Interact();
        inputActions.Player.Pause.performed += ctx => PauseGame();
        inputActions.Player.SkillCheck.performed += ctx => SkillCheck();

        inputActions.Player.Weapon1.performed += ctx => SwitchWeapon(0);
        inputActions.Player.Weapon2.performed += ctx => SwitchWeapon(1);
        inputActions.Player.Weapon3.performed += ctx => SwitchWeapon(2);

        inputActions.Player.PrimaryAction.performed += ctx => PrimaryAction();
        inputActions.Player.SecondaryAction.performed += ctx => SecondaryAction();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        // 1. Captura os inputs continuos (movimento e olhar)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // 2. Aplica rotacao da camera (olhar)
        ApplyLook();

        // 3. Calcula e aplica o movimento
        ApplyMovement();
    }

    #region LOGICA DE MOVIMENTO E FISICA

    private void ApplyMovement()
    {
        // Calcula a velocidade baseada no estado (correndo/andando/agachado)
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

        // Pega a direcao relativa à rotacao do personagem
        Vector3 moveDirection = (transform.right * moveInput.x) + (transform.forward * moveInput.y);
        moveDirection.Normalize();

        // Aplica gravidade (para manter o personagem no chao)
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Pequeno valor para manter contato
        } else {
            verticalVelocity += gravity * Time.deltaTime;
            
            if (verticalVelocity < maxFallSpeed)
                verticalVelocity = maxFallSpeed;
        }

        // Monta o vetor final de movimento
        Vector3 finalMovement = moveDirection * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(finalMovement * Time.deltaTime);

        // Ajuste de altura do agachamento
        controller.height = isCrouching ? crouchHeight : standingHeight;
    }

    private void ApplyLook()
    {
        if (cameraTransform == null) return;

        // Rotacao horizontal (personagem)
        float rotX = lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * rotX);

        // Rotacao vertical (camera) - limitada para nao virar de ponta-cabeca
        float rotY = -lookInput.y * mouseSensitivity * Time.deltaTime;
        Vector3 currentCamRot = cameraTransform.localEulerAngles;
        float newRotX = currentCamRot.x + rotY;
        newRotX = Mathf.Clamp(newRotX, -90f, 90f);
        cameraTransform.localEulerAngles = new Vector3(newRotX, 0f, 0f);
    }

    #endregion

    #region ACOES DOS BOTOES (EVENTOS)

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        Debug.Log($"Agachamento: {(isCrouching ? "Ativado" : "Desativado")}");
    }

    private void Interact()
    {
        Debug.Log("Interagir (F) - Executando Raycast para objetos...");
        // Exemplo: Dispara um Raycast da camera para frente e verifica interface IInteractable
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 3f))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
        }
    }

    private void PauseGame()
    {
        Debug.Log("Pausar (Esc) - Alternando Time.timeScale");
        // Lógica basica de pause (voce vai integrar com seu menu de pause depois)
        Time.timeScale = Time.timeScale == 1 ? 0 : 1;
    }

    private void SkillCheck()
    {
        Debug.Log("Teste de Salva-Guarda (Espaco) - Rolando dados...");
        // Aqui voce chama o sistema de dados / UI de skill check
        // Exemplo: UIManager.Instance.RollD20();
    }

    private void SwitchWeapon(int slotIndex)
    {
        Debug.Log($"Alternar para slot {slotIndex + 1} (Tecla {slotIndex + 1})");
        // Exemplo: Inventario.Instance.Equipar(slotIndex);
    }

    private void PrimaryAction()
    {
        Debug.Log("Acao Primaria (Botao Esquerdo) - Atirar/Usar item principal");
        // Exemplo: currentItem?.PrimaryUse();
    }

    private void SecondaryAction()
    {
        Debug.Log("Acao Secundaria (Botao Direito) - Mirar/Acao alternativa");
        // Exemplo: currentItem?.SecondaryUse();
    }

    #endregion
}

// Interface para interacao (crie em um script separado ou aqui mesmo)
public interface IInteractable
{
    void Interact();
}