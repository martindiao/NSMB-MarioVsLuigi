using System;
using UnityEngine;

using Fusion;

public abstract class BasicEntity : NetworkBehaviour, IBlockBumpable {

    //---Networked Variables
    private NetworkBool facingRightDefault = false;
    [Networked(Default = nameof(facingRightDefault))] public NetworkBool FacingRight { get; set; }

    //---Components
    [NonSerialized] public Rigidbody2D body;
    [NonSerialized] public AudioSource sfx;

    public virtual void Awake() {
        body = GetComponent<Rigidbody2D>();
        sfx = GetComponent<AudioSource>();
    }

    public void PlaySound(Enums.Sounds sound, CharacterData character = null, byte variant = 0, float volume = 1f) {
        sfx.PlayOneShot(sound.GetClip(character, variant), volume);
    }

    public abstract void Bump(BasicEntity bumper, Vector3Int tile, InteractableTile.InteractionDirection direction);
}