﻿using UnityEngine;
using UnityEngine.Timeline;

namespace Nova
{
    // TODO Current implementation of this class is inefficient, some modifications are needed.
    /// <summary>
    /// The class that loads assets at runtime
    /// Assets can be Sprite, BGM or Sound Effect
    /// </summary>
    /// <remarks>
    /// All assets should be stored in a Resources folder or a subfolder of a Resources folder
    /// </remarks>
    public class AssetsLoader
    {
        /// <summary>
        /// Get sprite by path
        /// </summary>
        /// <param name="path">
        /// The path of the sprite, the convention is the same as that of <see cref="Resources.Load(string)"/>
        /// </param>
        /// <returns>The specified sprite or null if not found</returns>
        public static Sprite GetSprite(string path)
        {
            var ret = Resources.Load<Sprite>(path);
            if (ret == null)
            {
                Debug.LogError(string.Format("Sprite {0} not found", path));
            }

            return ret;
        }

        /// <summary>
        /// Get AudioClip by path
        /// </summary>
        /// <param name="path">
        /// The path of the AudioClip, the convention is the same as that of <see cref="Resources.Load(string)"/>
        /// </param>
        /// <returns>The specified AudioClip or null if not found</returns>
        public static AudioClip GetAudioClip(string path)
        {
            var ret = Resources.Load<AudioClip>(path);
            if (ret == null)
            {
                Debug.LogError(string.Format("AudioClip {0} not found", path));
            }

            return ret;
        }

        public static TimelineAsset GetTimelineAsset(string path)
        {
            var ret = Resources.Load<TimelineAsset>(path);
            if (ret == null)
            {
                Debug.LogError(string.Format("TimelineAsset {0} not found", path));
            }

            return ret;
        }
    }
}