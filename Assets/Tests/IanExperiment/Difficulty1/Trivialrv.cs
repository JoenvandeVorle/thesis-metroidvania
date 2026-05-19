using System.Collections;
using Aplib.Core;
using Aplib.Integrations.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class Trivialrv : BaseExperiment
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting experiment trivialrv");
            SceneManager.LoadScene("trivialrv");
        }

        [UnityTest]
        public IEnumerator PerformTrivialReverse()
        {
            yield return PerformExperiment();
        }
    }
}
