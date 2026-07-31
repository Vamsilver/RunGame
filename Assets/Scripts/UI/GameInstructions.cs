using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RunGame.UI
{
    public sealed class GameInstructions : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
