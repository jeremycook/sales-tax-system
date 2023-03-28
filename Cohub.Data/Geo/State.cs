using System;
using System.Collections.Generic;

#nullable disable

namespace Cohub.Data.Geo
{
    public class State
    {
        public override string ToString()
        {
            return Id;
        }

        public string Id { get; set; }
    }
}
