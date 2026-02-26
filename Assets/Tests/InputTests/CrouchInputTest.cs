using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Metroidvania.InputSystem;
using UnityEngine.SceneManagement;

namespace Tests.Input
{
    /// <summary>
    /// Simple test to verify that attacking is working via the custom InputGenerator.
    /// </summary>
    public class CrouchingTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test CrouchingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        public IEnumerator PerformCrouchingTest()
        {
            // Arrange
            bool crouchTriggered = false;

            InputGenerator inputGenerator = InputGenerator.instance;
            inputGenerator.EnableDebugPrints();
            InputReader.instance.CrouchEvent += () => 
            {
                crouchTriggered = true;
            };
            
            yield return TestWait.ForSeconds(0.5f); // wait for initialization of stuff

            // Act
            inputGenerator.PressCrouch(0.05f);
            yield return TestWait.ForSeconds(1f); // wait for animation

            inputGenerator.PressCrouch(0.2f);
            yield return TestWait.ForSeconds(1f);

            inputGenerator.PressCrouch(1f);
            yield return TestWait.ForSeconds(1f);

            // Assert
            Assert.IsTrue(crouchTriggered, "Crouch event was not triggered" );
            yield return null;
        }
    }
}

