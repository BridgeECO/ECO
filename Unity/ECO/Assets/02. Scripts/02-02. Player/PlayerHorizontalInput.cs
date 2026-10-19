using UnityEngine;

// 좌우 키를 함께 누르면 Input.GetAxisRaw가 -1과 +1을 더해 0을 돌려줘 캐릭터가 멈춘다.
// 양쪽이 모두 눌린 동안에는 마지막으로 누른 방향을 우선한다. 키 목록은 InputManager의
// Horizontal 축(left/right, a/d)과 같아야 하며, 그 밖의 경우는 축 값을 그대로 써서 게임패드 입력을 유지한다.
public class PlayerHorizontalInput
{
    private const string HORIZONTAL_AXIS = "Horizontal";

    private float _lastPressedDirection;

    public float Read()
    {
        RecordPressedDirection();

        bool isLeftHeld = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        bool isRightHeld = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        if (isLeftHeld && isRightHeld)
        {
            return _lastPressedDirection;
        }
        return Input.GetAxisRaw(HORIZONTAL_AXIS);
    }

    private void RecordPressedDirection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            _lastPressedDirection = -1f;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            _lastPressedDirection = 1f;
        }
    }
}
