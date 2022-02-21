using System;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Playing,
            Paused,
            LevelWon,
            LevelLost
        }
        
        public static GameManager Instance;
        public static event Action<GameState> OnGameStateChanged; 

        public GameState gameState;


        private void Awake() => Instance = this;

        private void Start()
        {
            //Update Game State so we are playing
            UpdateGameState(GameState.Playing);
        }

        public void UpdateGameState(GameState newState)
        {
            //Get new state, switch to it and send event
            gameState = newState;

            switch (newState)
            {
                case GameState.Playing:
                    Debug.Log("Game is playing");
                    break;
                case GameState.Paused:
                    Debug.Log("Game is paused");
                    break;
                case GameState.LevelWon:
                    Debug.Log("Level won");
                    break;
                case GameState.LevelLost:
                    Debug.Log("Level lost");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
            //Send to subscribers
            OnGameStateChanged?.Invoke(newState);
        }
        

    }
}