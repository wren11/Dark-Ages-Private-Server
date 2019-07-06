namespace MenuInterpreter
{
    public class CheckpointHandlerArgs
    {
        /// <summary>
        ///     Checkpoint value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Checkpoint amount
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        ///     Checkpoint evaluating result
        ///     Set to true on success, and false on fail
        /// </summary>
        public bool Result { get; set; }
    }
}