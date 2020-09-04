using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSlideDustDestroyer : MonoBehaviour {
  void OnDestroy() {
    Destroy(this.gameObject);
  }
}
