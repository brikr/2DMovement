using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
  public CharacterController2D controller;
  public Animator animator;
  // Variables we should keep up to date in animator:
  // AnimState - 1 if running, 0 if idle
  // Attack1 - Triggered when doing attack style 1
  // Attack2 - Triggered when doing attack style 2
  // Attack3 - Triggered when doing attack style 3
  // Block - Triggered when blocking
  // IdleBlock - boolean; when false, moves from IdleBlock to Idle
  // Hurt - triggered when hurt
  // Death - triggered when ded
  // noBlood - boolean for whether to show blood when dyin
  // AirSpeedY - float representing vertical air speed
  // Grounded - boolean for whether we are on the ground
  // Jump - Triggered when jumping
  // Roll - Triggered when rolling
  // WallSlide - boolean for whether we are on a wall

  private float horizontalMove = 0f;
  private bool jumpRequested = false;
  private bool rollRequested = false;

  void Start() {
    this.animator.SetBool("noBlood", false);
  }

  void Update() {
    this.horizontalMove = Input.GetAxisRaw("Horizontal");

    if (Input.GetButtonDown("Jump")) {
      this.jumpRequested = true;
    }

    if (Input.GetButton("Fire3")) {
      this.rollRequested = true;
    }
  }

  void FixedUpdate() {
    MovementState movementState = controller.Move(
      this.horizontalMove * Time.fixedDeltaTime,
      this.jumpRequested,
      this.rollRequested
    );

    // Reset requests
    this.jumpRequested = false;
    this.rollRequested = false;

    // Update animator based on movement feedback
    this.animator.SetBool("Grounded", movementState.isGrounded);

    this.animator.SetBool("WallSlide", movementState.isOnWall);

    if (movementState.jumped) {
      this.animator.SetTrigger("Jump");
    }

    if (movementState.rolled) {
      this.animator.SetTrigger("Roll");
    }

    if (Mathf.Abs(this.horizontalMove) > 0.1f) {
      this.animator.SetInteger("AnimState", 1); // running
    } else {
      this.animator.SetInteger("AnimState", 0); // idle
    }

    this.animator.SetFloat("AirSpeedY", movementState.velocity.y);
  }
}
