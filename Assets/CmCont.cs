using UnityEngine;

public class CmCont : MonoBehaviour
{
    private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smooth = 10f;
    private Transform local;


    private void Start()
    {
        local = transform;
        target = GameObject.FindWithTag("Player").transform;

    }

    private void FixedUpdate()
    {
        if (target == null) return;
        local.position = Vector3.Lerp(local.position, target.position + offset, Time.deltaTime * smooth);
       
    }
}
