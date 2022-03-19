using UnityEngine;
using UnityEngine.UI;

namespace LostPolygon.uLiveWallpaper.Demos {
    /// <summary>
    /// Makes sure the <see cref="Scrollbar"/> is only active when content overflows the container.
    /// </summary>
    public class ScrollbarEnabler : MonoBehaviour {
        [SerializeField]
        private RectTransform _container;

        [SerializeField]
        private RectTransform _content;

        [SerializeField]
        private Scrollbar _scrollbar;

        private bool _enableScrollbar;

        private void Update() {
            if (_enableScrollbar != _scrollbar.gameObject.activeSelf) {
                _scrollbar.gameObject.SetActive(_enableScrollbar);
            }
        }

        private void OnRectTransformDimensionsChange() {
            _enableScrollbar = _container.rect.height < _content.rect.height;
        }
    }
}