using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour
{
    #region Serialized Fields

    [Header("Componentes Requeridos")]
    [SerializeField] private RectTransform joystickContainer;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    [Header("Configuración de Comportamiento")]
    [Tooltip("Rango máximo de movimiento del handle")]
    [SerializeField] private float handleRange = 50f;

    [Tooltip("Si es true, el joystick aparece donde tocas. Si es false, permanece en posición fija")]
    [SerializeField] private bool isDynamic = true;

    [Tooltip("Tiempo de fade in/out en segundos")]
    [SerializeField] private float fadeDuration = 0.15f;

    [Header("Configuración Visual")]
    [Tooltip("Alpha cuando el joystick está visible")]
    [SerializeField][Range(0f, 1f)] private float visibleAlpha = 0.8f;

    [Tooltip("Alpha cuando el joystick está oculto")]
    [SerializeField][Range(0f, 1f)] private float hiddenAlpha = 0f;

    #endregion

    #region Private Fields

    private Vector2 inputDirection = Vector2.zero;
    private Canvas parentCanvas;
    private Camera canvasCamera;
    private CanvasGroup canvasGroup;
    private RectTransform canvasRectTransform;

    private bool isPressed = false;
    private int currentPointerId = -1;

    private Vector2 centerPosition;
    private float currentAlpha;
    private float fadeVelocity;

    #endregion

    #region Properties

    /// <summary>
    /// Componente horizontal del input (-1 a 1)
    /// </summary>
    public float Horizontal => inputDirection.x;

    /// <summary>
    /// Componente vertical del input (-1 a 1)
    /// </summary>
    public float Vertical => inputDirection.y;

    /// <summary>
    /// Dirección del input como Vector2 normalizado
    /// </summary>
    public Vector2 Direction => inputDirection;

    /// <summary>
    /// Magnitud del input (0 a 1)
    /// </summary>
    public float Magnitude => inputDirection.magnitude;

    /// <summary>
    /// Indica si el joystick está siendo presionado
    /// </summary>
    public bool IsPressed => isPressed;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        SetupCanvasGroup();
        CacheCanvasInformation();
    }

    private void Start()
    {
        HideJoystick(true);
    }

    private void Update()
    {
        if (canvasGroup != null && Mathf.Abs(canvasGroup.alpha - currentAlpha) > 0.01f)
        {
            canvasGroup.alpha = Mathf.SmoothDamp(
                canvasGroup.alpha,
                currentAlpha,
                ref fadeVelocity,
                fadeDuration
            );
        }

        HandleInput();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        if (joystickContainer == null)
        {
            Debug.LogError("[Joystick] JoystickContainer no asignado. Usando transform actual.");
            joystickContainer = GetComponent<RectTransform>();
        }

        if (background == null)
        {
            Debug.LogError("[Joystick] Background no asignado.");
        }

        if (handle == null)
        {
            Debug.LogError("[Joystick] Handle no asignado.");
        }
    }

    private void SetupCanvasGroup()
    {
        canvasGroup = joystickContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = joystickContainer.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = false;
    }

    private void CacheCanvasInformation()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("[Joystick] No se encontró Canvas padre.");
            return;
        }

        canvasRectTransform = parentCanvas.transform as RectTransform;

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvasCamera = parentCanvas.worldCamera;
        }
        else if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = null;
        }

        centerPosition = joystickContainer.anchoredPosition;
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase == TouchPhase.Began && !isPressed)
                {
                    OnInputDown(touch.position, touch.fingerId);
                }
                else if (touch.fingerId == currentPointerId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        OnInputDrag(touch.position);
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        OnInputUp();
                    }
                }
            }
        }

        else if (Input.GetMouseButtonDown(0) && !isPressed)
        {
            OnInputDown(Input.mousePosition, 0);
        }
        else if (Input.GetMouseButton(0) && isPressed)
        {
            OnInputDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isPressed)
        {
            OnInputUp();
        }
    }

    private void OnInputDown(Vector2 screenPosition, int pointerId)
    {
        if (!IsPositionValid(screenPosition)) return;

        isPressed = true;
        currentPointerId = pointerId;

        if (isDynamic)
        {
            PositionJoystick(screenPosition);
        }

        ShowJoystick();
        UpdateHandlePosition(screenPosition);
    }

    private void OnInputDrag(Vector2 screenPosition)
    {
        if (!isPressed) return;
        UpdateHandlePosition(screenPosition);
    }

    private void OnInputUp()
    {
        if (!isPressed) return;

        isPressed = false;
        currentPointerId = -1;
        inputDirection = Vector2.zero;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        HideJoystick(false);
    }

    #endregion

    #region Joystick Logic

    private void PositionJoystick(Vector2 screenPosition)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            canvasCamera,
            out localPoint))
        {
            joystickContainer.anchoredPosition = localPoint;
        }
    }

    private void UpdateHandlePosition(Vector2 screenPosition)
    {
        if (background == null || handle == null) return;

        Vector2 localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            screenPosition,
            canvasCamera,
            out localPosition))
        {
            Vector2 normalizedPosition = new Vector2(
                localPosition.x / (background.sizeDelta.x * 0.5f),
                localPosition.y / (background.sizeDelta.y * 0.5f)
            );

            inputDirection = Vector2.ClampMagnitude(normalizedPosition, 1f);

            handle.anchoredPosition = inputDirection * handleRange;
        }
    }

    private bool IsPositionValid(Vector2 screenPosition)
    {
        if (canvasRectTransform == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            canvasRectTransform,
            screenPosition,
            canvasCamera
        );
    }

    #endregion

    #region Visibility

    private void ShowJoystick()
    {
        currentAlpha = visibleAlpha;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void HideJoystick(bool immediate)
    {
        currentAlpha = hiddenAlpha;

        if (immediate && canvasGroup != null)
        {
            canvasGroup.alpha = hiddenAlpha;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Resetea el joystick a su estado inicial
    /// </summary>
    public void ResetJoystick()
    {
        inputDirection = Vector2.zero;
        isPressed = false;
        currentPointerId = -1;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        if (isDynamic)
        {
            joystickContainer.anchoredPosition = centerPosition;
        }

        HideJoystick(true);
    }

    /// <summary>
    /// Fuerza la visibilidad del joystick
    /// </summary>
    public void ForceShow(bool show)
    {
        if (show)
        {
            ShowJoystick();
            if (canvasGroup != null) canvasGroup.alpha = visibleAlpha;
        }
        else
        {
            HideJoystick(true);
        }
    }

    #endregion

    #region Debug

    private void OnValidate()
    {
        if (handleRange < 0f)
        {
            handleRange = 0f;
        }

        if (fadeDuration < 0f)
        {
            fadeDuration = 0f;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (background != null && Application.isPlaying && isPressed)
        {
            Gizmos.color = Color.green;
            Vector3 worldPos = background.position;
            Gizmos.DrawWireSphere(worldPos, handleRange * 0.01f);
        }
    }
#endif

    #endregion
}