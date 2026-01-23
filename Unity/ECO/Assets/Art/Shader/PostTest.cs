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

        if (!playing) return;

        t += Time.deltaTime / duration;
        if (t >= 1f) { t = 1f; playing = false; }

        mat.SetFloat("_T", t);
    }

    public void CallShockWave()
    {
        playing = true;
        t = 0f;

        Vector3 vector3 = Camera.main.WorldToScreenPoint(transform.position);
        Vector4 vector4 = new Vector4(vector3.x/Screen.width, vector3.y/Screen.height, 0, 0);
        Debug.Log(vector4);

        mat.SetVector("_Centre", vector4);
        mat.SetFloat("_T", 0f);
    }
}
