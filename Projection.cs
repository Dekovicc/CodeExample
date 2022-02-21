using Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Physics
{
    public class Projection : MonoBehaviour
    {
        
        [SerializeField] private Transform obstaclesParent;
        [SerializeField] private LineRenderer lr;
        [SerializeField] private int maxPhysicsIterations;
        
        private Scene simulationScene;
        private PhysicsScene2D physicScene;


        private void Start()
        {
            CreatePhysicScene();
        }

        void CreatePhysicScene()
        {
            //Create new scene within this scene and get physics scene from it
            simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            physicScene = simulationScene.GetPhysicsScene2D();

            //Add all objects in obstacles to sim scene and hide visibility 
            foreach (Transform _obj in obstaclesParent)
            {
                var _ghostObj = Instantiate(_obj.gameObject, _obj.position, _obj.rotation);
                _ghostObj.GetComponent<Renderer>().enabled = false;
                SceneManager.MoveGameObjectToScene(_ghostObj, simulationScene);
            }
        }

        public void SimulateTrajectory(Ball ballPrefab, Vector3 pos, Vector3 velocity)
        {
            // instantiate ghost ball and move it to sim scene
            var _ghostObj = Instantiate(ballPrefab, pos, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(_ghostObj.gameObject, simulationScene);
            
            // initialise ghost ball
            _ghostObj.Init(velocity, true);

            
            //set line renderer position to max iterations, and fill appropriate each physics step
            lr.positionCount = maxPhysicsIterations;
            for (int i = 0; i < maxPhysicsIterations; i++)
            {
                physicScene.Simulate(Time.fixedDeltaTime);
                var z0 = _ghostObj.transform.position;
                z0.z = 0;
                lr.SetPosition(i,z0);
            }
            
            Destroy(_ghostObj.gameObject);
        }

        public void StopSimulation()
        {
            lr.positionCount = 0;
        }
    }
}