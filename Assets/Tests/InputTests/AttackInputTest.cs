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
    public class AttackingTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test AttackingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        public IEnumerator PerformAttackingTest()
        {
            // Arrange
            bool attackTriggered = false;

            InputGenerator inputGenerator = InputGenerator.instance;
            inputGenerator.PrintDebugLogs = true;
            InputReader.instance.AttackEvent += () => 
            {
                attackTriggered = true;
            };
            
            yield return TestWait.ForSeconds(0.5f); // wait for initialization of stuff

            // Act
            inputGenerator.PressAttack();
            yield return TestWait.ForSeconds(1f); // wait for animation

            inputGenerator.PressAttack();
            yield return TestWait.ForSeconds(1f);

            inputGenerator.PressAttack();
            yield return TestWait.ForSeconds(1f);

            // Assert
            Assert.IsTrue(attackTriggered, "Attack event was not triggered" );
            yield return null;
        }
    }
}

