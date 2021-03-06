﻿using System.Collections.Generic;
using System.Text;

namespace Assets.Editor.CS_CompilerGeneated
{


    //
    public class CodeClass_t : MemberBlockEx_t
    {
        protected CodeClass_t parent = null;  //存在父亲

        //儿子
        public List<MemberBlock_t> subs = new List<MemberBlock_t>();

        public CodeClass_t(int deep)
            : base(deep)
        {
        }
        MemberBlock_t _sub = null;
        protected List<string> temp = new List<string>();
        public override void handleLine(string line)
        {
            if (_sub != null)
            {
                _sub.AddLine(line);
                _testEndSub();
                return;
            }
            if (line.StartsWith("["))
            {
                //是注释
                temp.Add(line);
            }
            else if (line.StartsWith("private sealed class "))
            {
                //找到迭代子类
                _sub = new CodeIteratorClass_t(_deep + 1);
                _sub.AddLine(line);
            }
            else if (line.StartsWith("public enum "))
            {
                //找到子类
                _sub = new CodeEnumClass_t(_deep + 1);
                _sub.AddLine(line);
            }
            else if (line.StartsWith("public class "))
            {
                //找到子类
                _sub = new CodeClass_t(_deep + 1);
                _sub.AddLine(line);
            }
            else //if (line.Contains("private FaeriaButtonDock craftModeButton;"))
            {
                _sub = new MemberBlock_t(_deep + 1);
                _sub.AddLine(line);
                _testEndSub();
                //LogError("识别的行:"+ line);
            }
        }



        void _testEndSub()
        {
            if (_sub.IsEnd)
            {
                _sub.attrs.AddRange(temp);
                subs.Add(_sub);
                _sub = null;
                temp.Clear();
            }
        }

        public override void Handle()
        {
            for (int i = 0; i < subs.Count; ++i)
            {
                CodeClass_t cc = subs[i] as CodeClass_t;
                if(cc!=null)
                {
                    cc.Handle();
                    continue;
                }

                if (subs[i].isIEnumerator())
                {
                    //找到
                    string func = _FetchIEnumeratorFuncName(subs[i]._title);
                    if(func!=null)
                    {
                        CodeIteratorClass_t iter = _FindIteratorClass(func);
                        if (iter != null)
                        {
                            iter.ReplaceTo(subs[i]);
                        }
                        else
                        {
                            LogError("没有找到对应的CodeIteratorClass_t: " + func);
                        }
                    }
                    else
                    {
                        LogError("没有找到 _FetchIEnumeratorFuncName: " + subs[i]._title);
                    }
                }
            }
        }
        //
        public override void Save(StringBuilder sb)
        {
            sb.AppendLine(this._title);
            sb.AppendLine("{");
            for (int i = 0; i < subs.Count; ++i)
            {
                subs[i].Save(sb);
            }
            sb.AppendLine("}");
        }

        protected void setTitle(string line)
        {
            if(rows.Count == 0)
                rows.Add(line);
            _title = line;
            _stat = State_e.State_Begin;
        }
        protected bool isClassDec(string line)
        {
            return true;
        }


        //根据名字
        CodeIteratorClass_t _FindIteratorClass(string func)
        {
            for (int i = 0; i < subs.Count; ++i)
            {
                if (subs[i].isCodeIteratorClass(func))
                {
                    return subs[i] as CodeIteratorClass_t;
                }
            }
            return null;
        }


    }


    public class CodeEnumClass_t : CodeClass_t
    {
        public CodeEnumClass_t(int deep)
            : base(deep)
        {
        }
        //
        public override void handleLine(string line)
        {
        }
        public override void Save(StringBuilder sb)
        {
            for (int i = 0; i < rows.Count; ++i)
            {
                sb.AppendLine(sdeep + rows[i]);
            }
        }
    }
}
