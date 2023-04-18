using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Role
{
    public int sid;
    public int skillId_1
    {
        get {
            return 1000 + sid;
        }
    }
    public int skillId_2
    {
        get
        {
            return 10000 + sid;
        }
    }

    public string pathName
    {
        get
        {
            return sid > 3 ? "spineArt/role1/BunnyMaster_SkeletonData" : "spineArt/role2/Phoenix_SkeletonData";
        }
    }

    public string materialPathName
    {
        get
        {
            return sid > 3 ? "Spine/SkeletonGraphic" : "Spine/SkeletonGraphic";
        }
    }
    public int MaxHp;

    public int Hp;

    public int MaxShield;

    public int Shield;

    public string atkAnimaName = "atk";

    public string specialAtkAnimaName = "atk2";

    public string hitAnimaName = "hurt";

    public string skillAnimaName_1 = "kill1";

    public string skillAnimaName_2 = "kill2";
    public string idleAnimaName
    {
        get
        {
            return sid > 3 ? "idle" : "Idle";
        }
    }

    public bool Skill_1_CD
    {
        get {
            return Cd > MainManager.Ins.Timer;
        }
    }
    public int Cd;

    public int Skill_2_Value;

    public bool Skill_2_CD
    {
        get
        {
            return Skill_2_Value >= 30;
        }
    }

    public int hurtTimer;

    public Role()
    {
        sid = Random.Range(1, 6);
        MaxHp = 100;
        MaxShield = 50;
        Hp = MaxHp;
        Shield = MaxShield;

        Skill_2_Value = 30;
    }

    public void UseSkill1()
    {
        Cd = MainManager.Ins.Timer + 15;

        if (sid < 4)
        {
            //ÑÌÎíµ¯
        }
        else
        { 
            //¿ìËÙ³äÄÜ
        }
    }
    public bool IsRecoverHpSKill
    {
        get
        {
            return sid > 3;
        }
    }
    public int UseRecoverSkillTime;


    public void UseSkill2()
    {
        Skill_2_Value = 0;
        if (IsRecoverHpSKill)
            UseRecoverSkillTime = MainManager.Ins.Timer + 8;
    }
    public void ChangeSkill2Cd(int value)
    {
        Skill_2_Value += value;
    }
    public void UpdateSkill2(int  count)
    {
        ChangeSkill2Cd(1);
        if (count > 3 && count - 3 <=8 && MainManager.Ins.Timer < UseRecoverSkillTime)
        {
            addHpValue(count - 1);
        }
    }
    public void addHpValue(int changeValue)
    {
        if (Hp + changeValue >= MaxHp)
            Hp = MaxHp;
        else
            Hp += changeValue;

        if(changeValue < 0)
            hurtTimer = MainManager.Ins.Timer;
    }
    public void ChangeHpValue(int changeValue)
    {
        if (changeValue >= Shield)
        {
            Hp = Hp - (changeValue - Shield);
            Shield = 0;
        }
        else
        {
            Shield = Shield - changeValue;
        }
        if (Hp <= 0) Hp = 0;

        hurtTimer = MainManager.Ins.Timer;
    }
    public void ChangeHpValue2(int changeValue)
    {
        if (changeValue >= Hp)
        {
            Hp = 0;
        }
        else
        {
            Hp = Hp - changeValue;
        }

        hurtTimer = MainManager.Ins.Timer;
    }
    public void ChangeShildValue(int changeValue)
    {
        if (Shield + changeValue >= MaxShield)
            Shield = MaxShield;
        else
            Shield += changeValue;
    }


    public int ChangeShieldTime;
}
