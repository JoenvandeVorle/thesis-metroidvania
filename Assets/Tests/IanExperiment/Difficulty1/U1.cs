using System.Collections;
using Aplib.Core;
using Aplib.Integrations.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Experiments
{
    public class U1 : BaseExperiment
    {
        [SetUp]
        public void SetUp()
        {
            Debug.Log("Starting experiment u1");
            SceneManager.LoadScene("u1");
        }

        [UnityTest]
        public IEnumerator PerformUpledge1()
        {
            yield return PerformExperiment();
        }
    }
}
