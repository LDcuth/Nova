using UnityEngine;
using UnityEngine.EventSystems;

namespace Nova
{
    public class TestSkip : MonoBehaviour
    {
        public DialogueBoxController DialogueBoxController;

        void Start()
        {
            DialogueBoxController.SkipModeStarts.AddListener(OnSkipModeStarts);
            DialogueBoxController.SkipModeStops.AddListener(OnSkipModeEnds);
        }

        private void OnSkipModeStarts()
        {
            Debug.Log("Skip Start");
           // dialogueBoxController.State = DialogueBoxState.Normal;
        }

        private void OnSkipModeEnds()
        {
            Debug.Log("Skip End");
        }
    }
}