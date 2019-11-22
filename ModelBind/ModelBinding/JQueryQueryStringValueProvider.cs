using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ModelBinding
{
    /// <summary>
    /// The JQuery QueryString Value provider is used to handle JQuery formatted data in
    /// request QueryString.
    /// </summary>
    public class JQueryQueryStringValueProvider : NameValueCollectionValueProvider
    {
        /// <summary>
        /// Constructs a new instance of the JQuery QueryString ValueProvider
        /// </summary>
        /// <param name="controllerContext">The context on which the ValueProvider operates.</param>
        public JQueryQueryStringValueProvider(ControllerContext controllerContext)
                : this(controllerContext, controllerContext.HttpContext.Request.Unvalidated)
        {
        }

        // For unit testing
        internal JQueryQueryStringValueProvider(ControllerContext controllerContext, UnvalidatedRequestValuesBase unvalidatedValues)
            : base(controllerContext.HttpContext.Request.QueryString,
                        unvalidatedValues.QueryString,
                        CultureInfo.CurrentCulture,
                        jQueryToMvcRequestNormalizationRequired: true)
        {
        }
    }
}