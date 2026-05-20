using System.Collections;
using Aplib.Core;
using Aplib.Integrations.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty1 : BaseExperiment
    {
        private static readonly string[] _scenes =
        {
        "trivial", "trivialrv", "diver", "e1", "g1", "g1u1", "g2u1", "g2u1ceil", "g2u2",
        "offscreen-pitfall", "pitfall", "u1", "u2", "u3"
        };

        [UnityTest]
        [Timeout(30000)]
        public IEnumerator PerformDifficulty1([ValueSource(nameof(_scenes))] string scene)
        {
            Debug.Log($"Starting experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null; // let the scene finish loading before SetupExperiment runs
            yield return PerformExperiment();
        }
    }

}
