using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject choice;

    public void OnMouseDown()
    {
        if(choice != CameraController.instance.choicePlayer)
        {
            choice = CameraController.instance.choicePlayer;
        }

        if (GameObject.Find("Players").transform.Find("Player1").gameObject && CameraController.instance.choicePlayer == CameraController.instance.players[0])
        {
            CameraController.instance.newPosition = CameraController.instance.followTransform[0].position;
        }
         if (GameObject.Find("Players").transform.Find("Player2").gameObject && CameraController.instance.choicePlayer == CameraController.instance.players[1])
        {
            CameraController.instance.newPosition = CameraController.instance.followTransform[1].position;
        }
        if (GameObject.Find("Players").transform.Find("Player3").gameObject && CameraController.instance.choicePlayer == CameraController.instance.players[2])
        {
            CameraController.instance.newPosition = CameraController.instance.followTransform[2].position;
        }
        if (GameObject.Find("Players").transform.Find("Player4").gameObject && CameraController.instance.choicePlayer == CameraController.instance.players[3])
        {
            CameraController.instance.newPosition = CameraController.instance.followTransform[3].position;
        }
        if (GameObject.Find("Players").transform.Find("Player5").gameObject && CameraController.instance.choicePlayer == CameraController.instance.players[4])
        {
            CameraController.instance.newPosition = CameraController.instance.followTransform[4].position;
        }
    }
}
