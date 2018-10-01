using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nova
{
    public class DialogueBoxController : MonoBehaviour, IPointerClickHandler, IDialogueBoxController
    {
        [Serializable]
        private enum DialogueUpdateMode
        {
            Overwrite,
            Append,
        }

        [SerializeField] private DialogueUpdateMode dialogueUpdateMode;

        /// <summary>
        /// The regex expression to find the name
        /// </summary>
        public string namePattern;

        /// <summary>
        /// The group of the name in the name pattern
        /// </summary>
        public int nameGroup;

        private GameState gameState;

        public GameObject nameBox;
        public Text nameTextArea;
        public TextAnimator dialogueTextArea;

        private void Awake()
        {
            gameState = Utils.FindNovaGameController().GetComponent<GameState>();
        }

        private void OnEnable()
        {
            Debug.Log("Enable");
            gameState.DialogueWillChange += OnDialogueWillChange;
            gameState.DialogueChanged += OnDialogueChanged;
            gameState.BranchOccurs += OnBranchOcurrs;
            gameState.BranchSelected += OnBranchSelected;
            gameState.CurrentRouteEnded += OnCurrentRouteEnded;
        }

        private void OnDisable()
        {
            Debug.Log("Disable");
            StopAllCoroutines();
            gameState.DialogueWillChange -= OnDialogueWillChange;
            gameState.DialogueChanged -= OnDialogueChanged;
            gameState.BranchOccurs -= OnBranchOcurrs;
            gameState.BranchSelected -= OnBranchSelected;
            gameState.CurrentRouteEnded -= OnCurrentRouteEnded;
        }

        private void OnCurrentRouteEnded(CurrentRouteEndedData arg0)
        {
            State = DialogueBoxState.Normal;
        }

        private void OnDialogueWillChange()
        {
            StopTimer();
        }

        private DialogueBoxState _state = DialogueBoxState.Normal;

        /// <summary>
        /// Current state of the dialogue box
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DialogueBoxState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                {
                    return;
                }

                switch (_state)
                {
                    case DialogueBoxState.Normal:
                        break;
                    case DialogueBoxState.Auto:
                        StopAuto();
                        AutoModeStops.Invoke();
                        break;
                    case DialogueBoxState.Skip:
                        StopSkip();
                        SkipModeStops.Invoke();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (value)
                {
                    case DialogueBoxState.Normal:
                        _state = DialogueBoxState.Normal;
                        break;
                    case DialogueBoxState.Auto:
                        BeginAuto();
                        AutoModeStarts.Invoke();
                        break;
                    case DialogueBoxState.Skip:
                        BeginSkip();
                        SkipModeStarts.Invoke();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value", value, null);
                }
            }
        }

        [SerializeField] private string type;

        public string Type
        {
            get { return type; }
        }

        public void NewPage()
        {
            OverwriteDialogueDisplay("");
        }

        public UnityEvent AutoModeStarts;
        public UnityEvent AutoModeStops;
        public UnityEvent SkipModeStarts;
        public UnityEvent SkipModeStops;

        private bool isAnimating
        {
            get { return dialogueTextArea.IsAnimating; }
        }


        private string currentDialogueText;

        /// <summary>
        /// The content of the dialogue box needs to be changed
        /// </summary>
        /// <param name="text"></param>
        private void OnDialogueChanged(DialogueChangedData dialogueData)
        {
            RestartTimer();
            var text = currentDialogueText = dialogueData.text;
            Debug.Log(string.Format("<color=green><b>{0}</b></color>", text));

            switch (dialogueUpdateMode)
            {
                case DialogueUpdateMode.Overwrite:
                    OverwriteDialogueDisplay(text);
                    break;
                case DialogueUpdateMode.Append:
                    AppendDialogueText(text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Check current state and set schedule skip
            SetSchedule();
        }

        private void SetSchedule()
        {
            TryRemoveSchedule();
            switch (State)
            {
                case DialogueBoxState.Normal:
                    break;
                case DialogueBoxState.Auto:
                    TrySchedule(GetAutoScheduledTime());
                    break;
                case DialogueBoxState.Skip:
                    TrySchedule(SkipDelay);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OverwriteDialogueDisplay(string dialogueText)
        {
            if (nameBox == null || nameTextArea == null)
            {
                dialogueTextArea.Set(dialogueText);
                return;
            }

            string currentName, currentDialogue;
            ParseDialogueText(dialogueText, out currentName, out currentDialogue);
            if (currentName == "")
            {
                nameBox.SetActive(false);
            }
            else
            {
                nameBox.SetActive(true);
                nameTextArea.text = currentName;
            }

            dialogueTextArea.Set(currentDialogue);
        }

        private void AppendDialogueText(string text)
        {
            dialogueTextArea.Append(text + "\n\n");
        }


        /// <summary>
        /// Set current name and current dialogue based on dialogue text
        /// </summary>
        /// <param name="text">the dialogue text</param>
        /// <param name="currentName">character name of this dialogue</param>
        /// <param name="currentDialogue">content of this dialogue</param>
        private void ParseDialogueText(string text, out string currentName, out string currentDialogue)
        {
            var m = Regex.Match(text, namePattern);
            var dialogueStartIndex = 0;
            if (m.Success)
            {
                currentName = m.Groups[nameGroup].Value;
                dialogueStartIndex = m.Length;
            }
            else // No name is found
            {
                currentName = "";
            }

            currentDialogue = text.Substring(dialogueStartIndex).Trim();
        }

        private DialogueBoxState stateBeforeBranch;

        /// <summary>
        /// Make the state normal when branch occurs
        /// </summary>
        /// <param name="branchOccursData"></param>
        private void OnBranchOcurrs(BranchOccursData branchOccursData)
        {
            stateBeforeBranch = State;
            State = DialogueBoxState.Normal;
        }

        public bool continueAutoAfterBranch;
        public bool continueSkipAfterBranch;

        /// <summary>
        /// Check if should restore the previous state before the branch happens
        /// </summary>
        /// <param name="branchSelectedData"></param>
        private void OnBranchSelected(BranchSelectedData branchSelectedData)
        {
            Assert.AreEqual(State, DialogueBoxState.Normal, "DialogueBoxState.Normal != DialogueBox.State");
            switch (stateBeforeBranch)
            {
                case DialogueBoxState.Normal:
                    break;
                case DialogueBoxState.Auto:
                    State = continueAutoAfterBranch ? DialogueBoxState.Auto : DialogueBoxState.Normal;
                    break;
                case DialogueBoxState.Skip:
                    State = continueSkipAfterBranch ? DialogueBoxState.Skip : DialogueBoxState.Normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Coroutine scheduledStepCoroutine = null;

        public float AutoWaitTimePerCharacter;


        private void TrySchedule(float scheduledDelay)
        {
            if (dialogueAvaliable)
            {
                scheduledStepCoroutine = StartCoroutine(ScheduledStep(scheduledDelay));
            }
        }

        private void TryRemoveSchedule()
        {
            if (scheduledStepCoroutine == null) return;
            StopCoroutine(scheduledStepCoroutine);
            scheduledStepCoroutine = null;
        }

        private float GetAutoScheduledTime()
        {
            return AutoWaitTimePerCharacter * currentDialogueText.Length;
        }

        /// <summary>
        /// Start auto
        /// </summary>
        /// <remarks>
        /// This method should be called when the state is normal
        /// </remarks>
        private void BeginAuto()
        {
            Assert.AreEqual(State, DialogueBoxState.Normal, "DialogueBoxState State != DialogueBoxState.Normal");
            _state = DialogueBoxState.Auto;
            TrySchedule(GetAutoScheduledTime());
        }


        /// <summary>
        /// Stop Auto
        /// </summary>
        /// <remarks>
        /// This method should be called when the state is auto
        /// </remarks>
        private void StopAuto()
        {
            Assert.AreEqual(State, DialogueBoxState.Auto, "DialogueBoxState State != DialogueBoxState.Auto");
            _state = DialogueBoxState.Normal;
            TryRemoveSchedule();
        }

        public float SkipDelay;
        private bool shouldNeedAnimation;

        private void StopCharacterAnimation()
        {
            dialogueTextArea.Flush();
        }

        private bool needAnimation
        {
            get { return dialogueTextArea.NeedAnimation; }
            set { dialogueTextArea.NeedAnimation = value; }
        }

        /// <summary>
        /// Begin skip
        /// </summary>
        /// <remarks>
        /// This method should be called when the state is normal
        /// </remarks>
        private void BeginSkip()
        {
            Assert.AreEqual(State, DialogueBoxState.Normal, "DialogueBoxState State != DialogueBoxState.Normal");
            // Stop character animation
            StopCharacterAnimation();
            shouldNeedAnimation = needAnimation;
            needAnimation = false;
            _state = DialogueBoxState.Skip;
            TrySchedule(SkipDelay);
        }

        /// <summary>
        /// Stop skip
        /// </summary>
        /// <remarks>
        /// This method should be called when the state is Skip
        /// </remarks>
        private void StopSkip()
        {
            Assert.AreEqual(State, DialogueBoxState.Skip, "DialogueBoxState State != DialogueBoxState.Skip");
            needAnimation = shouldNeedAnimation;
            _state = DialogueBoxState.Normal;
            TryRemoveSchedule();
        }

        private IEnumerator ScheduledStep(float scheduledDelay)
        {
            Assert.IsTrue(dialogueAvaliable, "Dialogue should available when a step scheduled for it");
            while (scheduledDelay > timeAfterDialogueChange)
            {
                yield return new WaitForSeconds(scheduledDelay - timeAfterDialogueChange);
            }

            // Pause one frame before step
            // Give time for rendering and can stop schedule step in time before any unwanted effects occurs
            yield return null;

            if (gameState.canStepForward)
            {
                gameState.Step();
            }
            else
            {
                State = DialogueBoxState.Normal;
            }
        }

        private float timeAfterDialogueChange;

        private bool dialogueAvaliable;

        private void StopTimer()
        {
            timeAfterDialogueChange = 0;
            dialogueAvaliable = false;
        }

        private void RestartTimer()
        {
            timeAfterDialogueChange = 0;
            dialogueAvaliable = true;
        }

        private void Update()
        {
            if (dialogueAvaliable)
            {
                timeAfterDialogueChange += Time.deltaTime;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (State == DialogueBoxState.Normal && !isAnimating)
            {
                gameState.Step();
                return;
            }

            if (isAnimating)
            {
                StopCharacterAnimation();
            }

            State = DialogueBoxState.Normal;
        }
    }
}