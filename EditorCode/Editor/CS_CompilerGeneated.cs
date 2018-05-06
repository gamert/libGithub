using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Editor
{
    class CS_CompilerGeneated
    {

        //一个代码块，表示一行，一个表达式，或者一个类
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

        public class MemberBlock_t: Block_t
        {
            //普通的表达式
            protected int _deep = 0;   //表示深度
            protected string sdeep = "";
            public void setDeep(int deep)
            {
                _deep = deep;
                for (int i = 0; i < _deep; ++i)
                {
                    sdeep = sdeep + " ";
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
                if(_stat == State_e.State_Null)
                {
                    if(isClassDec(line))
                    {
                        setTitle(line);
                    }
                    else
                    {

                    }

                }

                if (_stat == State_e.State_Begin)
                {
                    if(isClassStart(line))
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
                if(_sub!=null)
                {
                    _sub.handleMember(line);
                    if(_sub.IsEnd)
                    {
                        _sub = null;
                    }
                    return;
                }

                line = line.Substring(4*(1+_deep));   //先去掉
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

        //迭代器子类
        public class CodeIteratorClass_t : CodeClass_t
        {

            //string _0024this;//internal PrivateLobby _003ClocPrivateLobby_003E__1;
            string _0024this;// internal CoOpQuestDescriptionPopup _0024this;
            string _0024current;// internal object _0024current;
            string _0024disposing;//internal bool _0024disposing;
            string _0024PC;//internal int _0024PC;

            MemberBlock_t Current1 = new MemberBlock_t(); 
            MemberBlock_t Current2 = new MemberBlock_t(); 
            MemberBlock_t Iterator0 = new MemberBlock_t(); 

            MemberBlock_t MoveNext = new MemberBlock_t(); 

        }


        public class CodeFile_t
        {
            public Block_t _using = new Block_t();

            public CodeClass_t _main = new CodeClass_t();

            public void Load(string filepath)
            {
                string[] lines = File.ReadAllLines(filepath);
                for(int i = 0;i<lines.Length;++i)
                {
                    if(lines[i].StartsWith("//"))
                    {

                    }
                    else if (lines[i].StartsWith("using "))
                    {
                        _using.rows.Add(lines[i]);
                    }
                    else if(!_main.IsEnd)  //如果已经结束
                    {
                        _main.AddLine(lines[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
