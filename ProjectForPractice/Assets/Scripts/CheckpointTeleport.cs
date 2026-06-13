using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTeleport : MonoBehaviour
{
    [Tooltip("ѕустой объект Ч точка приЄма.")]
    [SerializeField] private Transform teleportTarget;

    [Tooltip("“ег объекта, которого телепортировать. ќставьте пустым, чтобы разрешить всем.")]
    [SerializeField] private string requiredTag = "Player";

    [Tooltip("—овпадение поворота по Z при телепорте.")]
    [SerializeField] private bool matchRotation = false;

    [Tooltip("¬осстанавливать ли скорость Rigidbody2D после телепорта.")]
    [SerializeField] private bool preserveVelocity = true;

    private void Reset()
    {
        requiredTag = "Player";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryTeleport(other);
    }

    private void TryTeleport(Collider2D other)
    {
        if (teleportTarget == null) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        var rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // —охран€ем скорость и состо€ние физики
            Vector2 savedVelocity = rb.velocity;
            float savedAngular = rb.angularVelocity;
            bool wasSimulated = rb.simulated;

            // ќтключаем симул€цию, чтобы безопасно установить позицию/поворот
            rb.simulated = false;

            // ”станавливаем позицию и опционально поворот (в 2D по Z)
            rb.position = new Vector2(teleportTarget.position.x, teleportTarget.position.y);
            if (matchRotation)
                rb.rotation = teleportTarget.eulerAngles.z;

            // ¬ключаем симул€цию и восстанавливаем скорость
            rb.simulated = wasSimulated;
            if (preserveVelocity)
            {
                rb.velocity = savedVelocity;
                rb.angularVelocity = savedAngular;
            }
        }
        else
        {
            // ƒл€ объектов без Rigidbody2D Ч просто перемещаем трансформ
            other.transform.position = teleportTarget.position;
            if (matchRotation)
                other.transform.rotation = teleportTarget.rotation;
        }
    }
}
