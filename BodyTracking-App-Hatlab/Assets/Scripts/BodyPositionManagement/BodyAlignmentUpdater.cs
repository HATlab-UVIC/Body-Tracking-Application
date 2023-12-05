using UnityEngine;

/*
Summary:
Class is used for the purpose of modifying the display position of the patient
game object. This in conjunction with the corresponding voice commands, allow
the user to move the game object in 3D space so it is positioned over their body.
*/
public class BodyAlignmentUpdater : MonoBehaviour
{
    /*
    Summary:
    Method is used in conjunction with the voice commands to move the
    patient game object.
    */
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
