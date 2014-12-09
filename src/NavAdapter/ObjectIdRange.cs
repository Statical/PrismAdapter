using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

// TODO http://stackoverflow.com/questions/4490857/should-codecontracts-replace-the-regular-argumentexceptions
//          Contract.Requires(...) or Contract.Requires<ArgumentNullException>(...)

namespace NavAdapter
{
    public class ObjectIdRange
    {
        private int? Low { get; set; }
        private int? High { get; set; }

        public ObjectIdRange(int? low, int? high)
        {
            if (!low.HasValue && !high.HasValue)
            {
                throw new ArgumentException("Either low or high must be not null");
            }
            if (low.HasValue && high.HasValue && low.Value > high.Value)
            {
                throw new ArgumentException("Low must not be greater than high");
            }
            this.Low = low;
            this.High = high;
        }

        public static string WhereClause(ISet<ObjectIdRange> idRanges)
        {
            return string.Join(" OR ", idRanges.Select(r => r.WhereClause()));
        }

        public static string NavFilterExpression(ISet<ObjectIdRange> idRanges)
        {
            return string.Join("|", idRanges.Select(r => r.NavFilterExpression()));
        }

        private string WhereClause()
        {
            if (this.Low.HasValue && this.High.HasValue)
            {
                return "(" + this.Low.Value + " <= ID AND ID <= " + this.High.Value + ")";
            } 
            else if (this.High.HasValue)
            {
                return "ID <= " + this.High.Value;
            }
            else if (this.Low.HasValue)
            {
                return this.Low.Value + " <= ID";
            }
            else
            {
                throw new InvalidOperationException("Either low or high must be not null");
            }
        }

        private string NavFilterExpression()
        {
            if (this.Low.HasValue && this.High.HasValue)
            {
                return this.Low.Value + ".." + this.High.Value;
            }
            else if (this.High.HasValue)
            {
                return ".." + this.High.Value;
            }
            else if (this.Low.HasValue)
            {
                return this.Low.Value + "..";
            }
            else
            {
                throw new InvalidOperationException("Either low or high must be not null");
            }
        } 
    }
}
