using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform enemy;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera CinemachineCamera;
    [SerializeField] private CinemachineConfiner2D confiner;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomPadding = 2f;

    [Header("Smoothness")]
    [SerializeField] private float moveSmoothness = 5f;
    [SerializeField] private float zoomSmoothness = 5f;

    private void Awake()
    {
        // If this script is on the Cinemachine Camera,
        // this will automatically find it.
        if (CinemachineCamera == null)
            CinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void LateUpdate()
    {
        if (player == null || enemy == null || CinemachineCamera == null)
            return;

        // =====================================
        // FIND MIDDLE BETWEEN PLAYER AND ENEMY
        // =====================================

        Vector3 middlePoint = (player.position + enemy.position) / 2f;

        // Move the CAMERA TARGET, not the Cinemachine Camera
        Vector3 targetPosition = new Vector3(
            middlePoint.x,
            middlePoint.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSmoothness * Time.deltaTime
        );

        // =====================================
        // ZOOM
        // =====================================

        float distance = Mathf.Abs(
            player.position.x - enemy.position.x
        );

        // Further apart = zoom out
        float targetZoom = minZoom + distance * 0.5f;

        targetZoom += zoomPadding;

        targetZoom = Mathf.Clamp(
            targetZoom,
            minZoom,
            maxZoom
        );

        // Smooth zoom
        CinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
            CinemachineCamera.Lens.OrthographicSize,
            targetZoom,
            zoomSmoothness * Time.deltaTime
        );

        if (confiner != null)
        {
            confiner.InvalidateLensCache();
        }
    }
}