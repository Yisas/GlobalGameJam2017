using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour {
    public static PlayerHealth Instance;
    public int maxHealthPoints = 3;
    public int currentHP = 0;
    public Text HPLabel;
    public float invulnerabilityTime = 3;
    public float currentInvulnerability;

    void Start()
    {
        Instance = this;
        currentHP = maxHealthPoints;
    }

    void Update()
    {
        currentInvulnerability -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.X))
        {
            doHarm(1);
        }
    }

    public void doHarm(int damagePoints)
    {
        currentHP -= damagePoints;
        string hpTextConstruct = string.Empty;

        for (int i = currentHP; i > 0; i--)
        {
            hpTextConstruct += "<3 ";
        }

        HPLabel.text = hpTextConstruct;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Harmer" && currentInvulnerability < 0)
        {
            Debug.Log("Harm");
            currentInvulnerability = invulnerabilityTime;
            doHarm(1);

            if (currentHP <= 0)
            {
                Debug.Log("YOU LOSE");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
