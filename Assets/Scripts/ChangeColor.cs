using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MultiPlayerProject
{
    public class ChangeColor : MonoBehaviour
    {
        [SerializeField] Material[] mats;

        private void Start()
        {
            var x = Random.Range(0, 5);

            GetComponent<Renderer>().material = mats[x];
        }
    }
}
