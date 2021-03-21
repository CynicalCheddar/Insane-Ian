using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TutorialBehaviour : MonoBehaviour
{
    public KeyCode dismissKey;
    public int tutorialNumber;
    public GameObject effect;
    public bool requireDismissal = true;

    TutorialManager tutorialManager;
    private NetworkPlayerVehicle npv;

    private void Start() {
        tutorialManager = FindObjectOfType<TutorialManager>();
        npv = GetComponentInParent<NetworkPlayerVehicle>();
    }

    // Update is called once per frame
    void Update() {
        if (tutorialManager.tutorials[tutorialNumber] ) {
            effect.SetActive(true);
            if (Input.GetKeyDown(dismissKey)) {
                tutorialManager.tutorials[tutorialNumber] = false;
                Invoke(nameof(Deactivate), 1.75f);
            }
            if (!requireDismissal) {
                tutorialManager.tutorials[tutorialNumber] = false;
                Invoke(nameof(Deactivate), 6f);
            }
        } 
    }

    void Deactivate() {
        effect.SetActive(false);
    }
}
