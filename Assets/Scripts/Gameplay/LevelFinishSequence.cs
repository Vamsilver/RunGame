using System.Collections;
using RunGame.Player;
using RunGame.Procedural;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace RunGame.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public sealed class LevelFinishSequence : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float cameraMoveDuration = 1.25f;
        [SerializeField] private Vector3 celebrationCameraOffset = new(0f, 3.2f, -4.4f);
        [SerializeField] private GameObject completionBanner;
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField, Min(1f)] private float automaticContinueDelay = 30f;

        private bool completed;
        private ProceduralRunManager runManager;
        private Button nextLevelButton;
        private Text countdownText;

        public void Configure(ProceduralRunManager manager, GameObject banner, Button button, Text countdown)
        {
            runManager = manager;
            completionBanner = banner;
            nextLevelButton = button;
            countdownText = countdown;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (completed || !other.CompareTag("Player")) return;
            completed = true;
            StartCoroutine(CompleteLevel(other.gameObject));
        }

        private IEnumerator CompleteLevel(GameObject player)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            Rigidbody body = player.GetComponent<Rigidbody>();
            if (controller != null) controller.enabled = false;
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            if (completionBanner != null) completionBanner.SetActive(true);
            if (celebrationParticles != null) celebrationParticles.Play();
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveAllListeners();
                nextLevelButton.onClick.AddListener(ContinueToNextLevel);
                nextLevelButton.interactable = true;
            }
            if (runManager != null) StartCoroutine(AutomaticContinue());

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            Vector3 lookDirection = cameraTransform != null
                ? cameraTransform.position - player.transform.position
                : Vector3.back;
            lookDirection.y = 0f;
            Quaternion startRotation = player.transform.rotation;
            Quaternion faceCameraRotation = lookDirection.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(lookDirection.normalized, Vector3.up)
                : startRotation;

            CinemachineFollow follow = FindFirstObjectByType<CinemachineFollow>();
            Vector3 startOffset = follow != null ? follow.FollowOffset : Vector3.zero;
            float elapsed = 0f;
            while (elapsed < cameraMoveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / cameraMoveDuration);
                player.transform.rotation = Quaternion.Slerp(startRotation, faceCameraRotation, progress);
                if (follow != null)
                    follow.FollowOffset = Vector3.Lerp(startOffset, celebrationCameraOffset, progress);
                yield return null;
            }

            Vector3 dancePosition = player.transform.position;
            Vector3 danceScale = player.transform.localScale;
            float danceTime = 0f;
            while (true)
            {
                danceTime += Time.deltaTime;
                float bounce = Mathf.Abs(Mathf.Sin(danceTime * 4.8f));
                float sway = Mathf.Sin(danceTime * 3.2f);
                player.transform.position = dancePosition + Vector3.up * bounce * 0.38f;
                player.transform.rotation = faceCameraRotation * Quaternion.Euler(0f, sway * 24f, sway * 8f);
                player.transform.localScale = danceScale + new Vector3(bounce * 0.08f, -bounce * 0.08f, bounce * 0.08f);
                yield return null;
            }
        }

        private IEnumerator AutomaticContinue()
        {
            float remaining = automaticContinueDelay;
            while (remaining > 0f && completed)
            {
                if (countdownText != null)
                    countdownText.text = $"Next level starts in {Mathf.CeilToInt(remaining)}";
                remaining -= Time.deltaTime;
                yield return null;
            }
            ContinueToNextLevel();
        }

        private void ContinueToNextLevel()
        {
            if (!completed || runManager == null) return;
            completed = false;
            if (nextLevelButton != null) nextLevelButton.interactable = false;
            runManager.CompleteLevel();
        }
    }
}
