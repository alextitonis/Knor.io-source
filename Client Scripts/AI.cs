using UnityEngine;
using Worq.AEAI.Enemy;

public class AI : Player
{
    [SerializeField] GameObject AiAvatar;
    public int AiDamage;
    bool isInit = false;

    bool isGrounded
    {
        get
        {
            float distanceToGround = GetComponent<Collider>().bounds.extents.y;
            return Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.1f);
        }
    }

    public void SetUpAI(string id, int charId, string _name, float maxHealth, float currentHealth)
    {
        ID = id;
        this.charId = charId;
        charName = _name;
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;

        isInit = true;

        if (!isLocal)
        {
            gameObject.tag = "Enemy";
            gameObject.layer = 8;
            nameText.text = charName;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        else
        {
            InGameUIManager.getInstance.ID = ID;
            Destroy(canvas);
        }

        anim = GetComponentInChildren<Animator>();
        _ai = GetComponentInChildren<EnemyAI>();
        _ai._Awake(this);
    }

    void Update()
    {
        if (!isLocal)
            return;

        if (!isInit)
            return;
        
        transform.position = AiAvatar.transform.position;
        transform.rotation = AiAvatar.transform.rotation;
        AiAvatar.transform.localPosition = Vector3.zero;
        AiAvatar.transform.localRotation = Quaternion.identity;

        if (previousPosition != transform.position || previousRotation != transform.rotation)
        {
            float horizontal = 0f;
            if (isGrounded)
                horizontal = 1f;
            NetworkManager.getInstance.Move(transform, horizontal, 0, isGrounded, false, false, 0, 0, 0);

            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }
    }

    public void Attack(Player p)
    {
        if (p.charId == charId)
            return;
        
        Invector.vDamage dmg = new Invector.vDamage();
        dmg.damageValue = AiDamage;
        p.TakeDamage(dmg);
    }
    public void Attack(AI a)
    {
        if (a.charId == charId)
            return;

        a.TakeDamage(5);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        if (currentHealth <= 0 || isDead)
            return;

        NetworkManager.getInstance.TakeDamage(ID, damage);
        NetworkManager.getInstance.AttackAnimation(ID, -1, false, false, false, true, 1);

        anim.SetFloat("HitReaction", 1);
        anim.SetTrigger("TriggerReaction");
        PlaySound(AudioType.HitReaction);
    }
    public new void SetHealth(float value)
    {
        if (value > maxHealth)
            value = maxHealth;

        currentHealth = value;

        if (!isLocal)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public new void PlaySound(AudioType type)
    {
        previousAudio = type;

        if (type == AudioType.Jump)
            return;

        switch (type)
        {
            case AudioType.Movement:
                if (_audio.isPlaying)
                    return;

                _audio.Stop();
                _audio.clip = footstepsClip;
                _audio.loop = false;
                _audio.Play();
                break;
            case AudioType.Attack:
                _audio.Stop();
                _audio.clip = attackClip;
                _audio.loop = false;
                _audio.Play();
                break;
            case AudioType.Jump:
                _audio.Stop();
                _audio.clip = jumpClip;
                _audio.loop = false;
                _audio.Play();
                break;
            case AudioType.HitReaction:
                _audio.Stop();
                _audio.clip = hitReactionClip;
                _audio.loop = false;
                _audio.Play();
                break;
            default:
                _audio.Stop();
                break;
        }
    }
}