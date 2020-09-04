using UnityEngine;

public struct MovementState {
  public MovementState(bool isGrounded, bool isOnWall, bool jumped, bool rolled, Vector2 velocity) {
    this.isGrounded = isGrounded;
    this.isOnWall = isOnWall;
    this.jumped = jumped;
    this.rolled = rolled;
    this.velocity = velocity;
  }

  public bool isGrounded { get; }
  public bool isOnWall { get; }
  public bool jumped { get; }
  public bool rolled { get; }
  public Vector2 velocity { get; }
}
