namespace Wellness.Core
{
    public class AppSettings
    {
        #region LoadBalancerSettings
        /// <summary>
        /// Gets or sets the NLB global file path.
        /// </summary>
        public string NlbGlobalFilePath { get; set; }

        /// <summary>
        /// Gets or sets the NLB local relative path.
        /// </summary>
        public string NlbLocalRelativePath { get; set; }

        /// <summary>
        /// Gets or sets the NLB HTML template.
        /// </summary>
        public string NlbHtmlTemplate { get; set; }
        #endregion
    }
}
