using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    void Start()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();
        if (vp != null)
        {
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "menu_video.mp4");
            vp.url = videoPath;
            vp.Play();
        }
    }
}
