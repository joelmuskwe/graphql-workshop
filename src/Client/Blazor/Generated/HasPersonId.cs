﻿using System;
using System.Collections;
using System.Collections.Generic;
using StrawberryShake;

namespace Client
{
    [System.CodeDom.Compiler.GeneratedCode("StrawberryShake", "11.0.0")]
    public class HasPersonId
        : IHasPersonId
    {
        public HasPersonId(
            string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
