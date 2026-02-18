using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using Metroidvania.InputSystem;
using Metroidvania;

public class TestInputReader : InputReader
{
    public void Initialize()
    {
        if (inputActions == null)
        {
            inputActions = new InputActions();
            inputActions.Gameplay.SetCallbacks(this);
            inputActions.Menus.SetCallbacks(this);
        }
    }

    public void DoJump()
    {
        CallJump(new InputAction.CallbackContext());
    }

    protected override void CallJump(InputAction.CallbackContext context)
    {
       base.CallJump(context);
       Debug.Log("Doing jump!");
    }
}
