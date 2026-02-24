using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] winTrigger winGate;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] TMP_Text GameOverText;
    [SerializeField] TMP_Text YouWinText;
    [SerializeField] TMP_Text GoalText;
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
      StartCoroutine(ShowGoalText());
      enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    IEnumerator ShowGoalText()
    {
        GoalText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        GoalText.gameObject.SetActive(false);
    }

    void Update()
    {
        foreach (GameObject enemy in enemies)
        {
            EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
            if (enemyMovement.gameOver&&!GameOver)
            {
                Destroy(playerMovement.gameObject);
                GameOverText.gameObject.SetActive(true);
                GameOver = true;
                playerMovement.enabled = false;
                foreach (GameObject en in enemies)
            {
                if (en.GetComponent<EnemyMovement>() != null)
                {
                    en.GetComponent<EnemyMovement>().enabled = false;
                }
                NavMeshAgent agent = en.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.enabled = false;
                }
            }

            }
        }
        if (winGate.GameWon && !GameOver)
        {
            YouWinText.gameObject.SetActive(true);
            playerMovement.enabled = false;
            Destroy(playerMovement.gameObject);
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
