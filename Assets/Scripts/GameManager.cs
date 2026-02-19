using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] winTrigger winGate;
    [SerializeField] PlayerMovement playerMovement;
    bool GameOver;
    bool restart;
    GameObject[] enemies;
    Controls controls;

    void Awake()
    {
        controls = new Controls();
        controls.Menu.Enable();
        controls.Menu.Restart.performed += Restart;
    }

    private void Restart(InputAction.CallbackContext context)
    {
        restart = context.performed;
    }

    void Start()
    {
      enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }
    void Update()
    {
        foreach (GameObject enemy in enemies)
        {
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            if (enemyMovement.gameOver&&!GameOver)
            {
                GameOver = true;
                playerMovement.enabled = false;
            }
        }
        if (winGate.GameWon && !GameOver)
        {
            playerMovement.enabled = false;
            GameOver = true;
            print("won");
            foreach (GameObject enemy in enemies)
            {
                if (enemy.GetComponent<EnemyMovement>() != null)
                {
                    enemy.GetComponent<EnemyMovement>().enabled = false;
                }
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = false;
                }

            }
        }

        if (GameOver)
        {
            if (restart) SceneManager.LoadScene(0);
        }
    }
}
