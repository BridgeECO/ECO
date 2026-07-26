using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class PlayBackgroundVideo : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        InitVideoPlayer();
    }

    private void InitVideoPlayer()
    {
        if (_videoPlayer == null)
        {
            _videoPlayer = GetComponent<VideoPlayer>();
        }

        _videoPlayer.source = VideoSource.Url;
        string videoPath = Path.Combine(Application.streamingAssetsPath, "Video_TitleScene_BackGround.mp4");
        _videoPlayer.url = videoPath;
        _videoPlayer.Play();
    }
}
