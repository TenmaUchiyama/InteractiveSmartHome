using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

namespace SpatialLLM.Network
{
    public class HttpServer : MonoBehaviour, IDisposable
    {
        private HttpListener listener;
        private CancellationTokenSource cancellationTokenSource;

        // ルーティング用のマップ (GET, POST などのHTTPメソッドとパスのマッピング)
        private Dictionary<string, Func<HttpListenerContext, Task>> routes = new Dictionary<string, Func<HttpListenerContext, Task>>();


        HttpListenerContext context = null;

        /// <summary>
        /// サーバーを起動する
        /// </summary>
        public void InitServer(string domain = "localhost", int port = 9999)
        {
            if (listener != null && listener.IsListening) return;

            listener = new HttpListener();
            listener.Prefixes.Add($"http://{domain}:{port}/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            try
            {
                listener.Start();
                Debug.Log($"[HTTP SERVER] Server started on http://{domain}:{port}/");
                
                cancellationTokenSource = new CancellationTokenSource();
                _ = StartListener(cancellationTokenSource.Token); // Fire-and-forget (awaitしない)
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HTTP SERVER] サーバーの起動に失敗しました: {ex.Message}");
            }
        }

        void OnDestroy()
        {
            StopServer();
        }

        /// <summary>
        /// サーバーを停止する
        /// </summary>
        public void StopServer()
        {
            Debug.Log("[HTTP SERVER] サーバーを停止中...");
            
            cancellationTokenSource?.Cancel();

            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
            }

            Debug.Log("[HTTP SERVER] サーバーが正常に停止しました。");
        }

        /// <summary>
        /// クライアントからのリクエストを非同期に受け付けるメソッド
        /// </summary>
        private async Task StartListener(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    context = await listener.GetContextAsync();
                    Debug.Log($"[HTTP SERVER] Method: {context.Request.HttpMethod}, Path: {context.Request.Url.LocalPath}");

                    string routeKey = $"{context.Request.HttpMethod}:{context.Request.Url.LocalPath}";
                    if (routes.TryGetValue(routeKey, out var callback))
                    {
                        await callback(context);
                    }
                    else
                    {
                        await ReturnNotFound(context);
                    }
                }
                catch (Exception e) when (!(e is OperationCanceledException))
                {
                    Debug.LogError($"[HTTP SERVER] リクエスト処理中のエラー: {e}");

                    if (context != null)
                    {
                        await context.RespondError(500, e);
                    }
                    break;
                }
            }
        }

        public void Get(string path, Func<HttpListenerContext, Task> callback)
        {
            string routeKey = $"GET:{path}";
            routes[routeKey] = callback;
            Debug.Log($"[HTTP SERVER] GETルートを登録: {path}");
        }

        public void Post(string path, Func<HttpListenerContext, Task> callback)
        {
            string routeKey = $"POST:{path}";
            routes[routeKey] = callback;
            Debug.Log($"[HTTP SERVER] POSTルートを登録: {path}");
        }

        private async Task ReturnNotFound(HttpListenerContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "text/plain";
            using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                await writer.WriteAsync("404 Not Found");
            }
            context.Response.Close();
        }

        public void Dispose()
        {
            StopServer();
        }
    }

    public static class HttpListenerContextExtensions
    {
        /// <summary>
        /// 文字列を返す
        /// </summary>
        public static async Task Respond(this HttpListenerContext context, int statusCode, string message, string contentType = "text/plain")
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;

            using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                await writer.WriteAsync(message);
            }

            context.Response.Close();
        }

        /// <summary>
        /// JSONデータを返す
        /// </summary>
        public static async Task Respond(this HttpListenerContext context, int statusCode, object data)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            string json = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
            {
                await writer.WriteAsync(json);
            }

            context.Response.Close();
        }

        /// <summary>
        /// ファイルの内容を返す
        /// </summary>
        public static async Task RespondFile(this HttpListenerContext context, int statusCode, string filePath, string contentType = "application/octet-stream")
        {
            if (!File.Exists(filePath))
            {
                await context.Respond(404, "File not found");
                return;
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;
            context.Response.ContentLength64 = fileBytes.Length;

            await context.Response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
            context.Response.Close();
        }

        /// <summary>
        /// リクエストのボディを文字列として読み取る (非同期)
        /// </summary>
        public static async Task<string> ReadBodyAsTextAsync(this HttpListenerContext context)
        {
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// リクエストのボディをJSONオブジェクトとして解析する (非同期)
        /// </summary>
        public static async Task<T> ReadBodyAsJsonAsync<T>(this HttpListenerContext context)
        {
            string requestBody = await context.ReadBodyAsTextAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }

        /// <summary>
        /// フォームデータ (x-www-form-urlencoded) をパースする
        /// </summary>
        public static async Task<NameValueCollection> ReadFormDataAsync(this HttpListenerContext context)
        {
            string requestBody = await context.ReadBodyAsTextAsync();
                return requestBody.ParseQueryString();
}

public static NameValueCollection ParseQueryString(this string query)
{
    var result = new NameValueCollection();
    if (string.IsNullOrEmpty(query)) return result;

    string[] pairs = query.Split('&');
    foreach (var pair in pairs)
    {
        if (string.IsNullOrEmpty(pair)) continue;

        int idx = pair.IndexOf('=');
        if (idx > 0)
        {
            string key = WebUtility.UrlDecode(pair.Substring(0, idx));
            string value = WebUtility.UrlDecode(pair.Substring(idx + 1));
            result.Add(key, value);
        }
        else
        {
            string key = WebUtility.UrlDecode(pair);
            result.Add(key, "");
        }
    }

    return result;
}



    /// <summary>
    /// エラーメッセージをJSONで返す (Exceptionから生成)
    /// </summary>
    public static async Task RespondError(this HttpListenerContext context, int statusCode, Exception ex)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorData = new
        {
            status = "error",
            message = ex.Message,
            stackTrace = ex.StackTrace,
            innerException = ex.InnerException?.Message
        };

        string json = JsonConvert.SerializeObject(errorData, Formatting.Indented);
        using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
        {
            await writer.WriteAsync(json);
        }

        context.Response.Close();
    }

    /// <summary>
    /// エラーメッセージをJSONで返す (メッセージから生成)
    /// </summary>
    public static async Task RespondError(this HttpListenerContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorData = new
        {
            status = "error",
            message = message
        };

        string json = JsonConvert.SerializeObject(errorData, Formatting.Indented);
        using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8))
        {
            await writer.WriteAsync(json);
        }

        context.Response.Close();
    }


    }
}
