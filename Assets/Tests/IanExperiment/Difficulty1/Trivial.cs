using System.Collections;
using Aplib.Core;
using Aplib.Integrations.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Trivial : BaseExperiment
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting experiment trivial");
            SceneManager.LoadScene("trivial");
        }

        [UnityTest]
        public IEnumerator PerformTrivialTest()
        {
            SetupExperiment();
            while (pathfinder.GetCurrentPath() == null)
            {
                Debug.Log("Waiting for path to be calculated...");
                yield return TestWait.ForSeconds(1f);
            }
            nextPathNode = pathfinder.GetNextPathNode();

            AplibRunner runner = new AplibRunner(agent);

            // Act
            Assert.GreaterOrEqual(player.transform.position.y, -10, "Player should start above y = -10");
            yield return runner.Test();

            // Assert
            Assert.AreEqual(CompletionStatus.Success, agent.Status);
        }
    }
}
