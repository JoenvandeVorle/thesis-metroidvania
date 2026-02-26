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

    public void PressJump(float holdTime = DEFAULT_HOLDTIME)
    {
        if (doPrintDebug)
            Debug.Log("Generating Jump Input");
        InputReader.instance.SimulateJump(holdTime);
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
