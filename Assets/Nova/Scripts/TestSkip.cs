using UnityEngine;
using UnityEngine.EventSystems;

namespace Nova
{
    public class TestSkip : MonoBehaviour
    {
        public NarrowDialogueBoxController NarrowDialogueBoxController;

        void Start()
        {
            NarrowDialogueBoxController.SkipModeStarts.AddListener(OnSkipModeStarts);
            NarrowDialogueBoxController.SkipModeStops.AddListener(OnSkipModeEnds);
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