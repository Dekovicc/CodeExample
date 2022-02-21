using Managers;
using Physics;
using UnityEngine;

namespace Gameplay
{
    public class BallPlacement : MonoBehaviour
    {
        [SerializeField] private Renderer spawnZone;
        
        [SerializeField] private float placementDepth;
        [SerializeField] private Ball ballPrefab;
        [SerializeField] private Projection projection;
        [SerializeField] private float force;

        private Camera cineCam;

        private void Awake() => Init();

        private void Init()
        {
            //Grab Camera refrence from assetRefrences
            cineCam = AssetRefrences.i.GetCineCam();
        }


        private void Update()
        {
            //set placement depth to correct z value
            var _pos = cineCam.ScreenToWorldPoint(Input.mousePosition);
            _pos.z = placementDepth;

            //If player dosen't have more balls, fail level
            if (LevelManager.Instance.GetBalls() <= 0)
            {
                GameManager.Instance.gameState = GameManager.GameState.LevelLost;
                return;
            }
            
            //**BALL SPAWNING**//

            //Get camera if its not avalible
            if (cineCam == null) {
                Init();
                return;
            }
            
            //If mouse is pressed and its in spawn position, call simulation trajectory
            if (Input.GetMouseButton(0))
            {
                if (spawnZone.bounds.Contains(_pos))
                {
                    //start simulation
                    projection.SimulateTrajectory(ballPrefab, _pos, Vector3.down * force); 
                }
            }
            
            //If click up and its in spawn position, stop simulation and Instantiate ball, call Init(); 
            if (Input.GetMouseButtonUp(0))
            {
                if (spawnZone.bounds.Contains(_pos))
                {
                    
                    projection.StopSimulation();
                    Instantiate(ballPrefab, _pos, Quaternion.identity);
                    ballPrefab.Init(Vector3.down * force, false);
                }
            }
        }
    } 
}
