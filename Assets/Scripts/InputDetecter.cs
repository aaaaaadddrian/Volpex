using UnityEngine;
using UnityEngine.InputSystem;

public class InputDetecter : MonoBehaviour
{
    
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            RaycastHit2D hit = Physics2D.Raycast(mouse, Vector2.zero);
            
            if (hit.collider != null)
            {
                HexTile tile = hit.collider.GetComponent<HexTile>();

                if (tile != null)
                {
                    tile.OnMouseDown();
                }
            }
        }
        
    }
}
