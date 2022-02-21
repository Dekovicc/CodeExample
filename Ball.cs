using GameData;
using Managers;
using UnityEngine;
using Utils;

namespace Gameplay
{
    public class Ball : MonoBehaviour
    {
        [Header("Collision")] 
        [SerializeField]private BallData ballData;

        private AudioSource source;
        private Rigidbody2D rb;
        private bool isGhost;

        /*Init*/
        public void Init(Vector3 velocity, bool ghost)
        {
            //Trajectory
            isGhost = ghost;
            GetComponent<Rigidbody2D>().AddForce(velocity, ForceMode2D.Impulse);
            
            //Subtract total ball amount
            if (!isGhost)
            {
                LevelManager.Instance.RemoveBall();
            }
        }

        private void Awake()
        {
            //Caching
            source = GetComponent<AudioSource>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if(isGhost)
                return;
            
            /*If we enter finish trigger change game state*/
            if (col.CompareTag("Finish"))
            {
                GameManager.Instance.UpdateGameState(GameManager.GameState.LevelWon);
            }

            /*Destroy ball if we trigger col with tag "Death"*/
            if (col.CompareTag("Death"))
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if(isGhost)
                return;
            
            if (col.collider.CompareTag("Obstacle"))
            {
                //Sound Playing
                source.RandomPitch(false);   //Extension function; see Utils.cs
                source.volume = Mathf.InverseLerp(0, 5, rb.velocity.y); //Normalise value between 0,1
                source.PlayOneShot(ballData.popSound); //Play once
            }
        }
    }
}