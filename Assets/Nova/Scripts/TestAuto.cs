using UnityEngine;
using UnityEngine.EventSystems;

namespace Nova
{
    public class TestAuto : MonoBehaviour
    {
        public NarrowDialogueBoxController NarrowDialogueBoxController;

        void Start()
        {
            NarrowDialogueBoxController.AutoModeStarts.AddListener(OnAutoModeStarts);
            NarrowDialogueBoxController.AutoModeStops.AddListener(OnAutoModeEnds);
        }

        private void OnAutoModeStarts()
        {
            Debug.Log("Auto Start");
           // dialogueBoxController.State = DialogueBoxState.Normal;
        }

        private void OnAutoModeEnds()
        {
            Debug.Log("Auto End");
        }
    }
}