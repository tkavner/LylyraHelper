using Celeste.Mod.LylyraHelper.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.LylyraHelper.Other;

public static class LyraUtils
{
    public static EntityData CloneEntityData(EntityData original, int width = 8, int height = 8)
    {

        return CloneEntityData(original, original.Position, width, height);
    }
    public static EntityData CloneEntityData(EntityData original, Vector2 position, int width = 8, int height = 8)
    {
        EntityData newData = new EntityData();

        newData.Nodes = original.Nodes;
        newData.Values = new(original.Values);
        newData.Name = original.Name;

        newData.Position = position;
        newData.Width = width;
        newData.Height = height;

        return newData;
    }
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