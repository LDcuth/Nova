namespace Nova
{
    
    /// <summary>
    /// All Dialogue Box Controller should implement this interface, so that they can be managed by DialogueBoxMananger.
    /// </summary>
    public interface IDialogueBoxController
    {
        /// <summary>
        /// Type of the dialogue box. It is case insensitive.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// ask the dialogue box to clear its current content
        /// </summary>
        void NewPage();
    }
}