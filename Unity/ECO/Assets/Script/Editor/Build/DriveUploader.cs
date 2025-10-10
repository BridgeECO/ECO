#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using DriveFile = Google.Apis.Drive.v3.Data.File;

using ECO;

public static class DriveUploaderOnly
{
    private const string kTokenCacheDir = "Assets/Editor/.gdrive_token";
    private const string kCredPathPrefKey = "DriveUploader.ClientSecretPath";
    private const string kFolderIdPrefKey = "DriveUploader.FolderId";
    private const string kDefaultFolderId = "19C_-spQCjMHUcP6FGzDT15R16ugVQksi";

    [MenuItem("/ECO/Build/Upload/Set Google Drive Folder ID")]
    public static void SetFolderId()
    {
        var current = EditorPrefs.GetString(kFolderIdPrefKey, kDefaultFolderId);
        var input = EditorUtility.DisplayDialogComplex("Folder ID 설정", "현재: " + current, "변경", "지우기", "닫기");
        if (input == 0)
        {
            var id = EditorUtility.DisplayDialog("안내", "폴더 주소의 folders/ 뒤 문자열을 입력하세요.", "확인")
                ? "" : "";
            // 간단 입력창이 없어 Project Settings 등에서 관리하시거나, 필요 시 별도 EditorWindow 구현 권장
        }
        else if (input == 1)
        {
            EditorPrefs.DeleteKey(kFolderIdPrefKey);
            LOG.Info("Folder ID 삭제");
        }
    }

    [MenuItem("/ECO/Build/Upload/Upload File")]
    public static async void UploadFileMenu()
    {
        try
        {
            var path = EditorUtility.OpenFilePanel("업로드할 파일 선택", "", "*");
            if (string.IsNullOrEmpty(path)) return;
            var service = await CreateDriveService(await GetClientSecretPath());
            var folderId = EditorPrefs.GetString(kFolderIdPrefKey, kDefaultFolderId);
            var mime = GuessMime(Path.GetExtension(path));
            var id = await UploadFile(service, path, folderId, mime);
            LOG.Info("업로드 완료 File ID: " + id);
        }
        catch (Exception ex) { LOG.Error("업로드 실패: " + ex); }
    }

    [MenuItem("/ECO/Build/Upload/Zip Folder And Upload")]
    public static async void ZipFolderAndUploadMenu()
    {
        try
        {
            var folder = EditorUtility.OpenFolderPanel("압축 후 업로드할 폴더 선택", "", "");
            if (string.IsNullOrEmpty(folder)) return;

            var zipOutDir = Path.Combine(Application.dataPath, "../Builds/ZipTemp");
            Directory.CreateDirectory(zipOutDir);
            var zipPath = Path.Combine(zipOutDir, Path.GetFileName(folder) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".zip");

            if (System.IO.File.Exists(zipPath)) System.IO.File.Delete(zipPath);
            ZipFile.CreateFromDirectory(folder, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);

            var service = await CreateDriveService(await GetClientSecretPath());
            var folderId = EditorPrefs.GetString(kFolderIdPrefKey, kDefaultFolderId);
            var id = await UploadFile(service, zipPath, folderId, "application/zip");
            LOG.Info("업로드 완료 File ID: " + id);
        }
        catch (Exception ex) { LOG.Error("업로드 실패: " + ex); }
    }

    private static string GuessMime(string ext)
    {
        ext = (ext ?? "").ToLowerInvariant();
        if (ext == ".zip") return "application/zip";
        if (ext == ".exe") return "application/vnd.microsoft.portable-executable";
        return "application/octet-stream";
    }

    private static async Task<string> GetClientSecretPath()
    {
        var saved = EditorPrefs.GetString(kCredPathPrefKey, "");
        if (!string.IsNullOrEmpty(saved) && System.IO.File.Exists(saved)) return saved;

        string[] candidates = {
            "Assets/Editor/client_secret.json",
            "Assets/Editor/credentials.json",
            "Assets/client_secret.json",
            "client_secret.json"
        };
        foreach (var c in candidates)
            if (System.IO.File.Exists(c)) { EditorPrefs.SetString(kCredPathPrefKey, c); return c; }

        var picked = EditorUtility.OpenFilePanel("Select Google OAuth JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(picked)) throw new FileNotFoundException("OAuth JSON not selected.");
        EditorPrefs.SetString(kCredPathPrefKey, picked);
        return picked;
    }

    private static async Task<DriveService> CreateDriveService(string clientSecretPath)
    {
        Directory.CreateDirectory(kTokenCacheDir);
        using var stream = new FileStream(clientSecretPath, FileMode.Open, FileAccess.Read);
        var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

        var cred = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets,
            new[] { DriveService.Scope.DriveFile },
            "user",
            CancellationToken.None,
            new FileDataStore(kTokenCacheDir, true)
        );

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = cred,
            ApplicationName = "UnityAutoUploader"
        });
    }

    private static async Task<string> UploadFile(DriveService service, string filePath, string folderId, string mime)
    {
        var meta = new DriveFile { Name = Path.GetFileName(filePath) };
        if (!string.IsNullOrEmpty(folderId)) meta.Parents = new[] { folderId };

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var req = service.Files.Create(meta, fs, mime);
        req.Fields = "id, parents";
        req.SupportsAllDrives = true;

        req.ProgressChanged += (IUploadProgress p) =>
        {
            if (p.Status == UploadStatus.Uploading) LOG.Info("업로드 중 " + p.BytesSent + " bytes");
            else if (p.Status == UploadStatus.Completed) LOG.Info("업로드 완료");
            else if (p.Status == UploadStatus.Failed) LOG.Error("업로드 실패: " + p.Exception);
        };

        await req.UploadAsync();
        return req.ResponseBody?.Id;
    }

    private static async Task MakeAnyoneReader(DriveService service, string fileId)
    {
        var permission = new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" };
        var pReq = service.Permissions.Create(permission, fileId);
        pReq.SupportsAllDrives = true;
        await pReq.ExecuteAsync();
    }
}
#endif
