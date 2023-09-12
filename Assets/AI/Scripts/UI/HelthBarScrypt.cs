using AI.Units;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HelthBarScrypt : MonoBehaviour
{
    public Image bar;
    public float fill;
    public float fill_max;
    private BotComponent _parent;

    void Awake()
    {
        this.gameObject.SetActive(false);
    }

    void Start()
    {
        _parent = this.GetComponentInParent<BotComponent>();
        fill_max = _parent.GetProperties.Health;
        fill = 1f;

    }

    // Update is called once per frame
    void Update()
    {
        bar.fillAmount = fill;
        fill = _parent.GetProperties.Health/ fill_max;
    }

    public void OpenHelthBar()
    {
        this.gameObject.SetActive(true);
    }
}
