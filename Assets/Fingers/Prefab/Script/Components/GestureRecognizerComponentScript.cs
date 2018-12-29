//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRubyShared
{
    [System.Serializable]
    public class GestureRecognizerComponentStateUpdatedEvent : UnityEngine.Events.UnityEvent<DigitalRubyShared.GestureRecognizer> { }

    [System.Serializable]
    public class GestureRecognizerComponentEvent : UnityEngine.Events.UnityEvent { }

    [System.Serializable]
    public class GestureRecognizerComponentEventVector2 : UnityEngine.Events.UnityEvent<Vector2> { }

    public abstract class GestureRecognizerComponentScriptBase : MonoBehaviour
    {
        /// <summary>
        /// Different types of object modes for component gestures
        /// </summary>
        public enum GestureObjectMode
        {
            /// <summary>
            /// Gesture must execute on the game object
            /// </summary>
            RequireIntersectWithGameObject,

            /// <summary>
            /// Raycast will determine which object gets affected
            /// </summary>
            AllowOnAnyGameObjectViaRaycast
        }

        public DigitalRubyShared.GestureRecognizer GestureBase { get; protected set; }
    }

    public abstract class GestureRecognizerComponentScript<T> : GestureRecognizerComponentScriptBase where T : DigitalRubyShared.GestureRecognizer, new()
    {
        [Header("Gesture properties")]
        [Tooltip("Gesture state updated callback")]
        public GestureRecognizerComponentStateUpdatedEvent GestureStateUpdated;

        [Tooltip("The game object the gesture must execute over, null to allow the gesture to execute anywhere.")]
        public GameObject GestureView;

        [Tooltip("The minimum number of touches to track. This gesture will not start unless this many touches are tracked. Default is usually 1 or 2. Not all gestures will honor values higher than 1.")]
        [Range(1, 10)]
        public int MinimumNumberOfTouchesToTrack = 1;

        [Tooltip("The maximum number of touches to track. This gesture will never track more touches than this. Default is usually 1 or 2. Not all gestures will honor values higher than 1.")]
        [Range(1, 10)]
        public int MaximumNumberOfTouchesToTrack = 1;

        [Tooltip("Gesture components to allow simultaneous execution with. By default, gestures cannot execute together.")]
        public List<GestureRecognizerComponentScriptBase> AllowSimultaneousExecutionWith;

        [Tooltip("Whether to allow the gesture to execute simultaneously with all other gestures.")]
        public bool AllowSimultaneousExecutionWithAllGestures;

        public List<GestureRecognizerComponentScriptBase> RequireGestureRecognizersToFail;

        [Tooltip("Whether tracked touches are cleared when the gesture ends or fails, default is false. By setting to true, you allow the gesture to " +
            "possibly execute again with a different touch even if the original touch it failed on is still on-going. This is a special case, " +
            "so be sure to watch for problems if you set this to true, as leaving it false ensures the most correct behavior, especially " +
            "with lots of gestures at once.")]
        public bool ClearTrackedTouchesOnEndOrFail;

        public T Gesture { get; private set; }

        protected virtual void GestureStateUpdatedCallback(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (GestureStateUpdated != null)
            {
                GestureStateUpdated.Invoke(gesture);
            }
        }

        protected virtual void Awake()
        {
            
        }

        protected virtual void Update()
        {

        }

        protected virtual void LateUpdate()
        {

        }

        protected virtual void OnEnable()
        {
            Gesture = new T();
            GestureBase = Gesture;
            Gesture.StateUpdated += GestureStateUpdatedCallback;
            Gesture.PlatformSpecificView = GestureView;
            Gesture.MinimumNumberOfTouchesToTrack = MinimumNumberOfTouchesToTrack;
            Gesture.MaximumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack;
            Gesture.ClearTrackedTouchesOnEndOrFail = ClearTrackedTouchesOnEndOrFail;
            if (AllowSimultaneousExecutionWithAllGestures)
            {
                Gesture.AllowSimultaneousExecutionWithAllGestures();
            }
            else if (AllowSimultaneousExecutionWith != null)
            {
                foreach (GestureRecognizerComponentScriptBase gesture in AllowSimultaneousExecutionWith)
                {
                    Gesture.AllowSimultaneousExecution(gesture.GestureBase);
                }
            }
            foreach (GestureRecognizerComponentScriptBase gesture in RequireGestureRecognizersToFail)
            {
                Gesture.AddRequiredGestureRecognizerToFail(gesture.GestureBase);
            }

            FingersScript.Instance.AddGesture(Gesture);
        }

        protected virtual void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(Gesture);
            }
        }

        protected virtual void OnDestroy()
        {
            // OnDisable is called right before OnDestroy
        }
    }
}