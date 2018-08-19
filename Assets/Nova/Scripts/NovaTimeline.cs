﻿using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Nova
{
    // TODO haven't exported to lua script yet
    [RequireComponent(typeof(NovaAnimation), typeof(PlayableDirector))]
    public class NovaTimeline : MonoBehaviour
    {
        private NovaAnimation _animationTimer;
        private PlayableDirector _playableDirector;
        private TimeAnimationProperty _property;

        private void Awake()
        {
            _animationTimer = GetComponent<NovaAnimation>();
            _playableDirector = GetComponent<PlayableDirector>();
            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
            _playableDirector.playOnAwake = false;
            _property = new TimeAnimationProperty(_playableDirector);
        }

        private class TimeAnimationProperty : INovaAnimationProperty
        {
            private readonly PlayableDirector _playableDirector;

            public TimeAnimationProperty(PlayableDirector playableDirector)
            {
                _playableDirector = playableDirector;
            }

            public string ID
            {
                get { return "nova_timeline"; }
            }

            public float value
            {
                get { return (float) _playableDirector.time; }
                set
                {
                    _playableDirector.time = value;
                    _playableDirector.Evaluate();
                }
            }
        }
        
        /// <summary>
        /// Start play a timeline asset, the play back time is controlled by NovaAnimation
        /// </summary>
        /// <remarks>
        /// You can call this method from lua script. The play back will start from given time.
        /// Only one clip can play at the same time, the old clip will be discarded
        /// </remarks>
        /// <param name="timelineAsset">The timeline asset to play</param>
        /// <param name="startTime">The time to start playback</param>
        public void Play(TimelineAsset timelineAsset, float startTime)
        {
            Assert.IsNotNull(timelineAsset, "the timeline asset to play should not be null");
            _animationTimer.Stop(_property);
            _playableDirector.playableAsset = timelineAsset;
            _playableDirector.time = startTime;
            var target = (float) _playableDirector.duration;
            var duration = target - startTime;
            _animationTimer.RegisterTransition(_property, target, duration);
        }

        /// <summary>
        /// Start play a timeline asset, the play back time is controlled by NovaAnimation
        /// </summary>
        /// <remarks>
        /// You can call this method from lua script. The play back will start from zero
        /// Only one clip can play at the same time, the old clip will be discarded
        /// </remarks>
        /// <param name="timelineAsset">The timeline asset to play</param>
        public void Play(TimelineAsset timelineAsset)
        {
            // It seems the Unity Editor does not work correctly with optional parameter :(
            Play(timelineAsset, 0f);
        }

        /// <summary>
        /// Stop animation play back.
        /// </summary>
        /// <remarks>
        /// The time line will jump to its last frame and stops playing, which is usually
        /// the desired behaviour for animations in AVG Games.
        /// You can call this method from script
        /// </remarks>
        public void Stop()
        {
            _animationTimer.Stop(_property);
        }
    }
}