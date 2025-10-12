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

namespace ECO
{
    public static class EditorDriveUploader
    {
        private const string TOKEN_CACHE_DIR_NAME = ".gdrive_token";
        private const string CRED_PATH_PREF_KEY = "DriveUploader.ClientSecretPath";
        private const string FOLDER_ID_PREF_KEY = "DriveUploader.FolderId";
        private const string DEFAULT_FOLDER_ID = "19C_-spQCjMHUcP6FGzDT15R16ugVQksi";

        private static string GetTokenCacheDir()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var userSettings = Path.Combine(projectRoot, "UserSettings");
            return Path.Combine(userSettings, TOKEN_CACHE_DIR_NAME);
        }

        [MenuItem("ECO/Build/Upload/Set Google Drive Folder ID")]
        public static void SetFolderId()
        {
            var current = EditorPrefs.GetString(FOLDER_ID_PREF_KEY, DEFAULT_FOLDER_ID);
            var clicked = EditorUtility.DisplayDialogComplex("Folder ID 설정", "현재: " + current, "변경", "지우기", "닫기");

            if (clicked == 0)
            {
                EditorUtility.DisplayDialog(
                    "안내",
                    "브라우저 주소의 .../folders/<이 값> 을 Project Settings > EditorPrefs 등에 수동 저장하세요.\n또는 별도의 EditorWindow로 입력 UI를 구현하세요.",
                    "확인"
                );
            }
            else if (clicked == 1)
            {
                EditorPrefs.DeleteKey(FOLDER_ID_PREF_KEY);
                Debug.Log("Folder ID 삭제");
            }
        }

        [MenuItem("ECO/Build/Upload/Upload File")]
        public static void UploadFileMenu()
        {
            // 1) 모든 Unity API 호출은 여기서만
            var selectedPath = EditorUtility.OpenFilePanel("업로드할 파일 선택", "", "*");
            if (string.IsNullOrEmpty(selectedPath)) return;

            var credPath = GetClientSecretPathSync();
            var folderId = EditorPrefs.GetString(FOLDER_ID_PREF_KEY, DEFAULT_FOLDER_ID);
            var mime = GuessMime(Path.GetExtension(selectedPath));
            var tokenCacheDir = GetTokenCacheDir();

            // 2) 다음 프레임으로 넘겨 OnGUI/ProcessEvent 루프에서 완전히 빠져나간 뒤 실행
            EditorApplication.delayCall += () =>
            {
                _ = RunUploadFlowAsync(
                    credPath: credPath,
                    tokenCacheDir: tokenCacheDir,
                    filePath: selectedPath,
                    folderId: folderId,
                    mime: mime
                );
            };
        }

        [MenuItem("ECO/Build/Upload/Zip Folder And Upload")]
        public static void ZipFolderAndUploadMenu()
        {
            // 1) Unity API는 즉시 완료
            var folder = EditorUtility.OpenFolderPanel("압축 후 업로드할 폴더 선택", "", "");
            if (string.IsNullOrEmpty(folder)) return;

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var zipOutDir = Path.Combine(projectRoot, "Builds", "ZipTemp");
            Directory.CreateDirectory(zipOutDir);

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            var zipName = Path.GetFileName(folder) + "_" + stamp + ".zip";
            var zipPath = Path.Combine(zipOutDir, zipName);

            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(folder, zipPath, System.IO.Compression.CompressionLevel.Optimal, false);

            var credPath = GetClientSecretPathSync();
            var folderId = EditorPrefs.GetString(FOLDER_ID_PREF_KEY, DEFAULT_FOLDER_ID);
            var tokenCacheDir = GetTokenCacheDir();

            // 2) 다음 프레임에서 비동기 업로드 실행
            EditorApplication.delayCall += () =>
            {
                _ = RunUploadFlowAsync(
                    credPath: credPath,
                    tokenCacheDir: tokenCacheDir,
                    filePath: zipPath,
                    folderId: folderId,
                    mime: "application/zip"
                );
            };
        }

        // OnGUI/메뉴 루프 밖에서만 실행되는 비동기 플로우
        private static async Task RunUploadFlowAsync(string credPath, string tokenCacheDir, string filePath, string folderId, string mime)
        {
            try
            {
                Directory.CreateDirectory(tokenCacheDir);

                // Google 인증 및 서비스 생성 (Unity API 사용 금지)
                using var credStream = new FileStream(credPath, FileMode.Open, FileAccess.Read);
                var secrets = GoogleClientSecrets.FromStream(credStream).Secrets;

                var cred = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new[] { DriveService.Scope.DriveFile },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenCacheDir, true)
                ).ConfigureAwait(false);

                using var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = cred,
                    ApplicationName = "UnityAutoUploader"
                });

                var fileId = await UploadFileAsync(service, filePath, folderId, mime).ConfigureAwait(false);

                // 결과 로그는 메인 스레드에서
                EditorApplication.delayCall += () => Debug.Log("업로드 완료: File ID = " + fileId);
            }
            catch (Exception ex)
            {
                EditorApplication.delayCall += () => Debug.LogError("업로드 실패: " + ex);
            }
        }

        private static string GetClientSecretPathSync()
        {
            var saved = EditorPrefs.GetString(CRED_PATH_PREF_KEY, "");
            if (!string.IsNullOrEmpty(saved) && File.Exists(saved)) return saved;

            string[] candidates =
            {
                "Assets/Editor/client_secret.json",
                "Assets/Editor/credentials.json",
                "Assets/client_secret.json",
                "client_secret.json"
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    EditorPrefs.SetString(CRED_PATH_PREF_KEY, candidate);
                    return candidate;
                }
            }

            var picked = EditorUtility.OpenFilePanel("Select Google OAuth JSON", Application.dataPath, "json");
            if (string.IsNullOrEmpty(picked))
                throw new FileNotFoundException("OAuth JSON not selected.");

            EditorPrefs.SetString(CRED_PATH_PREF_KEY, picked);
            return picked;
        }

        private static string GuessMime(string ext)
        {
            ext = (ext ?? "").ToLowerInvariant();
            if (ext == ".zip") return "application/zip";
            if (ext == ".exe") return "application/vnd.microsoft.portable-executable";
            return "application/octet-stream";
        }

        private static async Task<string> UploadFileAsync(DriveService service, string filePath, string folderId, string mime)
        {
            var meta = new DriveFile { Name = Path.GetFileName(filePath) };
            if (!string.IsNullOrEmpty(folderId)) meta.Parents = new[] { folderId };

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var req = service.Files.Create(meta, fs, mime);
            req.Fields = "id, parents";
            req.SupportsAllDrives = true;

            req.ProgressChanged += (IUploadProgress p) =>
            {
                var msg = p.Status switch
                {
                    UploadStatus.Uploading => $"업로드 중 {p.BytesSent} bytes",
                    UploadStatus.Completed => "업로드 완료",
                    UploadStatus.Failed => "업로드 실패: " + p.Exception,
                    _ => "상태: " + p.Status
                };
                // 진행 콜백도 메인 스레드에서만 로그
                EditorApplication.delayCall += () => Debug.Log(msg);
            };

            await req.UploadAsync().ConfigureAwait(false);
            return req.ResponseBody?.Id ?? "";
        }
    }
}
