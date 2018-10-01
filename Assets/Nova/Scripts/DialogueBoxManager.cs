using System.Collections.Generic;
using UnityEngine;

namespace Nova
{
    /// <summary>
    /// The class that manages all Dialogue Box Controllers.
    /// Only one of all Dialogue Box Controllers can be activated at the same time. Switch between different Dialogue
    /// Box Controllers by changing the Mode of the DialogueBoxManager
    /// </summary>
    public class DialogueBoxManager : MonoBehaviour
    {
        /// <summary>
        /// All Dialogue Box Controllers to be managed
        /// </summary>
        public List<GameObject> DialogueBoxControllerGameObjects;

        private Dictionary<string, IDialogueBoxController> _dialogueBoxControllers;

        private void Awake()
        {
            // prepare data structure
            _dialogueBoxControllers = new Dictionary<string, IDialogueBoxController>();
            foreach (var dialogueGameObject in DialogueBoxControllerGameObjects)
            {
                var controller = dialogueGameObject.GetComponent<IDialogueBoxController>();
                _dialogueBoxControllers.Add(controller.Type.ToLower(), controller);
            }
        }

        private void Start()
        {
            // find the first activated controller, make it the current activated mode
            // All other controller will be deactivated
            foreach (var controller in _dialogueBoxControllers.Values)
            {
                var controllerBehaviour = controller as MonoBehaviour;
                if (controllerBehaviour == null)
                {
                    continue;
                }

                // default mode has been detected, all other dialogue box should be deactivated
                if (_mode != null)
                {
                    controllerBehaviour.gameObject.SetActive(false);
                    continue;
                }

                if (!controllerBehaviour.gameObject.activeSelf) continue;
                _mode = controller.Type;
            }
        }

        private static void SetActive(IDialogueBoxController controller, bool value)
        {
            var controllerBehaviour = controller as MonoBehaviour;
            if (controllerBehaviour == null)
            {
                return;
            }

            controllerBehaviour.gameObject.SetActive(value);
        }

        private string _mode;

        public string Mode
        {
            get { return _mode; }
            set
            {
                value = value.ToLower();
                if (value == _mode) return;
                IDialogueBoxController nextController;
                if (!_dialogueBoxControllers.TryGetValue(value, out nextController))
                {
                    Debug.LogFormat("Nova: Invalid dialogue box mode {}", value);
                }

                // if old mode is on, turn it off
                if (_mode != null)
                {
                    var currentController = _dialogueBoxControllers[_mode];
                    SetActive(currentController, false);
                }

                // turn on the next dialogue controller
                // next controller is definitely not null.
                SetActive(nextController, true);
                _mode = value;
            }
        }

        public IDialogueBoxController CurrentDialogueBoxController
        {
            get
            {
                IDialogueBoxController controller;
                return _dialogueBoxControllers.TryGetValue(Mode, out controller) ? controller : null;
            }
        }

        public void NewPage()
        {
            var controller = CurrentDialogueBoxController;
            if (controller != null)
            {
                controller.NewPage();
            }
        }
    }
}