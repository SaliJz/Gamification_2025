using UnityEngine;
using UnityEngine.Playables;

public class PauseSensitiveAnimator : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private PlayableDirector director; // opcional, si usas Timeline
    [SerializeField] private Renderer rend; // para detectar si el material podría animarse por shader

    private void Awake()
    {
        anim = GetComponent<Animator>();
        // fuerza modo normal para que dependa de timeScale
        anim.updateMode = AnimatorUpdateMode.Normal;
        director = GetComponent<PlayableDirector>();
        rend = GetComponent<Renderer>();
    }

    public void ApplyPause(bool paused)
    {
        // 1) Animator: velocidad 0 o 1
        anim.speed = paused ? 0f : 1f;

        // 2) Si hay Timeline/PlayableDirector, pausar
        if (director != null)
        {
            if (paused && director.state == PlayState.Playing) director.Pause();
            if (!paused && director.state != PlayState.Playing) director.Play();
        }

        // 3) Si el sprite se anima por shader (offset en material), detener manualmente
        // esto solo cubre casos simples donde el material usa una propiedad de tiempo que puedas controlar.
        if (rend != null && rend.material != null)
        {
            if (paused)
            {
                // guarda y forzar propiedad si tu shader soporta un factor de tiempo
                // ejemplo hipotético: rend.material.SetFloat("_TimeScale", 0f);
            }
            else
            {
                // rend.material.SetFloat("_TimeScale", 1f);
            }
        }

        Debug.Log($"[PauseSensitiveAnimator] Paused: {paused} | Animator Speed: {anim.speed}");
    }
}
