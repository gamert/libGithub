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

        //儿子
        public List<MemberBlock_t> subs = new List<MemberBlock_t>();


        CodeClass_t _sub = null;
        protected List<string> temp = new List<string>();
        protected virtual void handleLine(string line)
        {
            if (_sub != null)
            {
                _sub.handleLine(line);
                if (_sub.IsEnd)
                {
                    subs.Add(_sub);
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
                //找到迭代子类
                _sub = new CodeIteratorClass_t();
                _sub.setTitle(line);
            }
            else if (line.StartsWith("public enum "))
            {
                //找到子类
                _sub = new CodeEnumClass_t();
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

        void processIEnumerator()
        {
            for(int i = 0;i< subs.Count;++i)
            {
                if(subs[i].isIEnumerator())
                {
                    //找到
                    string func = _FetchFuncName(subs[i]._title);
                    CodeIteratorClass_t iter = _FindIteratorClass(func);
                    iter.ReplaceTo(subs[i]);
                }
            }
        }
        string _FetchFuncName(string title)
        {
            return "";
        }

        CodeIteratorClass_t _FindIteratorClass(string func)
        {
            for (int i = 0; i < subs.Count; ++i)
            {
                if (subs[i].isCodeIteratorClass())
                {
                    return subs[i] as CodeIteratorClass_t;
                }
            }
            return null;
        }


    }


    public class CodeEnumClass_t : CodeClass_t
    {

    }
}
