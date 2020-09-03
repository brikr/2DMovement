﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour {
  public float jumpForce = 300f;
  public float runSpeed = 200f;
  public float rollSpeedMultiplier = 2f;
  public float rollThreshold = 3.5f;
  public float acceleration = 12f;
  [Range(0, 1)]
  public float airAccelerationMultiplier = 0f;
  [Range(0, 0.3f)] [SerializeField] private float movementSmoothing = .05f;    // How much to smooth out the movement
  private Vector3 smoothVelocity = Vector3.zero;

  private Rigidbody2D rb;

  public GameObject[] groundCheckObjects;
  private List<ColliderCheck> groundChecks = new List<ColliderCheck>();
  private bool isGrounded {
    get {
      foreach (ColliderCheck check in this.groundChecks) {
        if (check.State()) {
          return true;
        }
      }
      return false;
    }
  }

  private bool isRolling = false;
  private bool canRoll = true;

  private bool facingRight = true;

  private ContactPoint2D[] contactPoints = new ContactPoint2D[10];

  void Awake() {
    foreach (GameObject groundCheckObject in this.groundCheckObjects) {
      ColliderCheck check = groundCheckObject.GetComponent<ColliderCheck>();
      if (check != null) {
        this.groundChecks.Add(check);
      }
    }

    this.rb = this.GetComponent<Rigidbody2D>();
  }

  public MovementState Move(float horizontalMove, bool jumpRequested, bool rollRequested) {
    bool isGrounded = this.isGrounded;
    float direction = this.facingRight ? 1f : -1f;
    bool jumped = false;
    bool rolled = false;
    float accel = this.acceleration;
    if (!isGrounded) {
      accel *= this.airAccelerationMultiplier;
    }

    // Prevents the player from sliding down slopes they are standing still on
    if (isGrounded) {
      this.rb.gravityScale = 0f;
    } else {
      this.rb.gravityScale = 1f;
    }

    if (jumpRequested && isGrounded) {
      // If jump was requested and we are on the ground
      this.rb.AddForce(new Vector2(0f, this.jumpForce));
      jumped = true;
    } else if (rollRequested && !this.isRolling && this.canRoll && isGrounded) {
      // If a roll was requested &&
      // we aren't rolling &&
      // we can roll &&
      // we are on the ground
      if ((this.facingRight && this.rb.velocity.x > this.rollThreshold) ||
          (!this.facingRight && this.rb.velocity.x < -this.rollThreshold)) {
        // If our speed is high enough to roll and in the correct direction
        this.isRolling = true;
        rolled = true;
        // EndRoll is also called when the rolling animation finishes; this is here as a backup in case rolling is
        // interrupted by, for example, falling off a ledge
        Invoke("EndRoll", 0.6f);
        // Prevent rolling for 1 second
        this.canRoll = false;
        Invoke("ResetCanRoll", 1f);
      }
    }

    float moveAmount = 0f;
    if (this.isRolling && isGrounded) {
      // If we are currently rolling and still on the ground, don't take horizontal movement input into account
      // Instead, our target velocity is 1 * runSpeed * rollSpeedMultiplier * fixedDeltaTime
      // The 1 representing a full horizontal movement input
      moveAmount = this.runSpeed * this.rollSpeedMultiplier * direction * Time.fixedDeltaTime;

      // If the moveAmount is less than our current speed, set it to our current speed i.e. don't slow down while rolling
      if (Mathf.Abs(moveAmount) < Mathf.Abs(this.rb.velocity.x)) {
        moveAmount = this.rb.velocity.x;
      }
    } else {
      // If we're not rolling, then move amount is simply horizontal movement input * runSpeed

      if (!isGrounded) {
        // If we're in the air, only consider movements that would move us towards the other direction, or faster in our
        // current direction
        if (horizontalMove < 0 && this.facingRight ||
            horizontalMove > 0 && !this.facingRight ||
            this.facingRight && horizontalMove > this.rb.velocity.x / runSpeed ||
            !this.facingRight && horizontalMove < this.rb.velocity.x / runSpeed) {
          moveAmount = horizontalMove * this.runSpeed;
        } else {
          // Otherwise, make sure the player doesn't slow down in the air
          accel = 0;
        }
      } else {
        moveAmount = horizontalMove * this.runSpeed;
      }
    }

    // Configure our target velocity based on our moveAmount
    Vector3 targetVelocity = new Vector2(moveAmount, this.rb.velocity.y);

    if (horizontalMove > 0 && !this.facingRight) {
      // If the input is right and player is facing left
      Flip();
    } else if (horizontalMove < 0 && this.facingRight) {
      // If the input is left and the player is facing right
      Flip();
    }

    // Move us towards target velocity using appropriate acceleration
    this.rb.velocity = Vector3.MoveTowards(rb.velocity, targetVelocity, accel * Time.fixedDeltaTime);

    return new MovementState(isGrounded, jumped, rolled, this.rb.velocity);
  }

  private void Flip() {
    if (this.isRolling) {
      // Don't flip while rolling
      return;
    }

    this.facingRight = !this.facingRight;

    this.transform.Rotate(0f, 180f, 0f);
  }

  public void EndRoll() {
    this.isRolling = false;
  }

  private void ResetCanRoll() {
    this.canRoll = true;
  }

  void OnCollisionEnter2D(Collision2D collision) {
    UpdateCollisionInfo(collision);
  }

  void OnCollisionStay2D(Collision2D collision) {
    UpdateCollisionInfo(collision);
  }

  void OnCollisionExit2D(Collision2D collision) {
    UpdateCollisionInfo(null);
  }

  private CollisionState collisionState;
  private int contactCount;
  [SerializeField]
  private float edgeThreshold = 0.1f;

  void UpdateCollisionInfo(Collision2D collision) {
    this.collisionState.top = false;
    this.collisionState.bottom = false;
    this.collisionState.front = false;
    this.collisionState.back = false;

    if (collision == null) {
      this.contactCount = 0;
      return;
    }

    Array.Clear(this.contactPoints, 0, this.contactPoints.Length);
    this.contactCount = collision.GetContacts(this.contactPoints);
    if (this.contactCount > this.contactPoints.Length) {
      Debug.Log("More contacts than expected! Bump the number up above " + contactCount);
    }

    if (this.contactCount == 0) {
      return;
    }

    // NOTE: If we start using multiple colliders, this code will not be happy
    Collider2D me = this.contactPoints[0].otherCollider;

    Vector2 horizontalEdgeSize = new Vector2(me.bounds.size.x + this.edgeThreshold, this.edgeThreshold);
    Vector2 verticalEdgeSize = new Vector2(this.edgeThreshold, me.bounds.size.y + this.edgeThreshold);

    Bounds top = new Bounds(new Vector2(me.bounds.center.x, me.bounds.center.y + me.bounds.extents.y), horizontalEdgeSize);
    Bounds bottom = new Bounds(new Vector2(me.bounds.center.x, me.bounds.center.y - me.bounds.extents.y), horizontalEdgeSize);
    Bounds left = new Bounds(new Vector2(me.bounds.center.x - me.bounds.extents.x, me.bounds.center.y), verticalEdgeSize);
    Bounds right = new Bounds(new Vector2(me.bounds.center.x + me.bounds.extents.x, me.bounds.center.y), verticalEdgeSize);
    Bounds front = this.facingRight ? right : left;
    Bounds back = this.facingRight ? left : right;

    for (int i = 0; i < this.contactCount; i++) {
      if (top.Contains(this.contactPoints[i].point)) {
        this.collisionState.top = true;
      }

      if (bottom.Contains(this.contactPoints[i].point)) {
        this.collisionState.bottom = true;
      }

      if (front.Contains(this.contactPoints[i].point)) {
        this.collisionState.front = true;
      }

      if (back.Contains(this.contactPoints[i].point)) {
        this.collisionState.back = true;
      }
    }
  }

  /*
   * Debugging / gizmos stuff
   */

  void OnDrawGizmosSelected() {
    // Draw a red sphere for each collision point
    Gizmos.color = Color.red;
    for (int i = 0; i < this.contactCount; i++) {
      ContactPoint2D contact = this.contactPoints[i];
      Gizmos.DrawSphere(contact.point, 0.1f);
    }

    // Draw a red line pointing towards each direction the collider is touching something in
    if (this.collisionState.top) {
      Gizmos.DrawLine(this.rb.position, this.rb.position + Vector2.up);
    }

    if (this.collisionState.bottom) {
      Gizmos.DrawLine(this.rb.position, this.rb.position + Vector2.down);
    }

    Vector2 front = this.facingRight ? Vector2.right : Vector2.left;
    Vector2 back = this.facingRight ? Vector2.left : Vector2.right;

    if (this.collisionState.front) {
      Gizmos.DrawLine(this.rb.position, this.rb.position + front);
    }

    if (this.collisionState.back) {
      Gizmos.DrawLine(this.rb.position, this.rb.position + back);
    }
  }
}
