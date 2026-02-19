using UnityEngine;
using Metroidvania.InputSystem;
using Metroidvania;

/// <summary>
/// A Singleton for generating input events in tests. 
/// This is needed because the Unity Input System does not work well in tests.
/// </summary>
public class InputGenerator : Singleton<InputGenerator>
{
    public bool PrintDebugLogs = false;

    public void DoJump()
    {
        if (PrintDebugLogs)
        {
            Debug.Log("Generating Jump Input");
            InputReader.instance.JumpEvent += () => Debug.Log("Jump triggered");
        }
        InputReader.instance.SimulateJump();
    }

    public void DoAttack()
    {
        if (PrintDebugLogs)
        {
            Debug.Log("Generating Attack Input");
            InputReader.instance.AttackEvent += () => Debug.Log("Attack triggered");
        }
        InputReader.instance.SimulateAttack();
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
