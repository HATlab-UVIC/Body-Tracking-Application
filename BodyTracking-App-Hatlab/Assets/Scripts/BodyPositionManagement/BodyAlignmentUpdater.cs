using UnityEngine;

public class BodyAlignmentUpdater : MonoBehaviour
{
    public void MoveBodyAlignment(int direction)
    {
        Vector3 _alignment = TCPStreamCoordinateHandler.BodyAlignment_Position;
        switch (direction)
        {
            case 0: //Right
                _alignment.x += 0.1f;
                break;

            case 1: //Left
                _alignment.x -= 0.1f;
                break;

            case 2: //Down
                _alignment.y += 0.1f;
                break;

            case 3: //Up
                _alignment.y -= 0.1f;
                break;

            case 4: //In
                _alignment.z += 0.1f;
                break;

            case 5: //Out
                _alignment.z -= 0.1f;
                break;
        }

        TCPStreamCoordinateHandler.BodyAlignment_Position = _alignment;
    }
}
