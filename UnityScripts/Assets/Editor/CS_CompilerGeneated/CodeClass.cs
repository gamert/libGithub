using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Editor.CS_CompilerGeneated
{


    //
    public class CodeClass_t : MemberBlockEx_t
    {
        protected CodeClass_t parent = null;  //存在父亲
        protected string _title = "";
        protected string type;    //默认的主type

        //儿子
        public List<MemberBlock_t> subs = new List<MemberBlock_t>();

        public void AddLine(string line)
        {
            //在状态开始前，先判断
            if (_stat == State_e.State_Null)
            {
                if (isClassDec(line))
                {
                    setTitle(line);
                }
                else
                {

                }

            }

            if (_stat == State_e.State_Begin)
            {
                if (isClassStart(line))
                {

                }
                else if (isClassEnd(line))
                {
                    _stat = State_e.State_End;
                }
                else
                {
                    //处理成员
                }
            }
        }

        CodeClass_t _sub = null;
        protected List<string> temp = new List<string>();
        protected void handleMember(string line)
        {
            if (_sub != null)
            {
                _sub.handleMember(line);
                if (_sub.IsEnd)
                {
                    _sub = null;
                }
                return;
            }

            line = line.Substring(4 * (1 + _deep));   //先去掉
            if (line.StartsWith("["))
            {
                //是注释
                temp.Add(line);
            }
            else if (line.StartsWith("private sealed class "))
            {
                //找到子类
                _sub = new CodeIteratorClass_t();
                _sub.setTitle(line);
            }
            else
            {

            }
        }

        protected void setTitle(string line)
        {
            _title = line;
            _stat = State_e.State_Begin;
        }
        protected bool isClassDec(string line)
        {
            return true;
        }


    }
}
