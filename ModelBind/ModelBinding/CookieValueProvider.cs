using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ModelBinding
{
    public class CookieValueProvider : NameValueCollectionValueProvider
    {
        public CookieValueProvider(ControllerContext controllerContext)
                : this(controllerContext, controllerContext.HttpContext.Request.Unvalidated)
        {
        }
        public CookieValueProvider(ControllerContext controllerContext, UnvalidatedRequestValuesBase unvalidatedValues)
            : base(ToNameValueCollection(controllerContext.HttpContext.Request.Cookies),
                        ToNameValueCollection(unvalidatedValues.Cookies),
                        CultureInfo.CurrentCulture,
                        jQueryToMvcRequestNormalizationRequired: false)
        {
        }

        public static NameValueCollection ToNameValueCollection(HttpCookieCollection cookieCollection)
        {
            var nvc = new NameValueCollection();
            foreach (var key in cookieCollection.AllKeys)
            {
                nvc.Add(key, cookieCollection[key].Value);
            }

            return nvc;
        }
    }
}
