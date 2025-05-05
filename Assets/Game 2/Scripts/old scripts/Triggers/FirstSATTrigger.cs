using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstSATTrigger : MonoBehaviour
{
    [SerializeField] private GameObject triggerObject;
    private SATGunController _satGunController;

    private void Start()
    {
        _satGunController = triggerObject.gameObject.GetComponent<SATGunController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Triggered by: {other.gameObject.name} Tag: {other.gameObject.tag}");
        if (other.gameObject.CompareTag("MainCamera"))
        {
            if (_satGunController != null) {
                _satGunController.start = true;
                Debug.Log("SATGunController started via Trigger.");
            } else {
                Debug.LogError("SATGunController reference is null!");
            }
        }
    }
}
