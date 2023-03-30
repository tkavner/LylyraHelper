﻿using Celeste.Mods.LylyraHelper.Intefaces;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LylyraHelper.Entities
{

    /**
     * Cutting for vanilla entities is done on a case by case basis instead of calling the Cut(Vector2) method in a Cuttable item.
     * Due to the case by case nature of vanilla entities
     * 
     * @return: whether or not this item 
     */
    public static class CuttingHelper
    {
        public static bool Cut(Scene s, Entity toCut, Vector2 cutDirection, Vector2 cutPosition, int cutWidth, Type[] breaklist, Type[] ignorelist)
        {
            if (toCut is Cuttable cuttable)
            {
                return cuttable.Cut(cutDirection, cutPosition, cutWidth);
            } else //check for vanilla entity
            {

            }

            return false;
        }
    }
}