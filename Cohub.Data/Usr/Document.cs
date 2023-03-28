using System;

#nullable disable

namespace Cohub.Data.Usr
{
    public class Document
    {
        public override string ToString()
        {
            return Name;
        }

        public int Id { get; set; }
        public DateTimeOffset Posted { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }

        public virtual User Owner { get; set; }
    }
}
