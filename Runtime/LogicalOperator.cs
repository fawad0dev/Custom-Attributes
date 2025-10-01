namespace CustomAttributes
{
    /// <summary>
    /// Logical operators for combining multiple conditions
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// All conditions must be true (&&)
        /// </summary>
        And,
        
        /// <summary>
        /// Any condition can be true (||)
        /// </summary>
        Or
    }
}