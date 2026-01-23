using UnityEngine;

public class ShockwaveTestTrigger : MonoBehaviour
{
    public Material mat;
    public float duration = 0.6f;

    float t;
    bool playing;

    void Update()
    {
        if (!mat) return;

        // Z 키로 테스트 트리거
        if (Input.GetKeyDown(KeyCode.Z))
        {
            playing = true;
            t = 0f;

            mat.SetVector("_Centre", new Vector4(0.5f, 0.5f, 0, 0));
            mat.SetFloat("_T", 0f);
        }

        if (!playing) return;

        t += Time.deltaTime / duration;
        if (t >= 1f) { t = 1f; playing = false; }

        mat.SetFloat("_T", t);
    }
}
