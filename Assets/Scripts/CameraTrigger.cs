using UnityEngine;
using Cinemachine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera followCam;
    [SerializeField] private CinemachineVirtualCamera zoomOutCam;

    void Start()
    {
        AppIntegrity.AssertPresent<CinemachineVirtualCamera>(followCam);
        AppIntegrity.AssertPresent<CinemachineVirtualCamera>(zoomOutCam);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            followCam.enabled = false;
            zoomOutCam.enabled = true;
        }
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            followCam.enabled = false;
            zoomOutCam.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            followCam.enabled = true;
            zoomOutCam.enabled = false;
        }
    }
}
