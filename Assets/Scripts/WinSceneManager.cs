using UnityEngine;
using TMPro;
using System;

public class WinSceneManager : MonoBehaviour
{
  public TextMeshProUGUI playerNameText;
  public TextMeshProUGUI uuidText;

  void Start()
  {
    string playerName = PlayerPrefs.GetString("PlayerName", "Wizard");
    string uuid = Guid.NewGuid().ToString();
    playerNameText.text = playerName;
    uuidText.text = uuid;
    GameObject player = GameObject.FindWithTag("Player");
    Destroy(player);
  }
}
