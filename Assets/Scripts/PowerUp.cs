using System;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Power
    {
        Shield,
        Speed,
        TripleShot
    }

    private const float _speed = 3f; 
    private const int _pointsForCollectingPowerUp = 5;

    public Power Type;

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);

        if (transform.position.y < Player.LowerBoundary - SpawnManager.PowerUpsSize)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<Player>();
        if (player == null)
            return;
        switch (Type)
        {
            case Power.Shield:
                player.ActivateShield();
                break;
            case Power.TripleShot:
                player.ActivateTripleShot();
                break;
            case Power.Speed:
                player.ActivateSpeedBoost();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        player.AddToScore(_pointsForCollectingPowerUp);
        Destroy(gameObject);
    }
}