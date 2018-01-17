using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.itac.mes.imsapi.domain.container;
using com.itac.oem.common.exceptions;

namespace com.amtec.common
{
    public static class ImsApiUtils
    {

        /// <summary>
        /// Convert array of <see cref="ImsApiKey"/> to string representation.
        /// </summary>
        /// <param name="imsApiKeys">array of <see cref="ImsApiKey"/> to convert</param>
        /// <returns>array <see cref="ImsApiKey"/> converted to string</returns>
        public static string[] createKeys(params ImsApiKey[] imsApiKeys)
        {
            string[] keys = new string[imsApiKeys.Length];
            for (int i = imsApiKeys.Length; --i >= 0; )
            {
                keys[i] = imsApiKeys[i].ToString();
            }
            return keys;
        }

        /// <summary>
        /// Creates array of <see cref="KeyValue"/> from provided keys and values.
        /// </summary>
        /// <param name="keys">array of <see cref="ImsApiKey"/></param>
        /// <param name="values">array of values corresponding to keys array</param>
        /// <returns><see cref="KeyValue"/> array created for provided keys and its values</returns>
        /// <exception cref="IllegalArgumentException"><code>keys</code> or <code>values</code> were <code>null</code>
        /// or sizes of their arrays varied</exception>
        public static KeyValue[] createKeyValues(ImsApiKey[] keys, params string[] values)
        {
            if (keys == null || values == null)
            {
                throw new IllegalArgumentException("keys or values cannot be null");
            }
            if (keys.Length != values.Length)
            {
                throw new IllegalArgumentException("You must provide the same number of keys and values");
            }
            KeyValue[] keyValues = new KeyValue[keys.Length];
            for (int i = keys.Length; --i >= 0; )
            {
                keyValues[i] = new KeyValue(keys[i].ToString(), values[i]);
            }
            return keyValues;
        }
    }
}