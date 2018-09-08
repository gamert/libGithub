using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetTool.RMS
{

    public interface ISimpleCompare
    {
        int GetComparer();
    }

    //带长度限制的
    public class LimitLengthList<T, N> : List<T>
        where T : ISimpleCompare
    {
        public void Insert(T res)
        {
            for (int i = 0; i < this.Count; ++i)
            {
                //
                //                if (res.CompareTo(this[i])>=0)
                if (res.GetComparer() > this[i].GetComparer())
                {
                    this.Insert(i, res);
                    RemoveDuoyu();
                    return;
                }
            }
            if (this.Count < 10)
            {
                this.Add(res);
            }
            else
            {
                RemoveDuoyu();
            }
        }
        void RemoveDuoyu()
        {
            if (this.Count <= 10)
            {
                return;
            }
            this.RemoveRange(10, this.Count - 10);
        }

    }
}
