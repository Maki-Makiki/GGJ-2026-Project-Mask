using UnityEngine;

namespace Code.Character
{
    public class Enemy : MonoBehaviour
    {
        public LayerMask playerDamage;
        public Animator animator;

        public GameObject cabeza;
        public Rigidbody2D cabezaRB;


        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((playerDamage & (1 << other.gameObject.layer)) != 0)
            {
                // 1. Guardar la posición y rotación actual en el mundo antes de soltarla
                Vector3 posicionActual = cabeza.transform.position;
                Quaternion rotacionActual = cabeza.transform.rotation;

                // 2. Desvincular completamente
                cabeza.transform.SetParent(null);

                // 3. RE-POSICIONAR manualmente justo después de soltarla 
                // (Esto evita el "salto" a coordenadas 0,0,0)
                cabeza.transform.position = posicionActual;
                cabeza.transform.rotation = rotacionActual;

                // 4. Activar físicas
                cabezaRB.bodyType = RigidbodyType2D.Dynamic;
                cabezaRB.simulated = true;

                // 5. IMPORTANTE: Ignora colisión con el cuerpo para evitar que salga volando por error
                Physics2D.IgnoreCollision(cabeza.GetComponent<Collider2D>(), GetComponent<Collider2D>());

                cabezaRB.AddForce(new Vector2(Random.Range(-2f, 2f), 5f), ForceMode2D.Impulse);

                // 6. Animación de muerte (opcional)
                animator.SetTrigger("Dead");
            }
        }
    }
}
