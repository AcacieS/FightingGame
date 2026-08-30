using UnityEngine;

//Animation events fire on the GameObject holding the animator (Player Img)
// so this relay forwards them to the Player on the root.
public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private Player player;

    private void Awake()
    {
        if (player == null)
            player = GetComponentInParent<Player>();
    }

    public void OnBottleRelease()
    {
        player.ReleaseBottle();
    }

}
