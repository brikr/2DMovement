using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCheck : MonoBehaviour {
  [SerializeField]
  private int collisionCount = 0;

  public bool State() {
    return this.collisionCount > 0;
  }

  void OnTriggerEnter2D(Collider2D other) {
    this.collisionCount++;
  }

  void OnTriggerExit2D(Collider2D other) {
    this.collisionCount--;
  }
}
