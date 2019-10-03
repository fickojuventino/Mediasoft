using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaSoft.Data.Extensions
{ 
    public static class ConfigExtensions
    {
        public static string GetConfigValue(this IConfiguration config, string key)
        {
            string value = config[key];
            return value ?? throw new ArgumentException("Non existant key.");
        }
    }
}
