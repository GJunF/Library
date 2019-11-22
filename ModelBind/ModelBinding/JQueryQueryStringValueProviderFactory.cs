using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ModelBinding
{
    /// <summary>
    /// Provides the necessary ValueProvider to handle JQuery QueryString data.
    /// </summary>
    public class JQueryQueryStringValueProviderFactory : ValueProviderFactory
    {
        /// <summary>
        /// Returns the suitable ValueProvider.
        /// </summary>
        /// <param name="controllerContext">The context on which the ValueProvider should operate.</param>
        /// <returns></returns>
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }

            return new JQueryQueryStringValueProvider(controllerContext, controllerContext.HttpContext.Request.Unvalidated);
        }
    }
}