using UnityEngine;

public class ClassSkillManager : MonoBehaviour
{
    private Player player;
    private Stats stats;

    private int hitCount;
    private float agilityTimer;
    private float activeCooldown;

    private bool activeSkillReady = true;
    private bool heavyStrike;
    private bool doubleShot;
    private bool empoweredSpell;

    private void Start()
    {
        player = GetComponent<Player>();
        stats = GetComponent<Stats>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            UseActiveSkill();

        if (agilityTimer > 0f)
            agilityTimer -= Time.deltaTime;

        if (activeCooldown > 0f)
        {
            activeCooldown -= Time.deltaTime;

            if (activeCooldown <= 0f)
                activeSkillReady = true;
        }
    }

    public void RegisterAttack()
    {
    }

    public void RegisterHit()
    {
        hitCount++;

        Class characterClass = stats.characterClass;

        if (characterClass == null)
            return;

        if (HasPassive(PassiveSkillType.Agility))
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.Agility);
            agilityTimer = skill.duration;
        }
    }

    public float GetDamageMultiplier(float distance)
    {
        Class characterClass = stats.characterClass;

        if (characterClass == null)
            return 1f;

        float multiplier = 1f;

        if (HasPassive(PassiveSkillType.StrongHit))
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.StrongHit);

            if (skill.attackCount > 0 && hitCount > 0 && hitCount % skill.attackCount == 0)
                multiplier *= 1f + skill.value / 100f;
        }

        if (HasPassive(PassiveSkillType.LongShot))
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.LongShot);

            if (distance >= 4f)
                multiplier *= 1f + skill.value / 100f;
        }

        if (HasPassive(PassiveSkillType.ElementBoost))
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.ElementBoost);

            if (stats.GetWeapon() != null && stats.GetWeapon().element != ElementType.None)
                multiplier *= 1f + skill.value / 100f;
        }

        if (heavyStrike || empoweredSpell)
        {
            ActiveSkill skill = characterClass.activeSkill;

            if (skill != null)
                multiplier *= 1f + skill.value / 100f;
        }

        return multiplier;
    }

    public float GetAttackSpeedMultiplier()
    {
        if (agilityTimer > 0f)
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.Agility);

            if (skill != null)
                return 1f + skill.value / 100f;
        }

        return 1f;
    }

    public float GetDamageReduction()
    {
        if (HasPassive(PassiveSkillType.Tough))
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.Tough);
            return Mathf.Clamp01(skill.value / 100f);
        }

        return 0f;
    }

    public float GetStaminaCostMultiplier()
    {
        if (HasPassive(PassiveSkillType.ManaSaving))
        {
            PassiveSkill skill = GetPassive(PassiveSkillType.ManaSaving);
            return Mathf.Clamp01(1f - skill.value / 100f);
        }

        return 1f;
    }

    public float GetKnockbackMultiplier()
    {
        if (heavyStrike)
        {
            Class characterClass = stats.characterClass;

            if (characterClass != null && characterClass.activeSkill != null)
                return characterClass.activeSkill.knockbackMultiplier;
        }

        return 1f;
    }

    public void UseActiveSkill()
    {
        Class characterClass = stats.characterClass;

        if (characterClass == null || characterClass.activeSkill == null)
            return;

        if (!activeSkillReady)
            return;

        ActiveSkill skill = characterClass.activeSkill;

        switch (skill.type)
        {
            case ActiveSkillType.HeavyStrike:
                heavyStrike = true;
                break;

            case ActiveSkillType.DoubleShot:
                doubleShot = true;
                break;

            case ActiveSkillType.EmpoweredSpell:
                empoweredSpell = true;
                break;
        }

        activeSkillReady = false;
        activeCooldown = skill.cooldown;

        Debug.Log("Active Skill Used: " + skill.skillName);
    }

    public void ConsumeAttackSkill()
    {
        heavyStrike = false;
        empoweredSpell = false;
    }

    public bool ConsumeDoubleShot()
    {
        if (!doubleShot)
            return false;

        doubleShot = false;
        return true;
    }

    public bool IsActiveSkillReady()
    {
        return activeSkillReady;
    }

    public float GetActiveCooldown()
    {
        return activeCooldown;
    }

    public int GetHitCount()
    {
        return hitCount;
    }

    public bool HasLongShot()
    {
        return HasPassive(PassiveSkillType.LongShot);
    }

    private bool HasPassive(PassiveSkillType type)
    {
        Class characterClass = stats.characterClass;

        if (characterClass == null)
            return false;

        if (characterClass.passiveSkill1 != null && characterClass.passiveSkill1.type == type)
            return true;

        if (characterClass.passiveSkill2 != null && characterClass.passiveSkill2.type == type)
            return true;

        return false;
    }

    private PassiveSkill GetPassive(PassiveSkillType type)
    {
        Class characterClass = stats.characterClass;

        if (characterClass == null)
            return null;

        if (characterClass.passiveSkill1 != null && characterClass.passiveSkill1.type == type)
            return characterClass.passiveSkill1;

        if (characterClass.passiveSkill2 != null && characterClass.passiveSkill2.type == type)
            return characterClass.passiveSkill2;

        return null;
    }
}