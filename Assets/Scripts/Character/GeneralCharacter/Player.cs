using UnityEngine;

public class Player : Character
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //TODO: Do the input that it should be for Movement
    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        Vector3 movement = Vector3.right * horizontal;

        transform.position += movement * Info.MoveSpeed * Time.deltaTime;
    }
}
