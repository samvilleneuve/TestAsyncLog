﻿// <author>Samuel Villeneuve</author>
// <date>2021-07-28</date>
// <Creation auteur = 'svilleneuve' date='28/07/21'/>
// <summary>An enumeration for the different results to take when an error is discovered.</summary>

namespace TestAsyncLog
{
    /// <summary>
    /// An enumeration for the different results to take when an error is discovered.
    /// </summary>
    public enum DetectionResult
    {
        /// <summary>
        /// Raise an exception on a potential CSRF problem.
        /// </summary>
        RaiseException = 0,

        /// <summary>
        /// Redirect to an error page specified in the configuration settings on a potential CSRF problem.
        /// </summary>
        Redirect = 1,

        /// <summary>
        /// Generate an HTTP 400 (Bad Request) on a potential CSRF problem.
        /// </summary>
        HTTP400BadRequest = 2
    }
}