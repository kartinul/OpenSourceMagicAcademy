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

    if (playerNameText != null)
      playerNameText.text = playerName;

    if (uuidText != null)
      uuidText.text = uuid;

    GameObject player = GameObject.FindWithTag("Player");
    if (player != null)
      Destroy(player);
  }
}
