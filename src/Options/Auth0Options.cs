namespace Options
{
    /// <summary>
    /// Class representing the options for the Open ID Connection to Auth0.
    /// </summary>
    public class Auth0Options
    {
        /// <summary>
        /// The domain redirected to the performs the Authentication and Authorization.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The ID of the client requesting the Authentication / Authorization.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The Client Secret shared between the application and the STS.
        /// </summary>
        public string ClientSecret { get; set; }

    }

}