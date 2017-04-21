using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zabbkit.Web
{
    internal static class Const
    {
        internal static class Validation
        {
            public const string MongoIdRegexp = "[0-9a-fA-F]{24,24}";
            public const string MongoIdErrorMessage = "Id must be 24-hex number";
            public const int MinTokenLength = 64; //iOS token length. Android and WP tokens are longer 
        }
    }
}