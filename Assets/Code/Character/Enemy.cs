using UnityEngine;

namespace Code.Character
{
    public class Enemy : MonoBehaviour
    {
        public LayerMask playerDamage;
        public Animator animator;

        public GameObject cabeza;
        public Rigidbody2D cabezaRB;

        public float RandomTorque = -10f;
        public float RandomTorqueMax = 10f;
        public Vector2 RandomForce = new Vector2(2f,2f);
        public Vector2 RandomForceMax = new Vector2(2f,2f);

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerDamage & (1 << other.gameObject.layer)) != 0)
            {
                Vector3 posicionActual = cabeza.transform.position;
                Quaternion rotacionActual = cabeza.transform.rotation;

                cabeza.transform.SetParent(null);
                cabeza.transform.position = posicionActual;
                cabeza.transform.rotation = rotacionActual;

                cabezaRB.bodyType = RigidbodyType2D.Dynamic;
                cabezaRB.simulated = true;

                // Ignorar colisión con el cuerpo
                Physics2D.IgnoreCollision(cabeza.GetComponent<Collider2D>(), GetComponent<Collider2D>());

                // --- Fuerza aleatoria más natural ---
                float fuerzaHorizontal = Random.Range(RandomForce.x, RandomForceMax.x); // más rango horizontal
                float fuerzaVertical = Random.Range(RandomForce.y, RandomForceMax.y);    // fuerza vertical variable
                Vector2 fuerza = new Vector2(fuerzaHorizontal, fuerzaVertical);

                cabezaRB.AddForce(fuerza, ForceMode2D.Impulse);

                // --- Torque aleatorio para que gire ---
                float torque = Random.Range(RandomTorque, RandomTorqueMax); // valores en N*m, ajustá según escala
                cabezaRB.AddTorque(torque, ForceMode2D.Impulse);

                // Ajustes de drag para rebote natural
                cabezaRB.linearDamping = 0.5f;         // frena horizontal
                cabezaRB.angularDamping = 1f;    // frena giro
                cabezaRB.gravityScale = 1.3f;   // gravedad normal

                // Animación de muerte
                animator.SetTrigger("Dead");
            }
        }
    }
}
