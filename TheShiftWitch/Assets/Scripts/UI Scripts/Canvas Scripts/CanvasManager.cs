using UnityEngine;
using UnityEngine.Events;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] protected GameObject canvas;
    [SerializeField] private GameObject firstOnMenu;
    [SerializeField] protected UnityEvent OnCancelCanvas;
    public static event UnityAction<GameObject> sendObj;
    // Start is called before the first frame update
    public void CanvasWasActivated() {

    }
    public virtual void CanvasControl(bool val) {
        if (canvas != null)
            canvas.SetActive(val);
        if (val == true) {

            AssignButtons();
        }
        else {
            UnAssignButtons();
        }
        print("canvas was opened");
    }
    private void KillMenus(bool val) {
        //if (GameManager.GetManager().CurrentState == GameManager.GameState.PlayMode) {
        //    CancelCanvas();
        //}
    }
    public virtual void CancelCanvas() {
        //CanvasControl(false);
        OnCancelCanvas?.Invoke();
        print("canvas was canceled");
    }
    public virtual void AssignCircle() {
        //close this Canvas and go back to main pause menu
        CancelCanvas();
        print("canvases was canceled");
    }
    public virtual void AssignButtons() {
        if (sendObj != null && firstOnMenu != null)
            sendObj?.Invoke(firstOnMenu);

    }
    public virtual void UnAssignButtons() {

    }
    public void GetFirstOnMenu() {
        if (sendObj != null && firstOnMenu != null)
            sendObj.Invoke(firstOnMenu);
    }
}
