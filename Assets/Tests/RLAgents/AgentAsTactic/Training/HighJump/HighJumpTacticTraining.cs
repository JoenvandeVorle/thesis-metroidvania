using UnityEngine;

namespace Tests.AplibTests
{
    public class HighJumpTacticTraining : HighJumpTacticBase
    {
        [SerializeField] private GameObject starts; // parent object containing possible start positions
        [SerializeField] private GameObject platformsRight;
        [SerializeField] private GameObject platformsLeft;

        private GameObject currentPlatform;
        private bool startLeft = true;

        protected override void Start()
        {
            base.Start();

            if (starts == null || platformsRight == null || platformsLeft == null)
            {
                Debug.LogError("Please assign start and platform parent objects in the inspector.");
                enabled = false;
                return;
            }

            foreach (Transform child in starts.transform)
                child.gameObject.SetActive(false);

            foreach (Transform platform in platformsRight.transform)
                platform.gameObject.SetActive(false);

            foreach (Transform platform in platformsLeft.transform)
                platform.gameObject.SetActive(false);
        }

        protected override void DoOnEpisodeBegin()
        {
            if (start != null)
                start.SetActive(false);

            start = starts.transform.GetChild(Random.Range(0, starts.transform.childCount)).gameObject;
            start.SetActive(true);
            this.transform.position = start.transform.position;

            if (currentPlatform != null)
                currentPlatform.SetActive(false);

            GameObject platformSet = startLeft ? platformsRight : platformsLeft;
            startLeft = !startLeft;

            currentPlatform = platformSet.transform.GetChild(Random.Range(0, platformSet.transform.childCount)).gameObject;
            currentPlatform.SetActive(true);
            GameObject target = currentPlatform.transform.GetChild(0).gameObject;
            goal = target.transform.position;

            // reset position if fallen
            if (transform.position.y <= fallHeight)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

    }
}
