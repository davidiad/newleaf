using UnityEngine;
using UnityEngine.UI;

public class TransparentButton : MonoBehaviour {

	void Start () {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
	}
}
