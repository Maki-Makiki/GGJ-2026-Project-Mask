using System;
using UnityEngine;

namespace Code.Character
{
    public class Enemy : MonoBehaviour
    {
        public LayerMask playerDamage;


        private void OnTriggerEnter2D(Collider2D other)
        {
            print("ASKJDBASJKDBKJASDBKJASDBK");
            if ((playerDamage & (1 << other.gameObject.layer)) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
