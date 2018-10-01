using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nova
{
    [RequireComponent(typeof(Text))]
    public class TextAnimator : MonoBehaviour
    {
        private class TextAppendingBuffer
        {
            private string _text;
            private int _currentIndex = 0;
            private readonly int _removeThreshold = 0;

            /// <summary>
            /// construct Text Appending Buffer with initial value and remove thredshold
            /// </summary>
            /// <param name="value"> The initial value of TextAppendingBuffer</param>
            /// <param name="removeThreshold">Unused characters will be removed if the number of the unused character
            /// exceeds this threshold</param>
            public TextAppendingBuffer(string value = "", int removeThreshold = 20)
            {
                _text = value;
                _removeThreshold = removeThreshold;
            }

            public char Next()
            {
                var result = _text[_currentIndex++];
                TryCleanUpUnused();
                return result;
            }

            public string Text
            {
                get { return _text.Substring(_currentIndex); }
            }

            public bool IsEmpty
            {
                get { return _currentIndex == _text.Length; }
            }

            public void Clear()
            {
                _text = "";
                _currentIndex = 0;
            }

            public void Append(string value)
            {
                TryCleanUpUnused();
                _text += value;
            }

            /// <summary>
            /// This method will clean up unused characters in buffer if
            /// + The number of unused characters exceeds the given thredshold.
            /// + All characters has been used
            /// </summary>
            /// <remarks>This method is called automatically</remarks>
            private void TryCleanUpUnused()
            {
                if (_currentIndex == 0) return;
                if (_currentIndex >= _removeThreshold || IsEmpty)
                {
                    _text = _text.Substring(_currentIndex);
                    _currentIndex = 0;
                }
            }
        }

        private Text _text;
        private StringBuilder _sb;
        private TextAppendingBuffer _appendingBuffer;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _sb = new StringBuilder(_text.text);
            _appendingBuffer = new TextAppendingBuffer();
        }

        public float CharacterDisplayDuration;

        public bool NeedAnimation;

        public void Set(string value)
        {
            _sb.Length = 0;
            Append(value);
            ForceUpdate();
        }

        public void Append(string value)
        {
            _appendingBuffer.Append(value);
        }

        /// <summary>
        /// Flush all characters in the appending buffer. immediately display full text on UI.
        /// </summary>
        public void Flush()
        {
            if (_appendingBuffer.IsEmpty) return;
            _sb.Append(_appendingBuffer.Text);
            _appendingBuffer.Clear();
            ForceUpdate();
        }

        public void ForceUpdate()
        {
            // update UI
            _text.text = _sb.ToString();
        }

        private float _timeSinceLastAppendChar = 0;

        private void Update()
        {
            if (_appendingBuffer.IsEmpty)
            {
                // waiting for next input
                _timeSinceLastAppendChar = CharacterDisplayDuration;
                return;
            }

            // Don't need Animation, flush immediately
            if (!NeedAnimation)
            {
                Flush();
                return;
            }

            _timeSinceLastAppendChar += Time.deltaTime;
            if (_timeSinceLastAppendChar < CharacterDisplayDuration) return;
            do
            {
                _sb.Append(_appendingBuffer.Next());
                _timeSinceLastAppendChar -= CharacterDisplayDuration;
            } while (_timeSinceLastAppendChar >= CharacterDisplayDuration && !_appendingBuffer.IsEmpty);

            ForceUpdate();
        }
    }
}