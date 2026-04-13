using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomTransition : MonoBehaviour
{
    [Foldout("Rooms")]
    [SerializeField]
    private Room _roomA;
    [SerializeField]
    private Room _roomB;

    private CameraRoomTransition _cameraRoomTransition;
    private float _lastTriggerTime = -1f;

    private void Start()
    {
        _cameraRoomTransition = Camera.main.GetComponent<CameraRoomTransition>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - _lastTriggerTime < 0.5f)
        {
            return;
        }

        if (other.CompareTag(nameof(ETags.Player)))
        {
            _lastTriggerTime = Time.time;

            Room targetRoom = null;
            if (RespawnManager.Instance.CurrentRoom == _roomA)
            {
                targetRoom = _roomB;
            }
            else if (RespawnManager.Instance.CurrentRoom == _roomB)
            {
                targetRoom = _roomA;
            }
            else if (RespawnManager.Instance.CurrentRoom == null)
            {
                // CurrentRoom이 아직 설정되지 않은 초기 상태일 경우 기본 타겟 설정
                targetRoom = _roomB; 
            }

            if (targetRoom != null)
            {
                _cameraRoomTransition.StartRoomTransitionAsync
                (targetRoom.MinBounds, targetRoom.MaxBounds,
                this.GetCancellationTokenOnDestroy()).Forget();

                RespawnManager.Instance.UpdateSavePoint(targetRoom);
            }
        }
    }
}