using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Metroidvania.InputSystem
{
    /// <summary>This class handle the unity input actions events</summary>
    public class InputReader : ScriptableSingleton<InputReader>, InputActions.IGameplayActions, InputActions.IMenusActions
    {
        public InputActions inputActions { get; protected set; }

        /// <summary>Triggered when the move button is performed or canceled. arg0 is the move horizontal direction</summary>
        public event Action<float> MoveEvent;

        // This actions is: [...]Event: when the input button down; [...]CanceledEvent: when the input button up;

        public event Action JumpEvent;

        public event Action AttackEvent;

        public event Action DashEvent;

        public event Action PauseEvent;

        public event Action MenuCloseEvent;

        protected void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new InputActions();

                // Use this method to implement callbacks On[Action](InputAction.CallbackContext)
                inputActions.Gameplay.SetCallbacks(this);
                inputActions.Menus.SetCallbacks(this);
            }
        }

        protected void OnDisable()
        {
            DisableAllInput();
        }

        /// <summary>Enable the gameplay input</summary>
        public void EnableGameplayInput()
        {
            inputActions.Menus.Disable();

            inputActions.Gameplay.Enable();
        }

        /// <summary>Enable menu input</summary>
        public void EnableMenuInput()
        {
            inputActions.Gameplay.Disable();

            inputActions.Menus.Enable();
        }

        /// <summary>Disable All inputs</summary>
        public void DisableAllInput()
        {
            inputActions.Menus.Disable();
            inputActions.Gameplay.Disable();
        }

        public float MoveValue()
        {
            return inputActions.Gameplay.Move.ReadValue<float>();
        }

        void InputActions.IGameplayActions.OnMove(InputAction.CallbackContext context)
        {
            OnMove(context);
        }

        protected virtual void OnMove(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    float value = context.ReadValue<float>();
                    int valueNormalized = value == 0 ? 0 : MathF.Sign(value);
                    MoveEvent?.Invoke(valueNormalized);
                    break;
                case InputActionPhase.Canceled:
                    MoveEvent?.Invoke(0);
                    break;
            }
        }

        void InputActions.IGameplayActions.OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallAttack(context);
        }

        protected virtual void CallAttack(InputAction.CallbackContext context)
            => AttackEvent?.Invoke();

        void InputActions.IGameplayActions.OnCrouch(InputAction.CallbackContext context)
        {
            CallCrouch(context);
        }

        protected virtual void CallCrouch(InputAction.CallbackContext context)
        {
        }

        void InputActions.IGameplayActions.OnDash(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallDash(context);
        }

        protected virtual void CallDash(InputAction.CallbackContext context)
            => DashEvent?.Invoke();

        void InputActions.IGameplayActions.OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallJump(context);
        }

        protected virtual void CallJump(InputAction.CallbackContext context) 
            => JumpEvent?.Invoke();
        

        void InputActions.IGameplayActions.OnPause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallPause(context);
        }

        protected virtual void CallPause(InputAction.CallbackContext context)
            => PauseEvent?.Invoke();


        void InputActions.IMenusActions.OnNavigate(InputAction.CallbackContext context)
        {
            CallNavigate(context);
        }

        protected virtual void CallNavigate(InputAction.CallbackContext context)
            => _ = context;

        void InputActions.IMenusActions.OnSubmit(InputAction.CallbackContext context)
        {
            CallSubmit(context);
        }

        protected virtual void CallSubmit(InputAction.CallbackContext context)
            => _ = context;

        void InputActions.IMenusActions.OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallCancel(context);
        }

        protected virtual void CallCancel(InputAction.CallbackContext context)
            => MenuCloseEvent?.Invoke();

        void InputActions.IMenusActions.OnClick(InputAction.CallbackContext context)
        {
            CallClick(context);
        }

        protected virtual void CallClick(InputAction.CallbackContext context)
            => _ = context;

        void InputActions.IMenusActions.OnPoint(InputAction.CallbackContext context)
        {
            CallPoint(context);
        }

        protected virtual void CallPoint(InputAction.CallbackContext context)
            => _ = context;

        void InputActions.IMenusActions.OnRightClick(InputAction.CallbackContext context)
        {
            CallRightClick(context);
        }

        protected virtual void CallRightClick(InputAction.CallbackContext context)
            => _ = context;

        void InputActions.IMenusActions.OnScrollWheel(InputAction.CallbackContext context)
        {
            CallScrollWheel(context);
        }

        protected virtual void CallScrollWheel(InputAction.CallbackContext context)
            => _ = context;

        void InputActions.IMenusActions.OnMiddleClick(InputAction.CallbackContext context)
        {
            CallMiddleClick(context);
        }

        protected virtual void CallMiddleClick(InputAction.CallbackContext context)
            => _ = context;
    }
}
