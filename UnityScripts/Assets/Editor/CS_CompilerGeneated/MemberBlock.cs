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
        public string _title = "";
        public string type;    //默认的主type

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

        public void ClearBody()
        {
            rows.Clear();
        }
        public void AddRow(string row)
        {
            rows.Add(row);
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

        //
        public void AddLine(string line)
        {
            if (line == "")
            {
                return;
            }
            this.rows.Add(line);
            if (isClassStart(line))
            {
                _title = rows[rows.Count-2];    //
                //从前一行中，处理声明?
                _stat = State_e.State_Begin;
            }
            else if (isClassEnd(line))
            {
                _stat = State_e.State_End;
            }
            else
            {
                handleLine(line);
            }
        }

        //由派生类执行处理
        public virtual void handleLine(string line)
        {

        }

        public bool isIEnumerator()
        {
            return _title.StartsWith("public IEnumerator ");
        }
        public bool isCodeIteratorClass()
        {
            return _title.StartsWith("private sealed class ");
        }
        //public void AddLine(string line)
        //{
        //    this.rows.Add(line);
        //    //在状态开始前，先判断
        //    if (_stat == State_e.State_Null)
        //    {
        //        if (isClassDec(line))
        //        {
        //            setTitle(line);
        //        }
        //        else
        //        {

        //        }
        //    }
        //    else if (_stat == State_e.State_Begin)
        //    {
        //        if (isClassStart(line))
        //        {

        //        }
        //        else if (isClassEnd(line))
        //        {
        //            _stat = State_e.State_End;
        //        }
        //        else
        //        {
        //            //处理成员
        //        }
        //    }
        //}

    }

    //
    public class MemberBlockEx_t : MemberBlock_t
    {
        //复杂的表达式

    }
}
