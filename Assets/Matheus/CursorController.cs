using UnityEngine;

public enum CursorState
{
    Neutral = 0,
    Clicking = 1,
    Clickable = 2,
    Paused = 3
}

public class CursorController : MonoBehaviour
{

    public static CursorController Instance { get; private set; }

    [Header("Cursor Config")]
    private Vector2 clickPosition = Vector2.zero;

    [Header("Basic Configuration")]
    [SerializeField] private LayerMask clickableLayers;

    private GameObject foundObjectReference;
    private Camera mainCam;
    private Vector3 offset;
    private CursorState state;
    private bool leftClickPressed;
    private bool rightClickPressed;

    #region Unity Functions
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        //clickPosition = cursors.defaultCursorCenter;
        state = CursorState.Neutral;
        Cursor.visible = true;
        mainCam = Camera.main;
    }

    void Update()
    {
        CheckInputs();
        UpdateState();
    }
    #endregion

    #region State Machine
    void UpdateState()
    {
        switch (state)
        {
            case CursorState.Neutral:
                UpdateNeutralState();
                break;
            case CursorState.Clickable:
                UpdateClickableState();
                break;
            case CursorState.Clicking:
                UpdateClickingState();
                break;
            case CursorState.Paused:
                break;
        }
    }

    void EnterState(CursorState newState)
    {
        switch (newState)
        {
            case CursorState.Neutral:
                Cursor.visible = true;
                break;
            case CursorState.Clickable:
                
                break;
            case CursorState.Clicking:
                Cursor.visible = false;
                break;
            case CursorState.Paused:
                
                break;
        }
    }

    void ExitState(CursorState newState)
    {
        switch (newState)
        {
            case CursorState.Neutral:
                break;
            case CursorState.Clickable:
                break;
            case CursorState.Clicking:
                ExitClickingState();
                break;
            case CursorState.Paused:
                break;
        }
    }

    void ChangeState(CursorState newState)
    {
        ExitState(state);
        state = newState;
        EnterState(state);
    }
    #endregion

    #region Neutral State
    void UpdateNeutralState()
    {
        FindObjectAtPointer();
    }
    void FindObjectAtPointer()
    {
        RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 999999, clickableLayers);
        if (hit)
        {
            foundObjectReference = hit.transform.gameObject;
            ChangeState(CursorState.Clickable);
        }
        else
        {
            foundObjectReference = null;
        }
    }
    #endregion
    #region Clicking State

    void UpdateClickingState()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos = new Vector3(mousePos.x, mousePos.y, 0.0f);
        Vector3 objPos = new Vector3(foundObjectReference.transform.position.x, foundObjectReference.transform.position.y, 0.0f);
        offset = mousePos - objPos;
        foundObjectReference.transform.position = mousePos;// + offset;
        if(!leftClickPressed)
        {
            Debug.Log("Passei aqui");
            ChangeState(CursorState.Neutral);
        }
    }
    
    void ExitClickingState()
    {
        
    }
    #endregion
    #region Clickable State

    void FindCursorContextOnHover()
    {
        
    }

    void UpdateClickableState()
    {
        CheckObjectHover();
        if(leftClickPressed)
        {
            if(foundObjectReference != null)
            {
                ChangeState(CursorState.Clicking);
            }
            else
                leftClickPressed = false;
        }
    }

    void CheckObjectHover()
    {
        RaycastHit2D hit = Physics2D.Raycast(mainCam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 999999, clickableLayers);
        if (hit)
        {
            return;
        }
        else
        {
            foundObjectReference = null;
            ChangeState(CursorState.Neutral);
        }
    }
    #endregion
    #region Paused State
    #endregion

    #region Input Handler
    void CheckInputs()
    {
        if (Input.GetButtonDown("LeftClick"))
        {
            leftClickPressed = true;
        }
        else if (Input.GetButtonUp("LeftClick"))
        {
            leftClickPressed = false;
        }

        if (Input.GetButtonDown("RightClick"))
        {
            rightClickPressed = true;
        }
        else if (Input.GetButtonUp("RightClick"))
        {
            rightClickPressed = false;
        }
    }
    #endregion

}
