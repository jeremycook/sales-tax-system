﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Cohub.Data.Geo
{
    public class Tz
    {
        public Tz()
        {
            Users = new HashSet<Usr.User>();
        }

        public override string ToString()
        {
            return Id;
        }

        public string Id { get; set; }

        public virtual ICollection<Usr.User> Users { get; set; }
    }
}
