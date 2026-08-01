using UnityEngine;

namespace RunGame.Procedural
{
    public sealed class RunModule : MonoBehaviour
    {
        [SerializeField, Min(5f)] private float length = 18f;
        [SerializeField] private string moduleName = "Module";

        public float Length => length;
        public string ModuleName => moduleName;
    }
}
