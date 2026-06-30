using UnityEngine;
using UnityEngine.UI;

namespace ADHDTraining.UI
{
    public static class UiSprites
    {
        private static Sprite _white;

        public static Sprite White
        {
            get
            {
                if (_white != null) return _white;

                _white = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                if (_white == null)
                    _white = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");

                if (_white == null)
                {
                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    _white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
                }

                return _white;
            }
        }

        public static void Apply(Image image, Color color)
        {
            image.sprite = White;
            image.type = Image.Type.Simple;
            image.color = color;
        }
    }
}
