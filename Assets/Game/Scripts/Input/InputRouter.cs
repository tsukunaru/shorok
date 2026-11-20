using UnityEngine;
using UnityEngine.InputSystem;
namespace Shorok
{
    public sealed class InputRouter : System.IDisposable
    {
        readonly InputAction move = new InputAction("Move", InputActionType.Value);
        readonly InputAction jump = new InputAction("Jump", InputActionType.Button);
        readonly InputAction interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
        readonly InputAction action = new InputAction("PrimaryAction", InputActionType.Button, "<Keyboard>/f");
        readonly InputAction secondary = new InputAction("SecondaryAction", InputActionType.Button, "<Keyboard>/q");
        readonly InputAction pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        readonly InputAction reset = new InputAction("ResetPuzzle", InputActionType.Button, "<Keyboard>/r");
        public int Left, Right, JumpTouches;
        bool touchJump, touchInteract, touchAction, touchSecondary;
        public float Move => Mathf.Clamp(move.ReadValue<float>() + (Right > 0 ? 1 : 0) - (Left > 0 ? 1 : 0), -1, 1);
        public bool JumpHeld => jump.IsPressed() || JumpTouches > 0;
        public bool JumpPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool ActionPressed { get; private set; }
        public bool SecondaryPressed { get; private set; }
        public bool PausePressed => pause.WasPressedThisFrame();
        public bool ResetHeld => reset.IsPressed();
        public InputRouter()
        {
            move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/a").With("Positive", "<Keyboard>/d");
            move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
            jump.AddBinding("<Keyboard>/space"); jump.AddBinding("<Keyboard>/w"); jump.AddBinding("<Keyboard>/upArrow");
            move.Enable(); jump.Enable(); interact.Enable(); action.Enable(); secondary.Enable(); pause.Enable(); reset.Enable();
        }
        public void Sample()
        {
            JumpPressed = jump.WasPressedThisFrame() || touchJump;
            InteractPressed = interact.WasPressedThisFrame() || touchInteract;
            ActionPressed = action.WasPressedThisFrame() || touchAction;
            SecondaryPressed = secondary.WasPressedThisFrame() || touchSecondary;
            touchJump = touchInteract = touchAction = touchSecondary = false;
        }
        public void TouchPress(int kind)
        {
            if (kind == 0) Left++; if (kind == 1) Right++;
            if (kind == 2) { JumpTouches++; touchJump = true; }
            if (kind == 3) touchInteract = true; if (kind == 4) touchAction = true; if (kind == 5) touchSecondary = true;
        }
        public void TouchRelease(int kind)
        {
            if (kind == 0) Left = Mathf.Max(0, Left - 1); if (kind == 1) Right = Mathf.Max(0, Right - 1);
            if (kind == 2) JumpTouches = Mathf.Max(0, JumpTouches - 1);
        }
        public void ClearTouch() { Left = Right = JumpTouches = 0; touchJump = touchInteract = touchAction = touchSecondary = false; }
        public void Dispose() { move.Dispose(); jump.Dispose(); interact.Dispose(); action.Dispose(); secondary.Dispose(); pause.Dispose(); reset.Dispose(); }
    }
}
