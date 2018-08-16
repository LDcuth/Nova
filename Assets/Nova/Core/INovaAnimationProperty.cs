namespace Nova
{
    /// <summary>
    /// The interface for properties that can be controlled by NovaAnimation component
    /// </summary>
    public interface INovaAnimationProperty
    {
        /// <summary>
        /// The unique id for this property
        /// </summary>
        string ID { get; }
        
        /// <summary>
        /// The value to be animated. You can use this value to update your actual property, like alpha or position.
        /// </summary>
        float value { get; set; }
    }
}