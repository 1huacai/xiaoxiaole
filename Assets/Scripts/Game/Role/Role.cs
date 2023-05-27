using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Role
{
    public bool isMin;
    public int side;
    public int skillId_1
    {
        get
        {
            return 1000 + side;
        }
    }
    public int skillId_2
    {
        get
        {
            return 10000 + side;
        }
    }

    public string pathName
    {
        get
        {
            return side > 1 ? "spineArt/role1/BunnyMaster_SkeletonData" : "spineArt/role2/Phoenix_SkeletonData";
        }
    }

    public string materialPathName
    {
        get
        {
            return side > 1 ? "Spine/SkeletonGraphic" : "Spine/SkeletonGraphic";
        }
    }

    public string Name;

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
            return side > 1 ? "idle" : "idle";
        }
    }

    public bool Skill_1_CD
    {
        get
        {
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

    public Role(string rname)
    {
        Name = rname;
        side = Random.Range(1, 3);
        MaxHp = 100;
        MaxShield = 50;
        Hp = MaxHp;
        Shield = MaxShield;

        Skill_2_Value = 30;
    }

    public void UseSkill1()
    {
        Cd = MainManager.Ins.Timer + 15;

        if (side < 2)
        {
            //烟雾弹
        }
        else
        {
            //快速充能
        }
    }
    public bool IsRecoverHpSKill
    {
        get
        {
            return side > 1;
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
    public void UpdateSkill2(int count)
    {
        ChangeSkill2Cd(1);
        if (count > 3 && count - 3 <= 8 && MainManager.Ins.Timer < UseRecoverSkillTime)
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

        if (changeValue < 0)
            hurtTimer = MainManager.Ins.Timer;

        HudManager.Ins.ShowHud(changeValue, HudType.huixue, isMin ? RoleType.min : RoleType.emmey);
    }
    public void ChangeHpValue(int changeValue)
    {
        Debug.Log(" -------- side:" + side + " -- change hp:" + changeValue);
        if (changeValue >= Shield)
        {
            HudManager.Ins.ShowHud(Shield, HudType.dun, isMin ? RoleType.min : RoleType.emmey);
            HudManager.Ins.ShowHud((changeValue - Shield), HudType.shanghai, isMin ? RoleType.min : RoleType.emmey);
            Hp = Hp - (changeValue - Shield);
            Shield = 0;
        }
        else
        {
            HudManager.Ins.ShowHud(Shield - changeValue, HudType.dun, isMin ? RoleType.min : RoleType.emmey);
            Shield = Shield - changeValue;
        }
        if (Hp <= 0) Hp = 0;

        hurtTimer = MainManager.Ins.Timer;
        Debug.Log(" -------- side:" + side + " -- Hp:" + Hp);
    }
    public void ChangeHpValue2(int changeValue)
    {
        if (changeValue >= Hp)
        {
            HudManager.Ins.ShowHud(Hp, HudType.shanghai, isMin ? RoleType.min : RoleType.emmey);
            Hp = 0;
        }
        else
        {
            HudManager.Ins.ShowHud(changeValue, HudType.shanghai, isMin ? RoleType.min : RoleType.emmey);
            Hp = Hp - changeValue;
        }

        hurtTimer = MainManager.Ins.Timer;
    }
    public void ChangeShieldValue(int changeValue)
    {
        if(Shield < MaxShield)
            HudManager.Ins.ShowHud(Shield + changeValue >= MaxShield ? MaxShield - changeValue : changeValue, HudType.jiadun, isMin ? RoleType.min : RoleType.emmey);
        if (Shield + changeValue >= MaxShield)
            Shield = MaxShield;
        else
            Shield += changeValue;
    }


    public int ChangeShieldTime;
}
