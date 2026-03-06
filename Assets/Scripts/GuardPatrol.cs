// using UnityEngine;

// public class GuardPatrol : MonoBehaviour
// {
//     public Transform point1;
//     public Transform point2;
//     public float speed = 2f;

//     private Transform target;

//     void Start()
//     {
//         transform.position = point1.position;
//         target = point2;
//     }

//     void Update()
//     {
//         transform.position = Vector2.MoveTowards(
//             transform.position,
//             target.position,
//             speed * Time.deltaTime
//         );

//         if (Vector2.Distance(transform.position, target.position) < 0.1f)
//         {
//             target = target == point1 ? point2 : point1;
//         }
//     }
// }


using UnityEngine;

public class GuardPatrol : MonoBehaviour
{
    bool frozen = false;

    public Transform point1;
    public Transform point2;
    public float speed = 2f;

    private Transform target;

    void Start()
    {
        transform.position = point1.position;
        target = point2;
    }

    void Update()
    {
        if (frozen) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target = target == point1 ? point2 : point1;
        }
    }

    public void Freeze(bool state)
    {
        frozen = state;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (frozen) return;

        if (other.CompareTag("Player"))
        {
            // respawn player logic here
        }
    }
}