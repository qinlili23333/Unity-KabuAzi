using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Networking;

/**
 * 使用B站api判断主播是否在直播
 * 直播时，显示小电视图标
 */
public class BilibiliIconController : MonoBehaviour
{
    // 所检测用户的uuid
    public string mid;

    // 所检测用户的直播间地址
    public string streamingUrl;

    // 检测周期
    public float testTime;

    // 历时
    private float pastedTime = 0;

    // 图片显示
    private SpriteRenderer spriteRenderer;

    // 按钮
    public GameObject ButtonOpenStreaming;
    private Button buttonOpenStreaming;

    // 标志是否在直播
    private bool isStreaming = false;

    void Awake()
    {
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        Debug.Log(Screen.currentResolution.refreshRate);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(checkIsStreaming());
    }

    // Update is called once per frame
    void Update()
    {
        if (!spriteRenderer)
        {
            // 图片显示组件
            spriteRenderer = this.GetComponent<SpriteRenderer>();

            // 启动时先默认开启
            spriteRenderer.enabled = true;
        }

        if (!buttonOpenStreaming)
        {
            // 打开网页的按钮组件
            buttonOpenStreaming = ButtonOpenStreaming.GetComponent<Button>();

            buttonOpenStreaming.enabled = true;
        }

        pastedTime += Time.deltaTime;

        // 周期性检测是否在直播
        if (pastedTime > testTime)
        {
            StartCoroutine(checkIsStreaming());
            pastedTime = 0;
        }
        /*  */
        if (isStreaming)
        {
            spriteRenderer.enabled = true;

            buttonOpenStreaming.enabled = true;
        }
        else
        {
            spriteRenderer.enabled = false;

            buttonOpenStreaming.enabled = false;
        }
    }

    // 检测是否在直播
    IEnumerator checkIsStreaming()
    {
        Uri uri;
        // API
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            uri = new Uri("https://bililive.qinlili.workers.dev/status?uid=" + mid);
        }
        else
        {
            uri = new Uri(
                "http://api.live.bilibili.com/bili/living_v2/" + mid + "?callback=liveXhrDone"
            );
        }

        //UnityWebRequest重写，解决网页支持问题
        var request = UnityWebRequest.Get(uri);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);
        // 网页源代码写入strHTML
        string strHTML = request.downloadHandler.text;

        // 判断是否在直播
        isStreaming = Regex.IsMatch(strHTML, "\"status\":1");
    }

    // 打开直播网页
    public void openStreamingUrl()
    {
        Application.OpenURL(streamingUrl);
    }
}
