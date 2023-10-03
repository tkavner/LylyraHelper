using Celeste.Mod.LylyraHelper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Other
{
    public static class LyraUtils
    {
        public static List<string> GetFullNames(string unparsedNames)
        {
            if (unparsedNames == null) return null;
            if (unparsedNames == "") return new List<string>();
            if (FrostHelperImports.FrostHelperLoaded)
            {
                Type[] typeArray = FrostHelperImports.GetTypesFH(unparsedNames);
                List<string> toReturn = new List<string>();
                foreach (Type type in typeArray)
                {
                    toReturn.Add(type.FullName);
                }
                return toReturn;
            } else
            {
                if (unparsedNames == null) unparsedNames = "";
                string[] typeNames = unparsedNames?.Split(',');
                if (typeNames == null || typeNames.Length == 0)
                {
                    return new List<string>();
                }
                return typeNames.ToList();
            }
        }
    }
}
