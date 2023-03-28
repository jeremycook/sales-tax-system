using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteKit.EntityFrameworkCore
{
    public class CandidateDbContextTypes : IEnumerable<Type>
    {
        private readonly Type[] types;

        public CandidateDbContextTypes(Type[] types)
        {
            this.types = types;
        }

        public IEnumerator<Type> GetEnumerator()
        {
            return types.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return types.GetEnumerator();
        }
    }
}
