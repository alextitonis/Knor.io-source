using Invector;
using Invector.vCamera;
using Invector.vCharacterController;
using Invector.vCharacterController.vActions;
using Invector.vMelee;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Worq.AEAI.Enemy;

public class Player : MonoBehaviour
{
    public string ID;
    public int charId;
    public string charName;
    public float maxHealth;
    public float currentHealth;
    public Animator anim { get; protected set; }

    public AudioSource _audio;
    public AudioClip attackClip, footstepsClip, jumpClip, hitReactionClip;
    public GameObject canvas;
    public Text nameText;
    public Slider healthSlider;

    public EnemyAI _ai = null;

    protected Vector3 previousPosition = Vector3.zero;
    protected Quaternion previousRotation = new Quaternion(0f, 0f, 0f, 0f);

    public enum AudioType
    {
        None = -1,
        Movement = 0,
        Jump,
        Attack,
        HitReaction,
    }
    protected AudioType previousAudio = AudioType.None;

    public bool isLocal
    {
        get
        {
            return ID == NetworkManager.getInstance.localID;
        }
    }
    public bool isDead = false;

    public void SetUp(string ID, int charId, string charName, float maxHealth, float currentHealth)
    {
        if (!isLocal)
            GetComponent<Rigidbody>().useGravity = false;

        anim = GetComponent<Animator>();
        anim.speed = 1.2f;

        this.ID = ID;
        this.charId = charId;
        this.charName = charName;
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;

        InGameUIManager.getInstance.SetHealth(this);
       
        previousPosition = transform.position;
        previousRotation = transform.rotation;

        if (!isLocal)
        {
            gameObject.tag = "Enemy";
            gameObject.layer = 8;
            nameText.text = charName;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        else if (isLocal)
        {
            InGameUIManager.getInstance.ID = ID;
            Destroy(canvas);
            NetworkManager.getInstance.LocalCamera.GetComponent<vThirdPersonCamera>().SetTarget(transform);
            NetworkManager.getInstance.LocalCamera.GetComponent<vThirdPersonCamera>().SetMainTarget(transform);
            NetworkManager.getInstance.LocalCamera.GetComponent<vThirdPersonCamera>().Init();
        }
    }

    void Update()
    {
        if (!isLocal)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ChatManager.getInstance.Enter();

            if (CursorManager.getInstance.enter)
            {
                GetComponent<vThirdPersonController>().enabled = false;
                GetComponent<vMeleeCombatInput>().enabled = false;
                GetComponent<vGenericAction>().enabled = false;
                GetComponent<vMeleeManager>().enabled = false;
                GetComponent<vLockOn>().enabled = false;
            }
            else
            {
                GetComponent<vThirdPersonController>().enabled = true;
                GetComponent<vMeleeCombatInput>().enabled = true;
                GetComponent<vGenericAction>().enabled = true;
                GetComponent<vMeleeManager>().enabled = true;
                GetComponent<vLockOn>().enabled = true;
            }
        }

        if (Utils.CloseEnough(previousPosition, transform.position, 0.01) || Utils.CloseEnough(previousRotation, transform.rotation, 0.01))
        {
            int moveSetID = 1;
            if (Input.GetAxis("Horizontal") != 0.0f && Input.GetAxis("Vertical") != 0.0f)
                moveSetID = 3;

            NetworkManager.getInstance.Move(transform, Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), GetComponent<vThirdPersonController>().isGrounded, GetComponent<vThirdPersonController>().isCrouching, GetComponent<vThirdPersonController>().isStrafing, GetComponent<vThirdPersonController>().input.magnitude, moveSetID, GetComponent<vThirdPersonController>().verticalVelocity);
            previousPosition = transform.position;
            previousRotation = transform.rotation;
        }
    }
    IEnumerator ResetAnimations()
    {
        yield return new WaitForSeconds(0.15f);
        GetComponent<vMeleeCombatInput>().ResetAttackTriggers();
    }

    public void TakeDamage(vDamage damage)
    {
        if (damage == null)
            return;

        if (currentHealth <= 0 || isDead)
            return;

        NetworkManager.getInstance.TakeDamage(ID, damage.damageValue);
        NetworkManager.getInstance.AttackAnimation(ID, -1, false, false, false, true, 1);
        
        anim.SetFloat("HitReaction", 1);
        anim.SetTrigger("TriggerReaction");
        PlaySound(AudioType.HitReaction);
    }
    public void SetHealth(float value)
    {
        if (value > maxHealth)
            value = maxHealth;
        
        currentHealth = value;

        InGameUIManager.getInstance.SetHealth(this);

        if (!isLocal)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void PlaySound(AudioType type)
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