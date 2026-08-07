// 볼륨 설정 PlayerPrefs 키의 단일 출처.
// SoundManager(로드)와 UI_SettingsTab_Sound(저장)가 같은 키를 공유하므로 문자열 이중 관리를 막는다.
public static class SoundVolumeKeys
{
    public const string MASTER = "Settings_Sound_Master";
    public const string BGM = "Settings_Sound_Bgm";
    public const string SFX = "Settings_Sound_Sfx";
    public const string VOICE = "Settings_Sound_Voice";
}
