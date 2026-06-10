using UnityEngine;
using Metroidvania.InputSystem;
using Metroidvania;

/// <summary>
/// A Singleton for generating input events in tests.
/// This is needed because the Unity Input System does not work well in tests.
/// </summary>
public class InputGenerator : Singleton<InputGenerator>
{
    public const float DEFAULT_HOLDTIME = 0.3f;

    private bool doPrintDebug = false;

    /// <summary>
    /// Disable regular gameplay keyboard input, so that only input from this class is processed.
    /// </summary>
    /// <param name="disable"></param>
    public void SetKeyboardDisable(bool disable)
    {
        InputReader.instance.InputDisabled = disable;
    }

    public void EnableDebugPrints()
    {
        doPrintDebug = true;
        InputReader.instance.MoveEvent += dir => Debug.Log($"Move triggered in direction {dir}");
        InputReader.instance.JumpEvent += () => Debug.Log("Jump triggered");
        InputReader.instance.CrouchEvent += () => Debug.Log("Crouch triggered");
        InputReader.instance.AttackEvent += () => Debug.Log("Attack triggered");
        InputReader.instance.DashEvent += () => Debug.Log("Dash triggered");
    }

    public void DisableDebugPrints()
    {
        doPrintDebug = false;
        InputReader.instance.MoveEvent -= dir => Debug.Log($"Move triggered in direction {dir}");
        InputReader.instance.JumpEvent -= () => Debug.Log("Jump triggered");
        InputReader.instance.CrouchEvent -= () => Debug.Log("Crouch triggered");
        InputReader.instance.AttackEvent -= () => Debug.Log("Attack triggered");
        InputReader.instance.DashEvent -= () => Debug.Log("Dash triggered");
    }

    /// <summary>
    /// Moves left or right depending on the direction.
    /// </summary>
    /// <param name="direction"></param>
    public void MoveTowards(Vector2 direction)
    {
        bool toLeft = direction.x < 0;
        if (toLeft)
            InputReader.instance.SimulateLeftHolding();
        else
            InputReader.instance.SimulateRightHolding();
    }

    public void HoldLeft()
    {
        if (doPrintDebug)
            Debug.Log("Pressing left arrow");
        InputReader.instance.SimulateLeftHolding();
    }

    public void HoldRight()
    {
        if (doPrintDebug)
            Debug.Log("Pressing right arrow");
        InputReader.instance.SimulateRightHolding();
    }

    public void ReleaseMovement()
    {
        if (doPrintDebug)
            Debug.Log("Releasing arrows");
        InputReader.instance.SimulateMovementRelease();
    }

    public void HoldJump()
    {
        if (doPrintDebug)
            Debug.Log("Holding Jump");
        InputReader.instance.SimulateJumpHold();
    }

    public void ReleaseJump()
    {
        if (doPrintDebug)
            Debug.Log("Releasing Jump");
        InputReader.instance.SimulateJumpRelease();
    }

    public void PressJumpFor(float holdTime = DEFAULT_HOLDTIME)
    {
        if (doPrintDebug)
            Debug.Log("Generating Jump Input");
        InputReader.instance.SimulateJumpHoldFor(holdTime);
    }

    public void PressAttack()
    {
        if (doPrintDebug)
            Debug.Log("Generating Attack Input");
        InputReader.instance.SimulateAttack();
    }
    public void PressCrouch(float holdTime = DEFAULT_HOLDTIME)
    {
        if (doPrintDebug)
            Debug.Log("Generating Crouch Input");
        InputReader.instance.SimulateCrouch(holdTime);
    }

    public void PressDash()
    {
        if (doPrintDebug)
            Debug.Log("Generating Dash Input");
        InputReader.instance.SimulateDash();
    }

    // Implementation using Reflection

    // public void DoJump()
    // {
    //     InputReader.instance.JumpEvent += () => Debug.Log("Jump triggered with REFLECTION!");
    //     InvokeInputReaderCallback("CallJump");
    // }
    // private static void InvokeInputReaderCallback(string methodName)
    // {
    //     MethodInfo callback = typeof(InputReader).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

    //     if (callback == null)
    //     {
    //         Debug.LogError($"Could not find InputReader method '{methodName}' via reflection.");
    //         return;
    //     }

    //     callback.Invoke(InputReader.instance, new object[] { default(InputAction.CallbackContext) });
    // }
}
