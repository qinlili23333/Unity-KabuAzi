using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;

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
        // 图片显示组件
        sprite_renderer =
            this.GetComponent<SpriteRenderer>();

        // 打开网页的按钮组件
        button_open_streaming =
            ButtonOpenStreaming.GetComponent<Button>();

        // 正在直播时，启用图片和按钮
        if (IsStreaming())
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

    // Update is called once per frame
    void Update()
    {
        past_time += Time.deltaTime;

        // 周期性检测是否在直播
        if (past_time > test_time)
        {
            if (IsStreaming())
            {
                sprite_renderer.enabled = true;

                button_open_streaming.enabled = true;
            }
            else
            {
                sprite_renderer.enabled = false;

                button_open_streaming.enabled = false;
            }

            past_time = 0;
        }
    }

    // 检测是否在直播
    public bool IsStreaming()
    {
        // 防止用户没网而抛出异常
        try
        {
            // API
            Uri uri = new Uri(
                "http://api.live.bilibili.com/bili/living_v2/" +
                mid + "?callback=liveXhrDone"
                );

            // GET请求
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            myReq.UserAgent =
                "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36";
            myReq.Method = "GET";

            // 获取数据
            HttpWebResponse result = (HttpWebResponse)myReq.GetResponse();
            Stream receviceStream = result.GetResponseStream();
            StreamReader readerOfStream = new StreamReader(receviceStream, System.Text.Encoding.GetEncoding("utf-8"));

            // 网页源代码写入strHTML
            string strHTML = readerOfStream.ReadToEnd();

            readerOfStream.Close();
            receviceStream.Close();
            result.Close();

            // 判断是否在直播
            is_streaming = Regex.IsMatch(strHTML, "\"status\":1");
        }
        catch
        {
            is_streaming = false;
        }

        return is_streaming;
    }

    // 打开直播网页
    public void Open_streaming_web()
    {
        Application.OpenURL(streaming_url);
    }
}
