using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventLifter : MonoBehaviour {
  CharacterController2D parentCharacterController;

  void Start() {
    this.parentCharacterController = this.GetComponentInParent<CharacterController2D>();
  }

  void EndRoll() {
    this.parentCharacterController.EndRoll();
  }

  void SpawnWallSlideDust() {
    this.parentCharacterController.SpawnWallSlideDust();
  }
}
