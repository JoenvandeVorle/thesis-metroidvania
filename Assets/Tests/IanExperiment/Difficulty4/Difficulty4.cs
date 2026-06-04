using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty4 : HybridExperiment
    {
        private static readonly string[] _scenes =
        {
            "gauntlet", "enemies", "careful", "jumpman"
        };

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator PerformDifficulty4([ValueSource(nameof(_scenes))] string scene, [ValueSource(nameof(_runs))] int run)
        {
            Debug.Log($"Starting experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null;
            yield return RunExperiment(run);
        }
    }

}
