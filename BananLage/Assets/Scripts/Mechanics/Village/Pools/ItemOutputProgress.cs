using Mechanics.Production;
using UnityEngine;
using UnityEngine.UI;

namespace Mechanics.Village.Pools
{
    public class ItemOutputProgress : MonoBehaviour
    {
        private ResourceOutput _resource;
        public ResourceOutput Resource
        {
            get => _resource;
            set
            {
                hasUpdated = value != null;
                _resource = value;
            }
        }
        private bool hasUpdated = false;

        [SerializeField] private Image  _image;
        [SerializeField] private RectMask2D _mask;
        
        private void Update()
        {
            if (hasUpdated && _resource != null) SetSprite();

            if (_resource == null) return;
            _mask.padding = new Vector4(0, 1 - _resource.Progress, 1 - _resource.Progress, 0);
        }

        private void SetSprite()
        {
            hasUpdated = false;
            if (!_image) return;
            _mask.padding = Vector4.one;
            _image.sprite = _resource.output.uiSprite;
        }
    }
}