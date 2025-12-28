namespace dev.vanHoof.ManusTest.WorldObjects.Objects
{
    /// <summary>
    /// Interface for Objects that can be (visually) selected.
    /// </summary>
    public interface ISelection
    {
        /// <summary>
        /// Select object.
        /// </summary>
        void Select();

        /// <summary>
        /// Deselect object.
        /// </summary>
        void Deselect();
    }
}