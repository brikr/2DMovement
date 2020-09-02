using System;
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

    return new MovementState(isGrounded, jumped, rolled, rb.velocity);
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

  /*
   * Debugging / gizmos stuff
   */

  private Collision2D lastCollision = null;
  private ContactPoint2D[] lastContactPoints = new ContactPoint2D[10];

  void OnCollisionEnter2D(Collision2D collision) {
    this.lastCollision = collision;
  }

  void OnCollisionStay2D(Collision2D collision) {
    this.lastCollision = collision;
  }

  void OnCollisionExit2D(Collision2D collision) {
    this.lastCollision = null;
  }

  void OnDrawGizmosSelected() {
    if (this.lastCollision == null) {
      return;
    }

    Array.Clear(this.lastContactPoints, 0, this.lastContactPoints.Length);
    int contactCount = this.lastCollision.GetContacts(this.lastContactPoints);
    if (contactCount > this.lastContactPoints.Length) {
      Debug.Log("More contacts than expected! Bump the number up above " + contactCount);
    }

    Gizmos.color = Color.red;
    for (int i = 0; i < contactCount; i++) {
      ContactPoint2D contact = this.lastContactPoints[i];
      Gizmos.DrawSphere(contact.point, 0.1f);
    }
  }
}
