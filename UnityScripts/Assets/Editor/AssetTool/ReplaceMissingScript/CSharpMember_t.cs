
using System;
using UnityEngine;

namespace AssetTool.RMS
{

    //Cinemachine.CinemachineBlendListCamera=-450300591
    public class CSharpMember_t
    {
        public string MemberType;

        public string Return;      //成员类型申明、返回类型
        public string Name;        //
        public string Params;      //参数列表

        //Method;Void set_LiveChild(ICinemachineCamera)
        public CSharpMember_t(string line)
        {
            string[] kv = line.Split(new char[] { '\t', ';' });
            if (kv.Length == 2)
            {
                //if(kv[0].Contains('.'))
                //{
                //}
                MemberType = kv[0];
                int p1 = kv[1].IndexOf(' ');
                if (p1 != -1)
                {
                    Return = kv[1].Substring(0, p1);
                    int p2 = kv[1].IndexOf('(');
                    if (p2 != -1)
                    {
                        Name = kv[1].Substring(p1 + 1, p2 - (p1 + 1));
                        Params = kv[1].Substring(p2 + 1);
                    }
                    else
                    {
                        Name = kv[1].Substring(p1 + 1);
                    }
                }
                else //NestedType;Cinemachine.CinemachineBlendListCamera+Instruction
                {
                    int p2 = kv[1].LastIndexOf('+');
                    if (p2 != -1)
                    {
                        Name = kv[1].Substring(p2 + 1);
                    }
                    else
                    {
                        Debug.LogError("未识别的成员类型：" + kv[1]);
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("DllMember_t fail: " + line);
            }
        }
    }


    //成员比较
    public class MemberCompareResult_t : IComparable<MemberCompareResult_t>
    {
        public CSharpMember_t src;
        public CSharpMember_t dst;

        public Int32 Distance;
        public Double Similarity;

        public MemberCompareResult_t(CSharpMember_t _src)
        {
            src = _src;
        }
        public int CompareTo(MemberCompareResult_t other)
        {
            return (int)((other.Similarity - this.Similarity) * 1000);
        }
    }
}
