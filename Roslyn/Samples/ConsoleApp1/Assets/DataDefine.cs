namespace MobaGo.FlatBuffer
{
    //zTODO: 代码改为Script生成器：
    public enum kScriptSpellProc : ushort
    {
        SSP_7 = 7,  //
        SSP_Max = 89,
    };

    //
    public enum kSoldierExOrdi : ushort
    {
        Reserve = 0,
        Zhanshi = 1,  //近战
        Yuancheng = 2,
        Paobing = 3,
        Chaojibing = 4,
        Xiaolong = 7,
        Dalong = 8,
        Xunluobing = 10,
    };

    //z技能范围指示类型:
    public enum kSpellIndicatorOrdi : ushort
    {
        Auto = 0,
        Target = 1,
        Pos = 2,
        Orient = 3,
        Track = 4,
    };

    //
    public enum kSpellPickOrdi : ushort
    {
        Closest = 0,
        BlLess = 1,
        Self = 2,
        Orient = 3,
        BlLessTeammate = 4,

        TelePortTarget = 5,
        LowerHpEnermyButDargon = 6,
    };

    //
    public enum kSpellCastOrdi : ushort
    {
        All = 0,
        Enermy = 1,
        We = 2,
    };

    //Clear
    public enum kScriptSpellClearOrdi : ushort
    {
        Null = 0,
        Hurt = 1,
        Dead = 2,
        Skill = 4,
        Hurt2 = 5,
        HurtOrUseSkill = 6,
        DamageDeadOrUSeSkill = 7,
    };

    //Add:
    public enum kScriptSpellAddOrdi : ushort
    {
        Exclude = 1,
        Break = 2,
        Take = 3,
        Keep = 4,
        Reset = 5,
        Force = 6,
    };


    //数值类型
    public enum kNumberOrdi : ushort
    {
        Pure = 0,
        Ratio = 1,
        Random = 2,
        Seq = 3
    };

    //
    public enum kCPCondOrdi : ushort
    {
        C1 = 1,
        C2 = 2,
        C3 = 3,
        C4 = 4,
        C5 = 5,
        C6 = 6,
        C7 = 7,
        C8 = 8,
        C9 = 9,
        C10 = 10,
        C11 = 11,
        C12 = 12,
        C13 = 13,
        C14 = 14,
        C15 = 15,
        C16 = 16,
        C17 = 17,
        C18 = 18,
    };

    public enum kCPTriggleOrdi : ushort
    {
        T1 = 1,
        T2 = 2,
        T3 = 3,
        T4 = 4,
        T5 = 5,
        T6 = 6,
        T7 = 7,
        T8 = 8,
    };

    //扩展类型:
    public enum kMonsterExOrdi : ushort
    {
        Jungle = 2, //野怪
    };

    //
    public enum kOverlayFadeOrdi : ushort
    {
        Precent = 0,
        Mult = 1,
        Scale = 2,
    };

}