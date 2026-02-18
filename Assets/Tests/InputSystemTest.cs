using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using Metroidvania.InputSystem;

namespace Tests
{
    /// <summary>
    /// Using this test, we can verify that input system does not work as intended 
    /// in the test environment
    /// </summary>
    public class InputSystemDoesNotWorkTest : InputTestFixture
    {
        public override void Setup()
        {
            base.Setup();
            Debug.Log("Starting test InputSystemDoesNotWorkTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        // Written following https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Testing.html
        public IEnumerator JumpInput_TriggersJumpEvent()
        {
            yield return null; // wait one frame

            var keyboard = InputSystem.AddDevice<Keyboard>();
            
            InputReader.instance.Initialize();
            InputReader.instance.EnableGameplayInput();

            bool jumpTriggered = false;
            InputReader.instance.JumpEvent += () => 
            {
                Debug.Log("JumpEvent callback triggered!");
                jumpTriggered = true;
            };

            Press(keyboard.zKey);
            InputSystem.Update();
            yield return null;
            Release(keyboard.zKey);
            InputSystem.Update();
            yield return null;

            // If it worked, this would be true
            Assert.IsFalse(jumpTriggered, "Jump event was not triggered by Z key press");
        }
    }
}
