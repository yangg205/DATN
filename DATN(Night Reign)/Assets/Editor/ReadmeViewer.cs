using UnityEditor;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking; // Thêm thư viện này
using System.IO.Compression;

public class ReadmeViewer : EditorWindow
{
    private string readmeContent;
    private Vector2 scrollPosition;
    // Đổi tên biến cho rõ ràng hơn vì nó không còn là link Google Drive dạng thư mục nữa
    private string directDownloadLink = "";
    private string downloadFolderPath;
    private bool isDownloading = false;

    [MenuItem("Tools/Readme Viewer")]
    public static void ShowWindow()
    {
        GetWindow<ReadmeViewer>("Readme Viewer");
    }

    private void OnEnable()
    {
        downloadFolderPath = Path.Combine(Application.dataPath, "Package");
        if (!Directory.Exists(downloadFolderPath))
        {
            Directory.CreateDirectory(downloadFolderPath);
        }

        string readmePath = Path.Combine(Application.dataPath, "../../README.md");
        if (File.Exists(readmePath))
        {
            readmeContent = File.ReadAllText(readmePath);
            string[] lines = readmeContent.Split('\n');
            foreach (string line in lines)
            {
                // Tìm kiếm liên kết tải xuống trực tiếp, giả sử nó nằm trong ngoặc tròn sau một đoạn text nào đó
                // Ví dụ: "Tải gói tại đây: [Tải xuống](https://drive.google.com/uc?export=download&id=YOUR_FILE_ID)"
                // Hoặc đơn giản là dòng chỉ chứa URL
                if (line.Contains("https://"))
                {
                    directDownloadLink = ExtractDirectDownloadLink(line);
                    if (!string.IsNullOrEmpty(directDownloadLink))
                    {
                        Debug.Log($"Tìm thấy liên kết tải xuống trong README: {directDownloadLink}");
                        break;
                    }
                }
            }
        }
        else
        {
            readmeContent = "README.md not found in the project root.";
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("README.md", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(readmeContent, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (!string.IsNullOrEmpty(directDownloadLink)) // Sử dụng biến đã đổi tên
        {
            GUI.enabled = !isDownloading;
            if (GUILayout.Button("Tải và giải nén"))
            {
                DownloadAndExtractZip(directDownloadLink);
            }
            GUI.enabled = true;
        }
        else
        {
            EditorGUILayout.HelpBox("Không tìm thấy liên kết tải xuống trực tiếp (dạng ZIP) trong README.md. Vui lòng đảm bảo README chứa một liên kết tải xuống file ZIP đã được chia sẻ công khai.", MessageType.Warning);
        }
    }

    // Hàm này được tối ưu để trích xuất URL từ các định dạng Markdown link hoặc URL thuần túy
    private string ExtractDirectDownloadLink(string line)
    {
        // Thử khớp với định dạng Markdown: [text](URL)
        System.Text.RegularExpressions.Match markdownMatch = System.Text.RegularExpressions.Regex.Match(line, @"\[.*?\]\((https?:\/\/[^\s\)]+)\)");
        if (markdownMatch.Success)
        {
            return markdownMatch.Groups[1].Value;
        }

        // Nếu không phải Markdown, thử tìm URL thuần túy
        System.Text.RegularExpressions.Match urlMatch = System.Text.RegularExpressions.Regex.Match(line, @"(https?:\/\/[^\s]+)");
        if (urlMatch.Success)
        {
            return urlMatch.Groups[1].Value;
        }
        return string.Empty;
    }

    private async void DownloadAndExtractZip(string directDownloadUrl)
    {
        try
        {
            isDownloading = true;
            Debug.Log($"Bắt đầu tải file từ URL: {directDownloadUrl}");

            // Lấy tên file từ URL để đặt tên file ZIP tạm thời
            string zipFileName = Path.GetFileName(directDownloadUrl.Split('?')[0]); // Lấy phần tên file trước dấu '?'
            if (string.IsNullOrEmpty(zipFileName) || !zipFileName.EndsWith(".zip"))
            {
                zipFileName = "downloaded_package.zip"; // Tên mặc định nếu không trích xuất được
            }

            string zipFilePath = Path.Combine(downloadFolderPath, zipFileName);

            // Kiểm tra xem file đã tồn tại chưa để tránh tải lại
            if (File.Exists(zipFilePath))
            {
                Debug.Log($"File {zipFileName} đã tồn tại, xóa và tải lại.");
                File.Delete(zipFilePath);
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get(directDownloadUrl))
            {
                // Thêm header để tránh Google chặn tải xuống (nếu có thể)
                webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36");
                webRequest.SetRequestHeader("Accept", "*/*");


                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                {
                    EditorUtility.DisplayProgressBar("Đang tải file", $"Tiến độ: {webRequest.downloadProgress * 100:F2}%", webRequest.downloadProgress);
                    await Task.Yield(); // Cho phép Unity cập nhật UI
                }

                EditorUtility.ClearProgressBar();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    throw new System.Exception($"Lỗi khi tải file: {webRequest.error}");
                }
                else
                {
                    // Lấy dữ liệu và ghi vào file
                    byte[] fileData = webRequest.downloadHandler.data;
                    File.WriteAllBytes(zipFilePath, fileData);
                    Debug.Log($"Đã tải file thành công về: {zipFilePath}");
                }
            }

            ExtractZipFile(zipFilePath, downloadFolderPath);
            File.Delete(zipFilePath);
            Debug.Log($"Đã giải nén và xóa file ZIP: {zipFilePath}");

            isDownloading = false;
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            isDownloading = false;
            EditorUtility.ClearProgressBar();
            Debug.LogError($"Lỗi khi tải hoặc giải nén: {ex.Message}");
        }
    }

    private void ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            if (!File.Exists(zipPath))
            {
                throw new System.Exception("File ZIP không tồn tại.");
            }
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            Debug.Log($"Đã giải nén file ZIP đến: {extractPath}");
        }
        catch (System.IO.InvalidDataException)
        {
            Debug.LogError("File tải về không phải là file ZIP hợp lệ. Vui lòng kiểm tra lại nguồn tải xuống.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lỗi khi giải nén file ZIP: {ex.Message}");
        }
    }
}