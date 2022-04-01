using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// maybe a better name for the script
public class FrozenCube : HoldableEntity {
    private static int GROUND_LAYER_ID = -1;

    public float throwSpeed = 10f;
    bool left;
    public BoxCollider2D frozenCubeCollider;
    public SpriteRenderer spriteRender;
    public Sprite[] iceSprites;

    public KillableEntity frozenEntity;
    public PlayerController frozenPlayer;

    public float fallTimer;
    float fallTimerCount;
    Vector2 blockOffset = new Vector3(0, 0.05f);

    public bool stationary, fallen;
    public bool kinematicEntity, flyingEntity, plantEntity;

    public float offset;

    // TODO: when ice collides with something while after being thrown it breaks

    new void Start() {
        base.Start();
        dead = false;
        holderOffset = new Vector2(1, 1);
        hitbox = GetComponentInChildren<BoxCollider2D>();
        dropcoin = false;
        body.velocity = new Vector2(0, 0);

        if (GROUND_LAYER_ID == -1)
            GROUND_LAYER_ID = LayerMask.NameToLayer("Ground");

    }

    private new void LateUpdate() {
        base.LateUpdate();
        if (frozenEntity && !plantEntity && !frozenPlayer) {
            frozenEntity.transform.position = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y / 2) + offset, frozenEntity.transform.position.z);
        } else if (frozenPlayer) {
            //frozenPlayer.transform.position = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y / 2) + offset, frozenPlayer.transform.position.z);
        }
    }

    void FixedUpdate() {
        if (GameManager.Instance && GameManager.Instance.gameover) {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0;
            animator.enabled = false;
            body.isKinematic = true;
            return;
        }

        body.mass = holder != null ? 0 : 1;

        if (frozenPlayer) {
            if (!frozenPlayer.frozen || frozenPlayer.frozenJump >= 3) {
                //photonView.RPC("SpecialKill", RpcTarget.All, false, false);
            }
        }

        if (frozenEntity) {
            if (plantEntity) {
                body.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            } else if (!flyingEntity || fallen) {
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            if (flyingEntity) {
                if ((fallTimerCount -= Time.fixedDeltaTime) < 0) {
                    fallen = true;
                }
            }
        } else if (!frozenPlayer) {
            PhotonNetwork.Destroy(photonView);
        }

        if (photonView && !photonView.IsMine)
            return;

        if (!dead)
            HandleTile();
    }

    public override void InteractWithPlayer(PlayerController player) {
        Vector2 damageDirection = (player.body.position - body.position).normalized;
        bool attackedFromAbove = Vector2.Dot(damageDirection, Vector2.up) > 0f;
        if (holder)
            return;
        else if (player.groundpound && player.state != Enums.PowerupState.Mini && attackedFromAbove) {
            photonView.RPC("SpecialKill", RpcTarget.All, player.body.velocity.x > 0, player.groundpound);
        } else if (Mathf.Abs(body.velocity.x) >= 2f * 1 && !physics.hitRoof) {
            player.photonView.RPC("Knockback", RpcTarget.All, body.position.x > player.body.position.x, 1, false, photonView.ViewID);
        }
        if (!holder && !dead && !plantEntity && !plantEntity) {
            if (player.state != Enums.PowerupState.Mini && !player.holding && player.running && !player.propeller && !player.flying && !player.crouching && !player.dead && !player.onLeft && !player.onRight && !player.doublejump && !player.triplejump) {
                photonView.RPC("Pickup", RpcTarget.All, player.photonView.ViewID);
                player.photonView.RPC("SetHolding", RpcTarget.All, photonView.ViewID);
            } else {
                photonView.RPC("Kick", RpcTarget.All, player.body.position.x < body.position.x, player.groundpound);
                player.photonView.RPC("SetHoldingOld", RpcTarget.All, photonView.ViewID);
                previousHolder = player;
            }
        }
    }

    [PunRPC]
    public void setFrozenEntity(string entity, int enitiyID) {
        if (entity == "Player") {
            spriteRender.sprite = iceSprites[1];
            GetComponent<BoxCollider2D>().size = new Vector2(1.35f, 2);
            frozenCubeCollider.size = new Vector2(1.35f, 2);
            frozenPlayer = PhotonView.Find(enitiyID).GetComponent<PlayerController>();
            frozenPlayer.FrozenObject = this;
            frozenPlayer.photonView.RPC("Freeze", RpcTarget.All);
        } else {
            frozenEntity = PhotonView.Find(enitiyID).GetComponent<KillableEntity>();
            frozenEntity.photonView.RPC("Freeze", RpcTarget.All);

            if (entity == "koopa") {
                spriteRender.sprite = iceSprites[1];

                if (frozenEntity.GetComponent<KoopaWalk>().shell) {
                    spriteRender.sprite = iceSprites[0];
                    GetComponent<BoxCollider2D>().size = new Vector2(1.35f, 1.35f);
                    frozenCubeCollider.size = new Vector2(1.35f, 1.35f);
                    offset = 0.25f;
                } else {
                    GetComponent<BoxCollider2D>().size = new Vector2(1.35f, 2);
                    frozenCubeCollider.size = new Vector2(1.35f, 2);
                }

            } else if (entity == "bulletbill") {
                spriteRender.sprite = iceSprites[2];
                kinematicEntity = true;
                flyingEntity = true;
                fallTimerCount = fallTimer;
                GetComponent<BoxCollider2D>().size = new Vector2(2, 1.35f);
                frozenCubeCollider.size = new Vector2(2, 1.35f);


            } else if (entity == "piranhaplant") {
                spriteRender.sprite = iceSprites[1];
                plantEntity = true;
                GetComponent<BoxCollider2D>().size = new Vector2(1.35f, 2);
                frozenCubeCollider.size = new Vector2(1.35f, 2);


            } else if (entity == "goomba" || entity == "bobomb") {
                spriteRender.sprite = iceSprites[0];

                GetComponent<BoxCollider2D>().size = new Vector2(1.35f, 1.35f);
                frozenCubeCollider.size = new Vector2(1.35f, 1.35f);
            }
        }
    }

    [PunRPC]
    public override void Kick(bool fromLeft, bool groundpound) {

    }

    [PunRPC]
    public override void Throw(bool facingLeft, bool crouch) {
        if (holder == null)
            return;

        stationary = false;
        body.position = new Vector2(holder.facingRight ? holder.transform.position.x + 0.1f : holder.transform.position.x - 0.1f, transform.position.y);

        previousHolder = holder;
        holder = null;

        photonView.TransferOwnership(PhotonNetwork.MasterClient);

        if (frozenEntity)
            frozenEntity.body.isKinematic = false;

        if (crouch) {
            body.velocity = new Vector2(2f * (facingLeft ? -1 : 1), body.velocity.y);
        } else {
            body.velocity = new Vector2(throwSpeed * (facingLeft ? -1 : 1), body.velocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if ((photonView && !photonView.IsMine) || dead)
            return;

        GameObject obj = collider.gameObject;
        KillableEntity killa = obj.GetComponentInParent<KillableEntity>();
        switch (obj.tag) {
        case "koopa":
        case "bobomb":
        case "bulletbill":
        //case "frozencube":
        case "goomba": {
            if (dead || killa.dead || killa.Equals(frozenEntity))
                break;
            killa.photonView.RPC("SpecialKill", RpcTarget.All, killa.body.position.x > body.position.x, false);
            photonView.RPC("SpecialKill", RpcTarget.All, killa.body.position.x < body.position.x, false);

            break;
        }
        case "piranhaplant": {
            if (killa.dead)
                break;
            killa.photonView.RPC("Kill", RpcTarget.All);
            if (holder)
                photonView.RPC("Kill", RpcTarget.All);

            break;
        }
        case "coin": {
            previousHolder.photonView.RPC("CollectCoin", RpcTarget.AllViaServer, obj.GetPhotonView().ViewID, new Vector3(obj.transform.position.x, collider.transform.position.y, 0));
            break;
        }
        case "loosecoin": {
            if (!holder && previousHolder) {
                Transform parent = obj.transform.parent;
                previousHolder.photonView.RPC("CollectCoin", RpcTarget.All, parent.gameObject.GetPhotonView().ViewID, parent.position);
            }
            break;
        }
        }
    }

    void HandleTile() {
        physics.UpdateCollisions();

        if ((physics.hitLeft || physics.hitRight || holder && physics.hitRoof)) {
            photonView.RPC("SpecialKill", RpcTarget.All, false, false);
        }


    }

    [PunRPC]
    public override void Freeze() {
        Debug.Log("You can't freeze a FrozenCube.");
    }

    [PunRPC]
    public override void Unfreeze() {
        Debug.Log("You can't unfreeze a frozen cube, unfreeze the entity.");
    }

    [PunRPC]
    public override void Kill() {
        if (frozenEntity) {
            frozenEntity.photonView.RPC("Unfreeze", RpcTarget.All);
            if (kinematicEntity)
                frozenEntity.body.isKinematic = true;
        } else if (frozenPlayer) {
            frozenPlayer.photonView.RPC("Unfreeze", RpcTarget.All);
            frozenPlayer.body.isKinematic = false;
        }

        if (holder)
            holder.holding = null;
        holder = null;
        frozenEntity = null;
        frozenPlayer = null;
        dead = true;
        photonView.RPC("SpecialKill", RpcTarget.All, false, false);
    }

    [PunRPC]
    public override void SpecialKill(bool right = true, bool groundpound = false) {
        base.SpecialKill(right, groundpound);
        hitbox.enabled = false;
        if (frozenEntity) {

            frozenEntity.dropcoin = false;
            frozenEntity.photonView.RPC("Unfreeze", RpcTarget.All);

            if (!plantEntity) {
                frozenEntity.photonView.RPC("SpecialKill", RpcTarget.All, right, false);
            } else {
                frozenEntity.photonView.RPC("Kill", RpcTarget.All);
                frozenEntity.dropcoin = true;
            }


            spriteRender.enabled = false;

        }

        if (frozenPlayer) {
            frozenPlayer.photonView.RPC("Unfreeze", RpcTarget.All);
        }

        if (holder)
            holder.holding = null;
        holder = null;

        Instantiate(Resources.Load("Prefabs/Particle/IceBreak"), transform.position, Quaternion.identity);
        dead = true;
    }
}
