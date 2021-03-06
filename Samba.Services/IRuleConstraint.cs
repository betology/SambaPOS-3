﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services
{
    public interface IRuleConstraint
    {
        string Name { get; set; }
        string NameDisplay { get; }
        string Value { get; set; }
        IEnumerable<string> Values { get; }
        string Operation { get; set; }
        string[] Operations { get; set; }
        string GetConstraintData();
        bool ValueEquals(object parameterValue);
    }
}
