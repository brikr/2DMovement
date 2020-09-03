using UnityEngine;

public struct CollisionState {
  public bool top { get; set; }
  public bool bottom { get; set; }
  public bool front { get; set; }
  public bool back { get; set; }

  public string toString() {
    return top + "," + bottom + "," + front + "," + back;
  }
}
