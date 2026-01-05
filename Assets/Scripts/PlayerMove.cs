using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;          
    public float jumpForce = 5f;      
    private Rigidbody rb;             
    private bool isGrounded;          
    [SerializeField] float moveHorizontal;
    [SerializeField] float moveVertical;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
         moveHorizontal = Input.GetAxis("Horizontal");
         moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(-moveHorizontal, 0f, -moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);


        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
