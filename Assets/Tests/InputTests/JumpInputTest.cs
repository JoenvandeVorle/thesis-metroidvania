using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Metroidvania.InputSystem;
using UnityEngine.InputSystem;

namespace Tests.Input
{
    /// <summary>
    /// Simple test to verify that jumping is working via the custom InputGenerator.
    /// </summary>
    public class JumpingTest
    {
        [UnityTest]
        public IEnumerator PerformJumpingTest()
        {
            // Arrange
            bool jumpTriggered = false;

            InputGenerator inputGenerator = InputGenerator.instance;
            inputGenerator.PrintDebugLogs = true;
            InputReader.instance.JumpEvent += () => 
            {
                jumpTriggered = true;
            };

            // Act
            while (!jumpTriggered)
            {
                inputGenerator.DoJump();
                yield return null; // wait one frame
            }

            // Wait two seconds for the jump animation to play
            float timer = 0f;
            while (timer < 2f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Assert
            Assert.IsTrue(jumpTriggered, "Jump event was not triggered" );
            yield return null;
        }

        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test JumpingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }
    }
}
