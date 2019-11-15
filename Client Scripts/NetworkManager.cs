using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using Invector.vCharacterController;
using Worq.AEAI.Enemy;
using static MenuManager;
using static CharacterSelectionManager;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager getInstance;
    void Awake() { getInstance = this; }

    [DllImport("__Internal")]
    private static extern void ToggleSocial(bool display);    
    [DllImport("__Internal")]
    private static extern void ToggleAds(bool display); 

    bool inGame = false;
    [SerializeField] GameObject warriorPrefab, orcPrefab, warriorAIPrefab, orcAIPrefab;
    [SerializeField] GameObject cameraPrefab;
    [SerializeField] GameObject cam;

    public string localID = "";
    // SocketIOControllerWeb socket;
    SocketIOController socket;
    public GameObject localPlayer;

    public bool isBotClient = false;
    public bool isBotOrc = false;
    public BotClass _bot = null;
    [Serializable]
    public class BotClass
    {
        public PlayerFaction faction;
        public string _name;
    }

    public Dictionary<string, Player> players = new Dictionary<string, Player>();
    public GameObject LocalCamera;

    public bool LocalPlayerExists
    {
        get
        {
            if (localPlayer == null)
                return false;

            if (!localPlayer.GetComponent<Player>().isLocal)
                return false;

            return true;
        }
    }
    public bool isLocalPlayer(string id)
    {
        return id == localID;
    }

    void botsetup(PlayerFaction faction)
    {
        string _name = Utils.RandomString(8);
        _bot = new BotClass { faction = faction, _name = _name };
    }
    void Start()
    {
        if (isBotClient && isBotOrc) {
            botsetup(PlayerFaction.Orc);
        } else if (isBotClient && !isBotOrc){
            botsetup(PlayerFaction.Warrior);
        }

        // socket = GetComponent<SocketIOControllerWeb>();
        socket = GetComponent<SocketIOController>();
        socket.Connect();
        socket.On("open", OnConnected);
        socket.On("register", OnRegister);
        socket.On("zone_data", OnInitialZoneData);
        socket.On("name", OnName);
        socket.On("spawn", OnSpawn);
        socket.On("move", OnMove);
        socket.On("jump", OnJump);
        socket.On("disconnected", OnDisconnected);
        socket.On("damage", OnDamage);
        socket.On("died", OnDeath);
        socket.On("respawn1", OnRespawnRequest);
        socket.On("respawn2", OnRespawn);
        socket.On("attack_animation", OnAttackAnimation);
        socket.On("reset_animation", OnResetAnimation);
        socket.On("score", OnScore);
        socket.On("chat", OnChat);
        socket.On("zone_update", OnZoneUpdate);
        socket.On("ai_animation", OnAIPlayAnimation);
        socket.On("disconnect", delegate { Application.Quit(); });
    }

    void OnApplicationQuit()
    {
        DisconnectedPacket packet = new DisconnectedPacket();
        packet.id = localID;

        socket.Emit("disconnected", JsonUtility.ToJson(packet));
    }

    private void OnConnected(SocketIOEvent obj)
    {
    }
    private void OnRegister(SocketIOEvent obj)
    {
        RegistrationPacket packet = JsonUtility.FromJson<RegistrationPacket>(obj.data);
        localID = packet.id;

        RegisterAsBot();
    }
    private void OnInitialZoneData(SocketIOEvent obj)
    {
        ZoneDataPacket packet = JsonUtility.FromJson<ZoneDataPacket>(obj.data);
        ZoneManager.getInstance.Add(packet);
    }
    private void OnName(SocketIOEvent obj)
    {
        MenuManager.getInstance.Change(screen.CharacterSelection);
    }
    private void OnSpawn(SocketIOEvent obj)
    {
        SpawnPacket packet = JsonUtility.FromJson<SpawnPacket>(obj.data);

        string id = packet.id;

        if (id == localID)
            MenuManager.getInstance.Change(screen.InGame);

        string _name = packet.name;
        bool isBot = packet.isBot;
        Vector3 position = new Vector3(packet.posX, packet.posY, packet.posZ);
        Quaternion rotation = new Quaternion(packet.rotX, packet.rotY, packet.rotZ, packet.rotW);
        int charId = packet.charId;
        float maxHealth = packet.maxHealth;
        float currentHealth = packet.currentHealth;

        Debug.Log("Spawning: " + (PlayerFaction)charId + " is bot: " + isBot + " at: " + position);
        GameObject playerPrefab = null;
        if (!isBot)
        {
            playerPrefab = warriorPrefab;
            if ((PlayerFaction)charId == PlayerFaction.Orc)
                playerPrefab = orcPrefab;
        }
        else
        {
            playerPrefab = warriorAIPrefab;
            if ((PlayerFaction)charId == PlayerFaction.Orc)
                playerPrefab = orcAIPrefab;
        }

        GameObject player = Instantiate(playerPrefab, position, rotation);

        if (!isBot)
        {
            if (id == localID)
            {
                cam.SetActive(false);
                LocalCamera = Instantiate(cameraPrefab, player.transform.position, player.transform.rotation);
            }
        }
        
        if (players.ContainsKey(id))
        {
            Debug.Log("Trying to respawn player with id: " + id);
            return;
        }
        if (!isBot)
        {
            Player p = player.GetComponent<Player>();
            if (id == localID)
                localPlayer = player;

            p.SetUp(id, charId, _name, maxHealth, currentHealth);
            players.Add(p.ID, p);
        }
        else
        {
            AI p = player.GetComponent<AI>();
            if (id == localID)
                localPlayer = player;

            Debug.Log(p == null);
            p.SetUpAI(id, charId, _name, maxHealth, currentHealth);
            players.Add(id, p);
        }
    }
    private void OnMove(SocketIOEvent obj)
    {
        MovementPacket packet = JsonUtility.FromJson<MovementPacket>(obj.data);

        string id = packet.id;
        if (!players.ContainsKey(id))
            return;

        if (players[id].isDead)
            return;

        Vector3 position = new Vector3(packet.x, packet.y, packet.z);
        Quaternion rotation = new Quaternion(packet.rx, packet.ry, packet.rz, packet.rw);

        if (!players.ContainsKey(id))
            return;

        players[id].transform.position = position;
        players[id].transform.rotation = rotation;
        AI ai = players[id].GetComponent<AI>();
        if (ai == null)
        {
            players[id].anim.SetFloat("InputHorizontal", packet.horizontal);
            players[id].anim.SetFloat("InputVertical", packet.vertical);
            players[id].anim.SetFloat("InputMagnitude", packet.inputMagnitude);
            players[id].anim.SetFloat("MoveSet_ID", packet.moveSetID);
            players[id].anim.SetBool("IsGrounded", packet.isGrounded);
            players[id].anim.SetBool("IsCrouching", packet.isCrouching);
            players[id].anim.SetBool("IsStrafing", packet.isStrafing);
            players[id].anim.SetFloat("VerticalVelocity", packet.verticalVelocity);

            if (packet.horizontal != 0.0f || packet.vertical != 0.0f)
                if (players[id].GetComponent<vThirdPersonController>().isGrounded)
                    players[id].PlaySound(Player.AudioType.Movement);
        }
        else
        {
            if (packet.horizontal != 0.0f || packet.vertical != 0.0f)
                players[id].anim.SetTrigger("isWalking");
            else
                players[id].anim.ResetTrigger("isWalking");
        }
    }
    private void OnJump(SocketIOEvent obj)
    {
        JumpAnimationPacket packet = JsonUtility.FromJson<JumpAnimationPacket>(obj.data);

        string id = packet.id;
        if (!players.ContainsKey(id))
            return;

        players[id].PlaySound(Player.AudioType.Jump);

        switch (packet.jumpMove)
        {
            case true:
                players[id].anim.CrossFadeInFixedTime("JumpMove", 0.1f);
                break;
            case false:
                players[id].anim.CrossFadeInFixedTime("Jump", 0.1f);
                break;
        }
    }
    private void OnAttackAnimation(SocketIOEvent obj)
    {
        AttackAnimationPacket packet = JsonUtility.FromJson<AttackAnimationPacket>(obj.data);

        string id = packet.id;
        if (!players.ContainsKey(id))
            return;

        players[id].anim.SetInteger("RandomAttack", packet.randomAttack);
        if (packet.weakAttack)
            players[id].anim.SetTrigger("WeakAttack");
        if (packet.strongAttack)
            players[id].anim.SetTrigger("StrongAttack");
        if (packet.weakAttack || packet.strongAttack)
            players[id].PlaySound(Player.AudioType.Attack);
        players[id].anim.SetBool("IsBlocking", packet.isBlocking);
        if (packet.triggerHitReaction)
        {
            players[id].anim.SetFloat("HitReaction", packet.hitReaction);
            players[id].anim.SetTrigger("TriggerReaction");
            players[id].PlaySound(Player.AudioType.HitReaction);
        }

        if (packet.weakAttack || packet.strongAttack)
        {
            StopCoroutine(ResetAnimations(id));
            StartCoroutine(ResetAnimations(id));
        }
    }
    private void OnResetAnimation(SocketIOEvent obj)
    {
        AnimationResetPacket packet = JsonUtility.FromJson<AnimationResetPacket>(obj.data);

        string id = packet.id;
        if (!players.ContainsKey(id))
            return;

        if (packet.resetState)
            players[id].anim.SetTrigger("ResetState");
    }
    private void OnDamage(SocketIOEvent obj)
    {
        DamagePacket packet = JsonUtility.FromJson<DamagePacket>(obj.data);

        string id = packet.id;
        if (!players.ContainsKey(id))
            return;

        players[id].SetHealth(packet.damage);
    }
    private void OnDisconnected(SocketIOEvent obj)
    {
        Debug.Log("Disconnection packet: " + obj.data);
        DisconnectedPacket packet = JsonUtility.FromJson<DisconnectedPacket>(obj.data);
        string id = packet.id;

        if (players.ContainsKey(id))
        {
            Destroy(players[id].gameObject);
            players.Remove(id);
        }

        if (isLocalPlayer(id))
        {
            Destroy(localPlayer);
            localPlayer = null;

            cam.SetActive(true);
            Destroy(LocalCamera);
            LocalCamera = null;

            MenuManager.getInstance.Change(screen.Starting);
        }
    }
    private void OnDeath(SocketIOEvent obj)
    {
        DeathPacket packet = JsonUtility.FromJson<DeathPacket>(obj.data);
        string id = packet.id;
        
        players[id].isDead = true;
        if (players[id].GetComponent<AI>() != null)
        {
            players[id]._ai.PlayDeathAnim();
            StartCoroutine(removeAI(id));
        }
        else
        {
            if (isLocalPlayer(id))
            {
                try {
                    ToggleAds(true);
                } catch (Exception ex) { 
                    Debug.Log(ex.Message + " | " + ex.StackTrace); 
                }
            }

            players[id].GetComponent<vThirdPersonController>().currentHealth = 0;
        }
    }
    private void OnRespawnRequest(SocketIOEvent obj)
    {
        //respawn request
        if (!isBotClient)
            InGameUIManager.getInstance.UpdateRespawnPanel();
    }
    private void OnRespawn(SocketIOEvent obj)
    {
        RespawnPacket packet = JsonUtility.FromJson<RespawnPacket>(obj.data);
        
        //the thread for respawn starts here
        StartCoroutine(RespawnPlayer(packet));
    }
    private void OnScore(SocketIOEvent obj)
    {
        ScorePacket packet = JsonUtility.FromJson<ScorePacket>(obj.data);
        InGameUIManager.getInstance.SetScore(packet.k1, packet.k2);
    }
    private void OnChat(SocketIOEvent obj)
    {
        ChatPacket packet = JsonUtility.FromJson<ChatPacket>(obj.data);

        ChatManager.getInstance.HandleMessage(packet);
    }
    private void OnZoneUpdate(SocketIOEvent obj)
    {
        ZoneUpdatePacket packet = JsonUtility.FromJson<ZoneUpdatePacket>(obj.data);
        ZoneManager.getInstance.SetOwner(packet);
    }

    public void Move(Transform p, float horizontal, float vertical, bool isGrounded, bool isCrouching, bool isStrafing, float inputMagnitude, float moveSetID, float verticalVelocity)
    {
        MovementPacket packet = new MovementPacket();
        packet.x = p.position.x;
        packet.y = p.position.y;
        packet.z = p.position.z;
        packet.rx = p.rotation.x;
        packet.ry = p.rotation.y;
        packet.rz = p.rotation.z;
        packet.rw = p.rotation.w;
        packet.horizontal = horizontal;
        packet.vertical = vertical;
        packet.isGrounded = isGrounded;
        packet.isCrouching = isCrouching;
        packet.isStrafing = isStrafing;
        packet.inputMagnitude = inputMagnitude;
        packet.moveSetID = moveSetID;
        packet.verticalVelocity = verticalVelocity;

        socket.Emit("move", JsonUtility.ToJson(packet));
    }
    public void Jump(string ID, bool jumpMove)
    {
        JumpAnimationPacket packet = new JumpAnimationPacket();
        packet.id = ID;
        packet.jumpMove = jumpMove;

        socket.Emit("jump", JsonUtility.ToJson(packet));
    }
    public void SetName(string _name)
    {
        try
        {
            NamePacket packet = new NamePacket();
            packet.id = localID;
            packet.name = _name;

            socket.Emit("name", JsonUtility.ToJson(packet));
        }
        catch (Exception ex) { Debug.Log(ex.Message + " | " + ex.StackTrace); }
    }
    public void Play(int charId)
    {
        PlayPacket packet = new PlayPacket();
        packet.id = localID;
        packet.charId = charId;
        socket.Emit("spawn", JsonUtility.ToJson(packet));
        try {
            ToggleSocial(false);
            ToggleAds(false); 
        } catch (Exception ex) { 
            Debug.Log(ex.Message + " | " + ex.StackTrace); 
        }
    }
    public void TakeDamage(string id, int dmg)
    {
        if (string.IsNullOrEmpty(id))
            return;

        DamagePacket packet = new DamagePacket();
        packet.id = id;
        packet.damage = dmg;

        socket.Emit("damage", JsonUtility.ToJson(packet));
    }
    public void AttackAnimation(string id, int rndAttack, bool weakAttack, bool strongAttack, bool isBlocking, bool triggerHitReaction, int hitReaction)
    {
        AttackAnimationPacket packet = new AttackAnimationPacket();
        packet.id = id;
        if (rndAttack != -1)
            packet.randomAttack = rndAttack;
        packet.weakAttack = weakAttack;
        packet.strongAttack = strongAttack;
        packet.isBlocking = isBlocking;
        packet.triggerHitReaction = triggerHitReaction;
        packet.hitReaction = hitReaction;

        socket.Emit("attack_animation", JsonUtility.ToJson(packet));
    }
    public void ResetAnimation(string id, bool reset)
    {
        AnimationResetPacket packet = new AnimationResetPacket();
        packet.id = id;
        packet.resetState = reset;

        socket.Emit("reset_animation", JsonUtility.ToJson(packet));
    }
    public void RespawnRequestAnswer(bool accept)
    {
        RespawnPacketRequest packet = new RespawnPacketRequest();
        packet.id = localID;
        packet.accept = accept;
        socket.Emit("respawn1", JsonUtility.ToJson(packet));

        if (!accept) {
            try {
                ToggleSocial(true);
                ToggleAds(true); 
            } catch (Exception ex) { 
                Debug.Log(ex.Message + " | " + ex.StackTrace); 
            }
        }        
    }
    public void SendChatMessage(string msg)
    {
        ChatPacket packet = new ChatPacket();
        packet.sender = localPlayer.GetComponent<Player>().charName;
        packet.msg = msg;

        if (string.IsNullOrEmpty(packet.sender) || string.IsNullOrEmpty(packet.msg))
            return;

        socket.Emit("chat", JsonUtility.ToJson(packet));
    }
    public void RegisterAsBot()
    {
        if (!isBotClient || _bot == null) {
            return;
        }

        BotRegistration packet = new BotRegistration();
        packet.id = localID;

        socket.Emit("bot_registration", JsonUtility.ToJson(packet));

        SetName(_bot._name);
        MenuManager.getInstance.Change(screen.CharacterSelection);

        Play((int)_bot.faction);
    }

    IEnumerator ResetAnimations(string id)
    {
        yield return new WaitForSeconds(0.15f);
        Debug.Log("Reseting attack animations for player with id: " + id);
        players[id].GetComponent<vMeleeCombatInput>().ResetAttackTriggers();
    }
    IEnumerator RespawnPlayer(RespawnPacket packet)
    {
        yield return new WaitForSeconds(3f);

        if (packet.isBot)
        {
            string id = packet.id;
            string _name = packet.name;
            bool isBot = packet.isBot;
            Vector3 position = new Vector3(packet.posX, packet.posY, packet.posZ);
            Quaternion rotation = Quaternion.identity;
            int charId = packet.charId;
            float maxHealth = packet.maxHealth;
            float currentHealth = packet.currentHealth;

            if (maxHealth != 100)
                maxHealth = 100;
            if (currentHealth != 100)
                currentHealth = 100;

            Debug.Log("Respawning: " + (PlayerFaction)charId + " is bot: " + isBot + " at: " + position);
            GameObject playerPrefab = null;
            playerPrefab = warriorAIPrefab;
            if ((PlayerFaction)charId == PlayerFaction.Orc)
                playerPrefab = orcAIPrefab;

            GameObject player = Instantiate(playerPrefab, position, rotation);

            if (players.ContainsKey(id))
            {
                Destroy(players[id]);
                players.Remove(id);
            }

            AI p = player.GetComponent<AI>();
            if (id == localID)
                localPlayer = player;
            
            p.SetUpAI(id, charId, _name, maxHealth, currentHealth);
            players.Add(id, p);
        }
        else
        {
            string id = packet.id;
            Vector3 pos = new Vector3(packet.posX, packet.posY, packet.posZ);
            float ch = packet.currentHealth;

            players[id].GetComponent<vThirdPersonController>().currentHealth = 100;

            players[id].gameObject.SetActive(true);
            players[id].isDead = false;
            players[id].transform.position = pos;
            players[id].transform.rotation = Quaternion.identity;
            players[id].SetHealth(ch);
            try {
                ToggleAds(false); 
            } catch (Exception ex) { 
                Debug.Log(ex.Message + " | " + ex.StackTrace); 
            }
        }
    }

    #region AI
    public void OnAttack(SocketIOEvent obj)
    {
        AIShootPacket packet = JsonUtility.FromJson<AIShootPacket>(obj.data);
        if (players.ContainsKey(packet.id))
            players[packet.id].GetComponent<EnemyAI>().shoot();
    }
    public void OnPlayAnimation(SocketIOEvent obj)
    {
        AIPlayAnimation packet = JsonUtility.FromJson<AIPlayAnimation>(obj.data);
        if (players.ContainsKey(packet.id))
            players[packet.id].GetComponent<EnemyAI>().playAnimation(packet.clip);
    }
    public void OnPlayAnimations(SocketIOEvent obj)
    {
        AIPlayAnimations packet = JsonUtility.FromJson<AIPlayAnimations>(obj.data);
        if (players.ContainsKey(packet.id))
            players[packet.id].GetComponent<EnemyAI>().playAnimation(packet.clips);
    }
    public void Attack()
    {
        AIShootPacket packet = new AIShootPacket();
        packet.id = localID;

        socket.Emit("ai_attack", JsonUtility.ToJson(packet));
    }
    public void PlayAnimation(UnityEngine.Object clip)
    {
        AIPlayAnimation packet = new AIPlayAnimation();
        packet.id = localID;
        packet.clip = clip;

        socket.Emit("ai_anim", JsonUtility.ToJson(packet));
    }
    public void PlayAnimations(IList<AnimationClip> clips)
    {
        AIPlayAnimations packet = new AIPlayAnimations();
        packet.id = localID;
        packet.clips = clips;

        socket.Emit("ai_anims", JsonUtility.ToJson(packet));
    }

    public void OnAIPlayAnimation(SocketIOEvent obj)
    {
        AIAnimation packet = JsonUtility.FromJson<AIAnimation>(obj.data);
        if (packet.anim == AIAnimation.Animation.none)
            return;

        if (players.ContainsKey(packet.id))
        {
            EnemyAI ai = players[packet.id].GetComponentInChildren<EnemyAI>();
            if (ai == null)
                return;

            switch (packet.anim)
            {
                case AIAnimation.Animation.idle:
                    ai.PlayIdleAnim();
                    break;
                case AIAnimation.Animation.walk:
                    ai.PlayWalkAnim();
                    break;
                case AIAnimation.Animation.run:
                    ai.PlayRunAnim();
                    break;
                case AIAnimation.Animation.aim:
                    ai.PlayAimAnim();
                    break;
                case AIAnimation.Animation.shoot:
                    ai.PlayShootAnim();
                    break;
                case AIAnimation.Animation.die:
                    ai.PlayDeathAnim();
                    break;
            }
        }
    }
    public void AIPlayAnimation(AIAnimation.Animation anim)
    {
        if (anim == AIAnimation.Animation.none)
            return;

        AIAnimation packet = new AIAnimation();
        packet.id = localID;

        packet.anim = anim;
        socket.Emit("ai_animation", JsonUtility.ToJson(packet));
    }
    IEnumerator removeAI(string id)
    {
        yield return new WaitForSeconds(2f);
        Destroy(players[id].gameObject);
        players.Remove(id);
        if (isLocalPlayer(id))
            RespawnRequestAnswer(true);
    }
    #endregion
}