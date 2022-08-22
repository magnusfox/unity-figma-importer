﻿using System;

namespace Cdm.Figma.UI
{
    [AttributeUsage(AttributeTargets.Class |AttributeTargets.Field | AttributeTargets.Property)]
    public class FigmaNodeAttribute : Attribute
    {
        public string bindingKey { get; }

        public FigmaNodeAttribute()
        {
        }
        
        public FigmaNodeAttribute(string bindingKey)
        {
            this.bindingKey = bindingKey;
        }
    }
}