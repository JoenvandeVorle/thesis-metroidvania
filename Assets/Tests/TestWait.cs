using System.Collections;
using UnityEngine;

namespace Tests
{
    public static class TestWait
    {
        public static IEnumerator ForSeconds(float seconds)
        {
            float timer = 0f;
            while (timer < seconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}
