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

    // Start is called before the first frame update
    void Start()
    {
        // 图片显示组件
        spriteRenderer =
            this.GetComponent<SpriteRenderer>();

        // 打开网页的按钮组件
        buttonOpenStreaming =
            ButtonOpenStreaming.GetComponent<Button>();

        // 正在直播时，启用图片和按钮
        if (checkIsStreaming())
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

    // Update is called once per frame
    void Update()
    {
        pastedTime += Time.deltaTime;

        // 周期性检测是否在直播
        if (pastedTime > testTime)
        {
            if (checkIsStreaming())
            {
                spriteRenderer.enabled = true;

                buttonOpenStreaming.enabled = true;
            }
            else
            {
                spriteRenderer.enabled = false;

                buttonOpenStreaming.enabled = false;
            }

            pastedTime = 0;
        }
    }

    // 检测是否在直播
    private bool checkIsStreaming()
    {
        try
        {
            // API
            Uri uri = new Uri(
                "http://api.live.bilibili.com/bili/living_v2/" +
                mid + "?callback=liveXhrDone"
                );

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(uri);
            myReq.UserAgent =
                "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36";
            myReq.Method = "GET";

            HttpWebResponse result = (HttpWebResponse)myReq.GetResponse();
            Stream receviceStream = result.GetResponseStream();
            StreamReader readerOfStream = new StreamReader(receviceStream, System.Text.Encoding.GetEncoding("utf-8"));

            string strHTML = readerOfStream.ReadToEnd();

            readerOfStream.Close();
            receviceStream.Close();
            result.Close();

            // 判断是否在直播
            isStreaming = Regex.IsMatch(strHTML, "\"status\":1");
        }
        catch
        {
            isStreaming = false;
        }

        return isStreaming;
    }

    // 打开直播网页
    public void openStreamingUrl()
    {
        Application.OpenURL(streamingUrl);
    }
}
