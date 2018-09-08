using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetTool.RMS
{

    //: IComparable<DllType_t>
    public class CSharpTypeCompareResult_t : IComparable<CSharpTypeCompareResult_t>, ISimpleCompare
    {
        public CSharpType_t refType;   //参考类型...
        public int percent;         //相似度
        public List<MemberCompareResult_t> strs = new List<MemberCompareResult_t>();

        private static StringBuilder _sb = new StringBuilder();
        public string Dump()
        {
            strs.Sort();

            _sb.Remove(0, _sb.Length);
            for (int i = 0; i < strs.Count; ++i)
            {
                if (strs[i].Similarity == 1)
                    _sb.AppendFormat("{0};", strs[i].src.Name);
                else
                    _sb.AppendFormat("{0}:{1}={2:F2};", strs[i].src.Name, strs[i].dst.Name, strs[i].Similarity);
            }
            return _sb.ToString();
        }

        public int CompareTo(CSharpTypeCompareResult_t other)
        {
            return percent - other.percent;
        }

        public int GetComparer()
        {
            return percent;
        }
    }

    //比较器:
    class NSResultCompare : IComparer
    {
        public static IComparer Default = new NSResultCompare();

        public int Compare(object x, object y)
        {
            return CSharpType_t.NSResultCompare((CSharpType_t)x, (CSharpType_t)y);
        }
    }

    //支持排序
    public class CSharpType_t : IComparable<CSharpType_t>
    {
        public string TypeWithNS;   //
        public string TypeName;
        public List<CSharpMember_t> Members = new List<CSharpMember_t>();
        public bool[] UseSign;      //用于匹配是表示是否已经完整匹配了
        public void InitUseSign()
        {
            if (UseSign == null && Members.Count > 0)
            {
                UseSign = new bool[Members.Count];
            }
            if (UseSign != null)
            {
                Array.Clear(UseSign, 0, UseSign.Length);
            }
        }

        //可以外部调整的参数:
        public static bool CONFIG_Type_LevenshteinDistance = true;
        public static bool CONFIG_Member_LevenshteinDistance = false;

        //按从高到低排序...
        public LimitLengthList<CSharpTypeCompareResult_t, int> CompareResult = new LimitLengthList<CSharpTypeCompareResult_t, int>();
        //相似的类型名字
        public LimitLengthList<CSharpTypeCompareResult_t, int> CompareNSResult = new LimitLengthList<CSharpTypeCompareResult_t, int>();

        //Cinemachine.CinemachineBlendListCamera=-450300591
        public CSharpType_t(string line)
        {
            string[] kv = line.Split(new char[] { '=' });
            if (kv.Length == 2)
            {
                //if(kv[0].Contains('.'))
                //{
                //}
                TypeWithNS = kv[0];
                string[] kv2 = TypeWithNS.Split(new char[] { '.' });
                TypeName = kv2[kv2.Length - 1];
            }
            else
            {
                UnityEngine.Debug.LogError("DllType_t fail: " + line);
            }
        }

        public void Add(string line)
        {
            Members.Add(new CSharpMember_t(line));
        }


        //使用降序
        public int CompareTo(CSharpType_t other)
        {
            if (other.CompareResult.Count == 0)
            {
                return 1;//空值比较小
            };
            if (this.CompareResult.Count == 0)
            {
                return -1;//空值比较大
            };
            if (other.CompareResult[0].strs.Count == this.CompareResult[0].strs.Count)
                return other.CompareResult[0].percent - this.CompareResult[0].percent;
            return other.CompareResult[0].strs.Count - this.CompareResult[0].strs.Count;
        }

        //比较NSResult
        public static int NSResultCompare(CSharpType_t src, CSharpType_t other)
        {
            //DllType_t src = (DllType_t)x;
            //DllType_t other = (DllType_t)y;

            if (other.CompareNSResult.Count == 0)
            {
                return 1;//空值比较小
            };
            if (src.CompareNSResult.Count == 0)
            {
                return -1;//空值比较大
            };
            return other.CompareNSResult[0].percent - src.CompareNSResult[0].percent;
            //return Comparer.Default.Compare(((DllType_t)x).Name, ((DllType_t)y).Name);
        }

        //zTODO: 如果匹配一致？包括完美一致？
        //使用词语近似性算法：LevenshteinDistance
        public MemberCompareResult_t Compare(CSharpMember_t src)
        {
            if (Members.Count == 0)
                return null;

            MemberCompareResult_t mcr = new MemberCompareResult_t(src);

            Int32 Distance;
            Double Similarity;
            for (int i = 0; i < Members.Count; ++i)
            {
                if (UseSign[i] == true)
                    continue;

                if (Members[i].Name == src.Name)
                {
                    this.UseSign[i] = true;

                    mcr.Distance = 0;
                    mcr.Similarity = 1;
                    mcr.dst = Members[i];
                    return mcr;
                }

                //忽略太短的词语
                if(CONFIG_Member_LevenshteinDistance)
                {
                    if (src.Name.Length > 4)
                    {
                        Distance = CCLevenshtein.LevenshteinDistance(Members[i].Name, src.Name, out Similarity, false);
                        if (Similarity > mcr.Similarity)
                        {
                            mcr.Distance = Distance;
                            mcr.Similarity = Similarity;
                            mcr.dst = Members[i];
                        }
                    }
                }
            }
            return mcr;
        }

        //2个类型比较，返回相似度: 0-100
        //z: 使用调节因子
        public CSharpTypeCompareResult_t Compare(CSharpType_t other)
        {

            if (Members.Count == 0)
                return null;

            other.InitUseSign();

            CSharpTypeCompareResult_t res = new CSharpTypeCompareResult_t();
            for (int i = 0; i < Members.Count; ++i)
            {
                //如果我的成员在ref集合中找到。。。
                MemberCompareResult_t mcr = other.Compare(Members[i]);
                if (mcr != null && mcr.Similarity >= 0.7)
                {
                    res.strs.Add(mcr);//Members[i].Name
                }
            }
            res.refType = other;
            res.percent = (int)(res.strs.Count * 100.0f / Members.Count);
            return res;
        }

        //看看本type是否在参考字典中存在：
        public void Compare(Dictionary<string, CSharpType_t> other)
        {
            CompareResult.Clear();

            //首先查找TypeWithNS完整的匹配的class，直接返回...
            CSharpType_t eg = null;
            other.TryGetValue(this.TypeName, out eg);
            if (eg != null && this.TypeWithNS == eg.TypeWithNS)
            {
                CSharpTypeCompareResult_t res = Compare(eg);
                if (res.percent > 80)
                {
                    CompareResult.Insert(res);
                    return;
                }
            }
            //忽略.成员数太小的Type
            if (this.Members.Count < 2)
                return;
            //遍历参考
            foreach (var p in other)
            {
                //比较类型名字:
                if(CSharpType_t.CONFIG_Type_LevenshteinDistance)
                {
                    if (this.TypeName.Length > 4)
                    {
                        double Similarity;
                        int Distance = CCLevenshtein.LevenshteinDistance(TypeName, p.Value.TypeName, out Similarity, false);
                        if (Similarity > 0.5)
                        {
                            CSharpTypeCompareResult_t n = new CSharpTypeCompareResult_t();
                            n.percent = (int)(Similarity * 1000);
                            n.refType = p.Value;
                            CompareNSResult.Insert(n);
                        }
                    }
                }

                //比较字典中的每个type,计算相似度:
                if (p.Value.Members.Count > 1)
                {
                    CSharpTypeCompareResult_t res = Compare(p.Value);
                    if (res.percent > 15)
                    {
                        CompareResult.Insert(res);
                    }
                }
            }
        }


        //
        public void Dump(StreamWriter sw)
        {
            if (CompareResult.Count > 0 && CompareResult[0].percent > 10)
            {
                sw.WriteLine(TypeWithNS + ":" + this.Members.Count);
                for (int i = 0; i < CompareResult.Count; ++i)
                {
                    string join = CompareResult[i].Dump();// string.Join(",", CompareResult[i].strs.ToArray());
                    sw.WriteLine(string.Format("\t[{0}%,{1}] {2},成员数{3}, 清单:{4}", CompareResult[i].percent, CompareResult[i].strs.Count, CompareResult[i].refType.TypeWithNS, CompareResult[i].refType.Members.Count, join));
                }
            }
        }
        //Dump相似的类型名字
        //percent = 0-100
        public void DumpNSResult(StreamWriter sw,int percent = 50)
        {
            int realpercent = percent * 10;
            if (CompareNSResult.Count > 0 && CompareNSResult[0].percent > realpercent )
            {
                sw.WriteLine(TypeWithNS + ":");
                for (int i = 0; i < CompareNSResult.Count; ++i)
                {
                    if(CompareNSResult[i].percent> realpercent)
                    {
                        sw.WriteLine(string.Format("\t[{0}%,{1},{2}] ", CompareNSResult[i].percent*0.1f, CompareNSResult[i].refType.TypeName, CompareNSResult[i].refType.TypeWithNS));
                    }
                }
            }
        }

    }//end of DllType

}
