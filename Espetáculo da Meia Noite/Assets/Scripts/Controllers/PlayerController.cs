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

    [Header("Camera (Mouse Look)")]
    [SerializeField] private Transform cameraTransform; // A camera que é filha do player
    [SerializeField] private float mouseSensitivity = 52f;
    [SerializeField] private float minVerticalAngle = -70f; // Limite inferior (olhar para baixo)
    [SerializeField] private float maxVerticalAngle = 20f;  // Limite superior (olhar para cima)
    [SerializeField] private bool enableSmoothing = true;   // Suavizacao opcional
    [SerializeField] private float smoothTime = 0.005f;      // Tempo de suavizacao

    [Header("Física")]
    [SerializeField] private float maxFallSpeed = -20f;
    [SerializeField] private InteractionDetector interactionDetector;

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

    private float currentVerticalAngle = 0f;
    private float currentHorizontalAngle = 0f;
    private float verticalVelocityRef;

    private void Awake()
    {
        // Instancia o Input Actions gerado
        inputActions = new InputSystem_Actions();
        controller = GetComponent<CharacterController>();

        // Trava e esconde o cursor para jogos 3D
        Cursor.lockState = CursorLockMode.Locked;

        // Inicializa os angulos com a rotacao atual da camera e do player
        if (cameraTransform != null)
        {
            currentVerticalAngle = cameraTransform.localEulerAngles.x;
            // Ajusta para o intervalo [-180, 180] para facilitar o clamp
            if (currentVerticalAngle > 180) currentVerticalAngle -= 360;
        }
        currentHorizontalAngle = transform.eulerAngles.y;
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
        ApplyMouseLook();

        // 3. Calcula e aplica o movimento
        ApplyMovement();
    }

    #region MOVIMENTO E FISICA

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
    #endregion

    #region CAMERA (MOUSE)

    private void ApplyMouseLook()
    {
        if (cameraTransform == null) return;

        // Calcula a rotacao horizontal (personagem) com base no input do mouse
        float horizontalDelta = lookInput.x * mouseSensitivity * Time.deltaTime;
        currentHorizontalAngle += horizontalDelta;

        // Calcula a rotacao vertical (camera) com base no input do mouse
        float verticalDelta = -lookInput.y * mouseSensitivity * Time.deltaTime;
        currentVerticalAngle += verticalDelta;

        // --- CRITÉRIO 2: Limite de angulo vertical ---
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

        // --- CRITÉRIO 3: Suavizacao Opcional (SmoothDamp) ---
        if (enableSmoothing){
            // Aplica a rotacao horizontal com suavizacao
            float targetHorizontalAngle = currentHorizontalAngle;
            float smoothHorizontal = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, 
                targetHorizontalAngle, 
                ref verticalVelocityRef, // Reaproveitamos a variavel, mas seria ideal ter uma dedicada
                smoothTime
            );
            transform.rotation = Quaternion.Euler(0f, smoothHorizontal, 0f);

            // Aplica a rotacao vertical com suavizacao
            float targetVerticalAngle = currentVerticalAngle;
            float smoothVertical = Mathf.SmoothDampAngle(
                cameraTransform.localEulerAngles.x,
                targetVerticalAngle,
                ref verticalVelocityRef,
                smoothTime
            );
            cameraTransform.localRotation = Quaternion.Euler(smoothVertical, 0f, 0f);
        } else {
            // Sem suavizacao: aplicacao direta e instantanea
            transform.rotation = Quaternion.Euler(0f, currentHorizontalAngle, 0f);
            cameraTransform.localRotation = Quaternion.Euler(currentVerticalAngle, 0f, 0f);
        }
    }

    #endregion

    #region ACOES (EVENTOS)

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        Debug.Log($"Agachamento: {(isCrouching ? "Ativado" : "Desativado")}");
    }

    private void Interact()
    {
        inputActions.Player.Interact.performed += ctx => interactionDetector.OnInteract(ctx);
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