using GreenOne;
using TMPro;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий взаимодействие пользователя с типом <see cref="TableTrait"/>.
    /// </summary>
    public class TableTraitDrawer : Drawer
    {
        public const int WIDTH = 7;
        public const int HEIGHT = 7;

        static readonly GameObject _prefab;

        public readonly new TableTrait attached;
        public readonly SpriteRenderer icon;
        public readonly TextMeshPro textMesh;

        readonly Sprite _normalSprite;

        static TableTraitDrawer()
        {
            _prefab = Resources.Load<GameObject>("Prefabs/Traits/Trait");
        }
        public TableTraitDrawer(TableTrait trait, Transform parent) : base(trait, _prefab, parent)
        {
            attached = trait;
            gameObject.name = trait.Data.ToString();

            _normalSprite = Resources.Load<Sprite>(attached.Data.spritePath);

            icon = transform.Find<SpriteRenderer>("Icon");
            icon.sprite = _normalSprite;

            textMesh = transform.Find<TextMeshPro>("Text");
            textMesh.text = attached.Data.name;

            ChangePointer = ChangePointerBase();
            OnMouseEnter += OnMouseEnterBase;
            OnMouseLeave += OnMouseLeaveBase;
            OnMouseClick += OnMouseClickBase;
        }

        public void RedrawSprite()
        {
            RedrawSprite(_normalSprite);
        }
        public void RedrawSprite(Sprite sprite)
        {
            icon.sprite = sprite;
        }

        protected virtual bool ChangePointerBase() => false;
        protected virtual void OnMouseEnterBase() { }
        protected virtual void OnMouseLeaveBase() { }
        protected virtual void OnMouseClickLeftBase() { }
    }
}
