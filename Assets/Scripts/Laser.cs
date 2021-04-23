using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private Player _player;

    private void Start()
    {
        _player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        Movement();
        _speed = 10f + Mathf.Floor(_player.Score / 100f);
    }

    private void Movement()
    {
        transform.Translate(Vector3.up * Time.deltaTime * _speed);
        if (transform.position.y < Player.UpperBoundary + 2f)
            return;

        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        Destroy(gameObject);
    }
}