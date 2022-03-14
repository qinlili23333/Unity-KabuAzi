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
    public string streaming_url;

    // 检测周期
    public float test_time;

    // 历时
    private float past_time = 0;

    // 图片显示
    private SpriteRenderer sprite_renderer;

    // 按钮
    public GameObject ButtonOpenStreaming;
    private Button button_open_streaming;

    // 标志是否在直播
    private bool is_streaming = false;

    /// <summary>
    /// 以下是函数
    /// </summary>

    // Start is called before the first frame update
    void Start()
    {
        //网页初始化顺序似乎有变化，滞后处理

    }

    // Update is called once per frame
    void Update()
    {
        if (!sprite_renderer)
        {
            // 图片显示组件
            sprite_renderer = this.GetComponent<SpriteRenderer>();

            // 启动时先默认开启
            sprite_renderer.enabled = true;
        }

        if (!button_open_streaming)
        {
            // 打开网页的按钮组件
            button_open_streaming = ButtonOpenStreaming.GetComponent<Button>();

            button_open_streaming.enabled = true;
        }

        past_time += Time.deltaTime;

        // 周期性检测是否在直播
        if (past_time > test_time)
        {
            StartCoroutine(IsStreaming());
            past_time = 0;
        }
        if (is_streaming)
        {
            sprite_renderer.enabled = true;

            button_open_streaming.enabled = true;
        }
        else
        {
            sprite_renderer.enabled = false;

            button_open_streaming.enabled = false;
        }
    }

    // 检测是否在直播
    IEnumerator IsStreaming()
    {
        // API
        //Uri uri = new Uri(
        //    "http://api.live.bilibili.com/bili/living_v2/" + mid + "?callback=liveXhrDone"
        //);
        //网页版有CORS，自己搞了个反代
        Uri uri = new Uri("https://bililive.qinlili.workers.dev/status?uid=" + mid);

        //UnityWebRequest重写，解决网页支持问题
        var request = UnityWebRequest.Get(uri);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        Debug.Log(request.downloadHandler.text);
        // 网页源代码写入strHTML
        string strHTML = request.downloadHandler.text;

        // 判断是否在直播
        is_streaming = Regex.IsMatch(strHTML, "\"status\":1");
    }

    // 打开直播网页
    public void Open_streaming_web()
    {
        Application.OpenURL(streaming_url);
    }
}
