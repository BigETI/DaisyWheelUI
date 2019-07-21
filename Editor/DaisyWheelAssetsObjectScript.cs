using UnityEngine;

/// <summary>
/// Daisy wheel UI editor namespace
/// </summary>
namespace DaisyWheelUI.Editor
{
    /// <summary>
    /// Daisy wheel assets object script class
    /// </summary>
    internal class DaisyWheelAssetsObjectScript : ScriptableObject
    {
        /// <summary>
        /// Canvas asset
        /// </summary>
        [SerializeField]
        private GameObject canvasAsset;

        /// <summary>
        /// Event system asset
        /// </summary>
        [SerializeField]
        private GameObject eventSystemAsset;

        /// <summary>
        /// Daisy wheel asset
        /// </summary>
        [SerializeField]
        private GameObject daisyWheelAsset;

        /// <summary>
        /// Canvas asset
        /// </summary>
        public GameObject CanvasAsset => canvasAsset;

        /// <summary>
        /// Event system asset
        /// </summary>
        public GameObject EventSystemAsset => eventSystemAsset;

        /// <summary>
        /// Daisy wheel asset
        /// </summary>
        public GameObject DaisyWheelAsset => daisyWheelAsset;
    }
}
