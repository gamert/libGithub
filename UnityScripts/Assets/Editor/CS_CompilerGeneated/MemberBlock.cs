using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Editor.CS_CompilerGeneated
{

    //一个代码块
    public class Block_t
    {
        //普通的表达式
        public List<string> rows = new List<string>();
    }


    public enum State_e
    {
        State_Null = 0,
        State_Begin,
        State_End,
    }

    //管理一个成员块:包括带{}的函数，类等
    public class MemberBlock_t : Block_t
    {
        //普通的表达式
        protected int _deep = 0;   //表示深度
        protected string sdeep = "";
        public void setDeep(int deep)
        {
            _deep = deep;
            for (int i = 0; i < _deep; ++i)
            {
                sdeep = sdeep + "    ";
            }
        }
        protected State_e _stat = State_e.State_Null;   //未初始化
        public bool IsEnd { get { return _stat == State_e.State_End; } }

        protected bool isClassStart(string line)
        {
            return _stat == State_e.State_Begin && line.StartsWith(sdeep + "{");
        }
        protected bool isClassEnd(string line)
        {
            return _stat == State_e.State_Begin && line.StartsWith(sdeep + "}");
        }
    }

    //
    public class MemberBlockEx_t : MemberBlock_t
    {
        //复杂的表达式

    }
}
