using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Metroidvania.InputSystem;

namespace Tests.Input
{
    public class DashingTest
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting test DashingTest");
            SceneManager.LoadScene("SimpleTestScene");
        }

        [UnityTest]
        public IEnumerator PerformDashingTest()
        {
            // Arrange
            bool dashTriggered = false;

            InputGenerator inputGenerator = InputGenerator.instance;
            inputGenerator.EnableDebugPrints();
            InputReader.instance.DashEvent += () => dashTriggered = true;

            // Wait for one second to ensure stuff is initialized
            yield return TestWait.ForSeconds(1f);

            // Act
            inputGenerator.PressDash();
            yield return TestWait.ForSeconds(2f);

            inputGenerator.PressDash();
            yield return TestWait.ForSeconds(2f);

            inputGenerator.PressDash();
            yield return TestWait.ForSeconds(2f);

            // Assert
            Assert.IsTrue(dashTriggered, "Dash was not triggered");
            yield return null;
        }
    }
}

