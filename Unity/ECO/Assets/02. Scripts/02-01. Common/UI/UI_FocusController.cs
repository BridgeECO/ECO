using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

public class UI_FocusController : MonoBehaviour
{
    [Foldout("Hierarchy")]
    [SerializeField]
    private GameObject _firstSelectedButton;
    private GameObject _lastSelectedButton;

    private void OnEnable()
    {
        SubscribeNextFrameAsync().Forget();
    }

    private void OnDisable()
    {
        _lastSelectedButton = null;


        SetSelectedGameObject(null);


        InputHandler.OnNavigateEvent -= ChangeNavigationMode;
        InputHandler.OnPointEvent -= ChangePointerMode;
    }

    private void Update()
    {
        CheckNavigationInput();
        CheckPointerInput();
    }

    private async UniTaskVoid SubscribeNextFrameAsync()
    {
        await UniTask.Yield();

        _lastSelectedButton = null;
        InputHandler.OnNavigateEvent += ChangeNavigationMode;
        InputHandler.OnPointEvent += ChangePointerMode;

        if (_firstSelectedButton != null && _firstSelectedButton.activeInHierarchy)
        {
            SetSelectedGameObject(_firstSelectedButton);
        }
    }

    private void CheckNavigationInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            InputHandler.TriggerNavigateEvent();
        }
    }

    private void CheckPointerInput()
    {
        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
        {
            InputHandler.TriggerPointEvent();
        }
    }

    public void ChangeSelected(GameObject target)
    {
        SetSelectedGameObject(target);
    }

    private void ChangeNavigationMode()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }

        GameObject targetButton = (_lastSelectedButton != null && _lastSelectedButton.activeInHierarchy)
            ? _lastSelectedButton

            : _firstSelectedButton;

        SetSelectedGameObject(targetButton);
        _lastSelectedButton = targetButton;
    }

    private void ChangePointerMode()
    {
        if (EventSystem.current == null)
        {
            return;
        }
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected == null)
        {
            return;
        }

        _lastSelectedButton = currentSelected;
        if (currentSelected.GetComponent<TMP_InputField>() != null)
        {
            return;
        }
        SetSelectedGameObject(null);
    }

    private void SetSelectedGameObject(GameObject target)
    {
        if (EventSystem.current == null)
        {
            return;
        }
        EventSystem.current.SetSelectedGameObject(target);
    }
}
