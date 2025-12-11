using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MonkeyInWorldInfoController: MonoBehaviour
    {
        public MonkeyInfoType type;
        public int amount;

        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image image;
        [SerializeField] private Color positiveColor, negativeColor;
        
        [SerializeField] private Sprite heart, bolt;

        [SerializeField] private float lifeTime = 3f;
        
        private float _currentLifeTime;

        private void Start()
        {
            Destroy(gameObject, lifeTime + 1);
            image.sprite = type == MonkeyInfoType.HpLoss ? heart : bolt;
            text.text = type == MonkeyInfoType.HpLoss ? $"-{amount}" : $"+{amount}";
            text.color = type == MonkeyInfoType.AVGain ? positiveColor : negativeColor;
            _currentLifeTime = lifeTime;
        }

        private void FixedUpdate()
        {
            if (_currentLifeTime <= 0) return;
            
            transform.Translate(Vector3.up * Time.deltaTime);
            transform.localScale = Vector3.one * (_currentLifeTime / 3f);
            _currentLifeTime -= Time.deltaTime;
        }
    }

    public enum MonkeyInfoType
    {
        HpLoss,
        AVGain
    }
}