using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace commons.Database;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CollectionNameAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}
