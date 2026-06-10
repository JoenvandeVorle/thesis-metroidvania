using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Difficulty4Aplib : NaiveAplibExperiment
    {
        private static readonly string[] _scenes =
        {
            "gauntlet", "enemies", "careful", "jumpman"
        };

        private static string _resultsFile = "difficulty4.jsonl";

        [UnityTest]
        [Timeout(90000)]
        public IEnumerator PerformDifficulty4([ValueSource(nameof(_scenes))] string scene, [ValueSource(nameof(_runs))] int run)
        {
            Debug.Log($"Starting Aplib Experiment {scene}");
            SceneManager.LoadScene(scene);
            yield return null;
            yield return RunExperiment(_resultsFile, run);
        }
    }

}
