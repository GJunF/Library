using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ModelBinding
{
    public class CookieValueProviderFactory : ValueProviderFactory
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

            return new CookieValueProvider(controllerContext, controllerContext.HttpContext.Request.Unvalidated);
        }
    }
}