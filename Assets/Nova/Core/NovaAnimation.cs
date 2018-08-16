using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nova
{
    public class NovaAnimation : MonoBehaviour
    {
        private class AnimationEntry
        {
            public float duration;

            public float timeElapsed;

            public float targetValue;

            public float startValue;

            public void SetTransition(float from, float to, float duration)
            {
                targetValue = to;
                timeElapsed = 0;
                startValue = from;
                this.duration = duration;
            }

            public void Stop()
            {
                // Coroutine will check these values then quit
                timeElapsed = duration + 1.0f;
            }
        }

        private IEnumerator AnimationCoroutine(AnimationEntry entry, INovaAnimationProperty property)
        {
            while (true)
            {
                if (entry.timeElapsed >= entry.duration)
                {
                    property.value = entry.targetValue;
                    _animationEntries.Remove(property.ID);
                    Debug.Log("Animation stops");
                    yield break;
                }

                var t = Mathf.Clamp01(entry.timeElapsed / entry.duration);
                property.value = Mathf.Lerp(entry.startValue, entry.targetValue, t);
                entry.timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        private readonly Dictionary<string, AnimationEntry>
            _animationEntries = new Dictionary<string, AnimationEntry>();

        /// <summary>
        /// Register a transition
        /// </summary>
        /// <remarks>
        /// Nova animation is target based. If a new transition is registered before the last transition ends, the target
        /// of the property will switch to the new value immediately. No matter what value the current property is, it
        /// will always take the specified time duration to reach the target state. 
        /// </remarks>
        /// <param name="property">The property to be animated</param>
        /// <param name="target">The target of the property value</param>
        /// <param name="duration">How long should it take for the property to reach the target</param>
        public void RegisterTransition(INovaAnimationProperty property, float target, float duration)
        {
            AnimationEntry entry;
            var propertyId = property.ID;
            if (_animationEntries.TryGetValue(propertyId, out entry))
            {
                entry.SetTransition(property.value, target, duration);
                return;
            }

            entry = new AnimationEntry();
            _animationEntries.Add(propertyId, entry);
            entry.SetTransition(property.value, target, duration);
            StartCoroutine(AnimationCoroutine(entry, property));
        }

        /// <summary>
        /// Stop the animation by property ID
        /// </summary>
        /// <remarks>
        /// The value of the property will be set to the target value immediately. All unfinished transitions will be
        /// discarded.
        /// Nothing will happens if the property ID haven't been registed or the last animation of the property
        /// has stopped.
        /// </remarks>
        /// <param name="propertyId">the id of the property animation to stop</param>
        public void Stop(string propertyId)
        {
            AnimationEntry entry;
            if (_animationEntries.TryGetValue(propertyId, out entry))
            {
                entry.Stop();
            }
        }

        /// <summary>
        /// Stop the animation of the specified property
        /// </summary>
        /// <remarks>
        /// The value of the property will be set to the target value immediately. All unfinished transitions will be
        /// discarded.
        /// Nothing will happens if the property ID haven't been registed or the last animation of the property
        /// has stopped.
        /// </remarks>
        /// <param name="property">the property to stop</param>
        public void Stop(INovaAnimationProperty property)
        {
            Stop(property.ID);
        }

        /// <summary>
        /// Stop all animations
        /// </summary>
        public void Stop()
        {
            foreach (var entry in _animationEntries)
            {
                entry.Value.Stop();
            }
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}