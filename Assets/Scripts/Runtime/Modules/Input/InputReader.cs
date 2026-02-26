using System;
using System.Collections.Generic;
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

        public event Action CrouchEvent;

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

        #region Gameplay InputActions Button Processing Callbacks

        void InputActions.IGameplayActions.OnMove(InputAction.CallbackContext context) => CallMove(context);

        void InputActions.IGameplayActions.OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallAttack(context);
        }

        void InputActions.IGameplayActions.OnCrouch(InputAction.CallbackContext context) => CallCrouch(context);

        void InputActions.IGameplayActions.OnDash(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallDash(context);
        }

        void InputActions.IGameplayActions.OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                CallJump(context);
            }
        }

        void InputActions.IGameplayActions.OnPause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallPause(context);
        } 

        #endregion

        #region Menu InputActions Button Processing Callbacks

        void InputActions.IMenusActions.OnNavigate(InputAction.CallbackContext context) => CallNavigate(context);

        void InputActions.IMenusActions.OnSubmit(InputAction.CallbackContext context) => CallSubmit(context);

        void InputActions.IMenusActions.OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                CallCancel(context);
        }

        void InputActions.IMenusActions.OnClick(InputAction.CallbackContext context) => CallClick(context);

        void InputActions.IMenusActions.OnPoint(InputAction.CallbackContext context) => CallPoint(context);

        void InputActions.IMenusActions.OnRightClick(InputAction.CallbackContext context) => CallRightClick(context);

        void InputActions.IMenusActions.OnScrollWheel(InputAction.CallbackContext context) => CallScrollWheel(context);

        void InputActions.IMenusActions.OnMiddleClick(InputAction.CallbackContext context) => CallMiddleClick(context);

        #endregion

        #region InputAction Event Invoking Callbacks
        // We separate these from the actual InputAction callbacks so that these can be called in Tests and possible elsewhere
        private void CallMove(InputAction.CallbackContext context)
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

        private void CallAttack(InputAction.CallbackContext context) => AttackEvent?.Invoke();

        private void CallCrouch(InputAction.CallbackContext context) => CrouchEvent?.Invoke();

        private void CallDash(InputAction.CallbackContext context) => DashEvent?.Invoke();

        private void CallJump(InputAction.CallbackContext context) => JumpEvent?.Invoke();

        private void CallPause(InputAction.CallbackContext context) => PauseEvent?.Invoke();

        private void CallNavigate(InputAction.CallbackContext context) => _ = context;
        private void CallSubmit(InputAction.CallbackContext context) => _ = context;
        private void CallCancel(InputAction.CallbackContext context) => MenuCloseEvent?.Invoke();
        private void CallClick(InputAction.CallbackContext context) => _ = context;
        private void CallPoint(InputAction.CallbackContext context) => _ = context;
        private void CallRightClick(InputAction.CallbackContext context) => _ = context;
        private void CallScrollWheel(InputAction.CallbackContext context) => _ = context;
        private void CallMiddleClick(InputAction.CallbackContext context) => _ = context;

        #endregion

#if UNITY_EDITOR

        public TestableInputAction GetTestableInputAction(InputAction inputAction)
        {
            if (!testableInputActions.ContainsKey(inputAction))
            {
                testableInputActions[inputAction] = new TestableInputAction(inputAction);
            }
            return testableInputActions[inputAction];
        }

        public TestableInputAction TestableJumpAction => GetTestableInputAction(inputActions.Gameplay.Jump);
        public TestableInputAction TestableAttackAction => GetTestableInputAction(inputActions.Gameplay.Attack);
        public TestableInputAction TestableCrouchAction => GetTestableInputAction(inputActions.Gameplay.Crouch);
        public TestableInputAction TestableDashAction => GetTestableInputAction(inputActions.Gameplay.Dash);
        public TestableInputAction TestableMoveAction => GetTestableInputAction(inputActions.Gameplay.Move);

        private Dictionary<InputAction, TestableInputAction> testableInputActions = new ();

        public void Update()
        {
            foreach (var testableInputAction in testableInputActions.Values)
            {
                testableInputAction.Update();
            }
        }

        /// <summary>
        /// Holds until released by Release()
        /// </summary>
        public void SimulateLeftHolding()
        {
            TestableMoveAction.SimulatePress();
            MoveEvent?.Invoke(-1);
        }

        /// <summary>
        /// Holds until released by Release()
        /// </summary>
        public void SimulateRightHolding()
        {
            TestableMoveAction.SimulatePress();
            MoveEvent?.Invoke(1);
        }

        public void SimulateMovementRelease()
        {
            TestableMoveAction.SimulateRelease();
            MoveEvent?.Invoke(0);
        }

        public void SimulateJump(float holdDuration)
        {
            TestableJumpAction.HoldFor(holdDuration);
            CallJump(default);
        }

        public void SimulateAttack()
        {
            TestableAttackAction.HoldFor(0.1f);
            CallAttack(default);
        }

        public void SimulateCrouch(float holdDuration)
        {
            TestableCrouchAction.HoldFor(holdDuration);
            CallCrouch(default);
        }

        public void SimulateDash()
        {
            TestableDashAction.HoldFor(0.1f);
            CallDash(default);
        }

#endif
    }
}
