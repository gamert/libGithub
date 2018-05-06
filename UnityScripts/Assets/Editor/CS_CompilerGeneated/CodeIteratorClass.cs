﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Editor.CS_CompilerGeneated
{
    public class MoveNextBlock_t : MemberBlock_t
    {
        int substate = 0;

        public List<MemberBlock_t> subs = new List<MemberBlock_t>();

        MemberBlock_t _sub;
        public override void handleLine(string line)
        {
            if (line == "")
            {
                return;
            }
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

            if (substate == 0)
            {
                if (line.StartsWith("switch (num)"))
                {
                    substate++;
                }
            }
            else if (substate == 1)
            {
                if (line.StartsWith("switch (num)"))
                {
                    substate++;
                }
            }
        }
    }

    //迭代器子类
    public class CodeIteratorClass_t : CodeClass_t
    {
        List<string> _tmember = new List<string>(); //临时成员

        //
        //string _0024this;//internal PrivateLobby _003ClocPrivateLobby_003E__1;
        string _0024this;// internal CoOpQuestDescriptionPopup _0024this;
        string _0024current;// internal object _0024current;
        string _0024disposing;//internal bool _0024disposing;
        string _0024PC;//internal int _0024PC;

        MemberBlock_t Current1 = new MemberBlock_t();
        MemberBlock_t Current2 = new MemberBlock_t();
        MemberBlock_t Iterator0 = new MemberBlock_t();

        MemberBlock_t MoveNext = new MemberBlock_t();
        MemberBlock_t Dispose = new MemberBlock_t();
        MemberBlock_t Reset = new MemberBlock_t();

        public void Init()
        {
            Current1.setDeep(_deep+1);
            Current2.setDeep(_deep+1);
            Iterator0.setDeep(_deep+1);
            MoveNext.setDeep(_deep+1);
            Dispose.setDeep(_deep+1);
            Reset.setDeep(_deep+1);
        }

        //
        int substate = 0;
        void handleLine(string line)
        {
            if (line == "")
            {
                return;
            }
            switch (substate)
            {
                case 0:
                    if(line.Contains(" _0024this;"))
                    {
                        _0024this = line;
                        substate = 1;
                    }
                    else 
                    {
                        _tmember.Add(line);
                    }
                    break;
                case 1:
                    _0024current = line;
                    substate ++;
                    break;
                case 2:
                    _0024disposing = line;
                    substate++;
                    break;
                case 3:
                    _0024PC = line;
                    substate++;
                    break;
                case 4:
                    Current1.AddLine(line);
                    if (Current1.IsEnd)
                    {
                        substate++;
                    }
                    break;
                case 5:
                    Current2.AddLine(line);
                    if (Current2.IsEnd)
                    {
                        substate++;
                    }
                    break;
                case 6:
                    Iterator0.rows.Add(line);
                    if (Iterator0.IsEnd)
                    {
                        substate++;
                    }
                    break;
                case 7:
                    MoveNext.AddLine(line);
                    if (MoveNext.IsEnd)
                    {
                        substate++;
                    }
                    break;
                case 8:
                    Dispose.rows.Add(line);
                    if (Dispose.IsEnd)
                    {
                        substate++;
                    }
                    break;
                case 9:
                    Reset.AddLine(line);
                    if (Reset.IsEnd)
                    {
                        substate++;
                    }
                    break;
            }
        }

        //替换
        public void ReplaceTo(MemberBlock_t dst)
        {
            dst.ClearBody();
            //增加临时成员
            dst.AddRow(dst._title);
            dst.AddRow("{");
            for (int i = 0;i< _tmember.Count;++i)
            {
                dst.AddRow(_tmember[i]);
            }
            //MoveNext


            dst.AddRow("}");
        }
    }

}
